using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AcadSign.Backend.Application.Services;
using AcadSign.Backend.Domain.Entities;
using System.Text.Json;

namespace AcadSign.Backend.Web.Controllers;

[ApiController]
[Route("api/v1/audit")]
[Authorize(Roles = "Auditor,Administrator")]
public class AuditController : ControllerBase
{
    private readonly IAuditLogRepository _auditRepo;
    private readonly ILogger<AuditController> _logger;
    
    public AuditController(
        IAuditLogRepository auditRepo,
        ILogger<AuditController> logger)
    {
        _auditRepo = auditRepo;
        _logger = logger;
    }
    
    [HttpGet("{documentId}")]
    [ProducesResponseType(typeof(AuditTrailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDocumentAuditTrail(Guid documentId)
    {
        var logs = await _auditRepo.GetByDocumentIdAsync(documentId);
        
        if (!logs.Any())
        {
            return NotFound(new { error = "No audit logs found for this document" });
        }
        
        var response = new AuditTrailResponse
        {
            DocumentId = documentId,
            Events = logs.Select(log => new AuditEventDto
            {
                EventType = log.EventType.ToString(),
                Timestamp = log.CreatedAt,
                UserId = log.UserId,
                IpAddress = log.IpAddress,
                UserAgent = log.UserAgent,
                CertificateSerial = log.CertificateSerial,
                Metadata = log.Metadata != null 
                    ? JsonSerializer.Deserialize<Dictionary<string, object>>(log.Metadata) 
                    : null
            }).ToList(),
            TotalEvents = logs.Count
        };
        
        return Ok(response);
    }
    
    [HttpGet("search")]
    [ProducesResponseType(typeof(AuditSearchResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchAuditLogs([FromQuery] AuditSearchRequest request)
    {
        var logs = await _auditRepo.SearchAsync(
            eventType: request.EventType,
            startDate: request.StartDate,
            endDate: request.EndDate,
            userId: request.UserId,
            documentId: request.DocumentId,
            limit: request.Limit ?? 100,
            offset: request.Offset ?? 0);
        
        var totalCount = await _auditRepo.CountAsync(
            eventType: request.EventType,
            startDate: request.StartDate,
            endDate: request.EndDate,
            userId: request.UserId,
            documentId: request.DocumentId);
        
        var response = new AuditSearchResponse
        {
            Events = logs.Select(log => new AuditEventDto
            {
                Id = log.Id,
                DocumentId = log.DocumentId,
                EventType = log.EventType.ToString(),
                Timestamp = log.CreatedAt,
                UserId = log.UserId,
                IpAddress = log.IpAddress,
                Metadata = log.Metadata != null 
                    ? JsonSerializer.Deserialize<Dictionary<string, object>>(log.Metadata) 
                    : null
            }).ToList(),
            TotalCount = totalCount,
            Limit = request.Limit ?? 100,
            Offset = request.Offset ?? 0
        };
        
        return Ok(response);
    }
}

public class AuditTrailResponse
{
    public Guid DocumentId { get; set; }
    public List<AuditEventDto> Events { get; set; } = new();
    public int TotalEvents { get; set; }
}

public class AuditEventDto
{
    public Guid? Id { get; set; }
    public Guid? DocumentId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Guid? UserId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? CertificateSerial { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public class AuditSearchRequest
{
    public string? EventType { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? UserId { get; set; }
    public Guid? DocumentId { get; set; }
    public int? Limit { get; set; }
    public int? Offset { get; set; }
}

public class AuditSearchResponse
{
    public List<AuditEventDto> Events { get; set; } = new();
    public int TotalCount { get; set; }
    public int Limit { get; set; }
    public int Offset { get; set; }
}
