using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AcadSign.Backend.Application.Services;
using AcadSign.Backend.Application.Common.Interfaces;
using AcadSign.Backend.Domain.Entities;
using System.Security.Claims;

namespace AcadSign.Backend.Web.Controllers;

[ApiController]
[Route("api/v1/students")]
[Authorize]
public class StudentDataController : ControllerBase
{
    private readonly IStudentRepository _studentRepo;
    private readonly IDocumentRepository _documentRepo;
    private readonly IDataDeletionRequestRepository _deletionRequestRepo;
    private readonly IAuditLogService _auditService;
    private readonly ILogger<StudentDataController> _logger;
    
    public StudentDataController(
        IStudentRepository studentRepo,
        IDocumentRepository documentRepo,
        IDataDeletionRequestRepository deletionRequestRepo,
        IAuditLogService auditService,
        ILogger<StudentDataController> logger)
    {
        _studentRepo = studentRepo;
        _documentRepo = documentRepo;
        _deletionRequestRepo = deletionRequestRepo;
        _auditService = auditService;
        _logger = logger;
    }
    
    [HttpGet("{studentId}/data")]
    [ProducesResponseType(typeof(StudentDataResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetStudentData(string studentId)
    {
        if (!IsAuthorizedToAccessStudentData(studentId))
        {
            return Forbid();
        }
        
        var student = await _studentRepo.GetByStudentIdAsync(studentId);
        if (student == null)
        {
            return NotFound();
        }
        
        var studentGuid = Guid.TryParse(studentId, out var guid) ? guid : Guid.Empty;
        var documents = await _documentRepo.GetByStudentIdAsync(studentGuid);
        
        var response = new StudentDataResponse
        {
            StudentId = student.Id.ToString(),
            FirstName = student.FirstName,
            LastName = student.LastName,
            CIN = student.CIN,
            CNE = student.CNE,
            Email = student.Email,
            PhoneNumber = student.PhoneNumber,
            DateOfBirth = student.DateOfBirth,
            Documents = documents.Select(d => new StudentDocumentDto
            {
                DocumentId = Guid.NewGuid(), // TODO: Utiliser un vrai mapping
                DocumentType = d.DocumentType ?? "Unknown",
                CreatedAt = d.Created.DateTime,
                Status = d.Status ?? "Unknown"
            }).ToList(),
            DataCollectedAt = DateTime.UtcNow,
            DataRetentionUntil = DateTime.UtcNow.AddYears(30)
        };
        
        await _auditService.LogEventAsync(AuditEventType.USER_LOGIN, null, new
        {
            studentId = studentId,
            requestedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        });
        
        return Ok(response);
    }
    
    [HttpPut("{studentId}/data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateStudentData(
        string studentId, 
        [FromBody] UpdateStudentDataRequest request)
    {
        if (!IsAuthorizedToAccessStudentData(studentId))
        {
            return Forbid();
        }
        
        var student = await _studentRepo.GetByStudentIdAsync(studentId);
        if (student == null)
        {
            return NotFound();
        }
        
        if (!string.IsNullOrEmpty(request.Email))
        {
            student.Email = request.Email;
        }
        
        if (!string.IsNullOrEmpty(request.PhoneNumber))
        {
            student.PhoneNumber = request.PhoneNumber;
        }
        
        await _studentRepo.UpdateAsync(student);
        
        await _auditService.LogEventAsync(AuditEventType.USER_LOGIN, null, new
        {
            studentId = studentId,
            updatedFields = new[] { 
                !string.IsNullOrEmpty(request.Email) ? "email" : null,
                !string.IsNullOrEmpty(request.PhoneNumber) ? "phoneNumber" : null
            }.Where(f => f != null)
        });
        
        return Ok(new { message = "Données mises à jour avec succès" });
    }
    
    [HttpDelete("{studentId}/data")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RequestDataDeletion(string studentId)
    {
        if (!IsAuthorizedToAccessStudentData(studentId))
        {
            return Forbid();
        }
        
        var student = await _studentRepo.GetByStudentIdAsync(studentId);
        if (student == null)
        {
            return NotFound();
        }
        
        var deletionRequest = new DataDeletionRequest
        {
            Id = Guid.NewGuid(),
            StudentId = studentId,
            RequestedBy = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value!),
            RequestedAt = DateTime.UtcNow,
            Status = DeletionRequestStatus.Pending,
            Reason = "Student request for data erasure (CNDP Loi 53-05)"
        };
        
        await _deletionRequestRepo.AddAsync(deletionRequest);
        
        await _auditService.LogEventAsync(AuditEventType.USER_LOGOUT, null, new
        {
            studentId = studentId,
            requestId = deletionRequest.Id
        });
        
        return Accepted(new
        {
            message = "Demande de suppression enregistrée. Un administrateur examinera votre demande.",
            requestId = deletionRequest.Id,
            status = "PENDING",
            note = "Les documents académiques ne peuvent pas être supprimés (rétention légale 30 ans)"
        });
    }
    
    private bool IsAuthorizedToAccessStudentData(string studentId)
    {
        var userStudentId = User.FindFirst("student_id")?.Value;
        var isAdmin = User.IsInRole("Administrator");
        
        return userStudentId == studentId || isAdmin;
    }
}

public class StudentDataResponse
{
    public string StudentId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string CIN { get; set; } = string.Empty;
    public string CNE { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public List<StudentDocumentDto> Documents { get; set; } = new();
    public DateTime DataCollectedAt { get; set; }
    public DateTime DataRetentionUntil { get; set; }
}

public class StudentDocumentDto
{
    public Guid DocumentId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class UpdateStudentDataRequest
{
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
}
