using Hangfire;
using Microsoft.Extensions.Logging;
using AcadSign.Backend.Application.Services;

namespace AcadSign.Backend.Application.BackgroundJobs;

public class EmailNotificationJob
{
    private readonly IEmailService _emailService;
    private readonly IDocumentRepository _documentRepo;
    private readonly IStudentRepository _studentRepo;
    private readonly IStorageService _storageService;
    private readonly ILogger<EmailNotificationJob> _logger;
    
    public EmailNotificationJob(
        IEmailService emailService,
        IDocumentRepository documentRepo,
        IStudentRepository studentRepo,
        IStorageService storageService,
        ILogger<EmailNotificationJob> logger)
    {
        _emailService = emailService;
        _documentRepo = documentRepo;
        _studentRepo = studentRepo;
        _storageService = storageService;
        _logger = logger;
    }
    
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 300, 900 })]
    public async Task SendDocumentReadyEmailAsync(Guid documentId)
    {
        try
        {
            _logger.LogInformation("Sending email notification for document {DocumentId}", documentId);
            
            var document = await _documentRepo.GetByIdAsync(documentId);
            if (document == null)
            {
                _logger.LogWarning("Document {DocumentId} not found", documentId);
                return;
            }
            
            var student = await _studentRepo.GetByStudentIdAsync(document.StudentId);
            if (student == null || string.IsNullOrEmpty(student.Email))
            {
                _logger.LogWarning("Student {StudentId} has no email", document.StudentId);
                return;
            }
            
            var downloadUrl = await _storageService.GeneratePreSignedUrlAsync(
                documentId.ToString(),
                TimeSpan.FromHours(24));
            
            await _emailService.SendDocumentReadyEmailAsync(
                toEmail: student.Email,
                studentName: $"{student.FirstName} {student.LastName}",
                document: new DocumentMetadata
                {
                    DocumentId = documentId,
                    DocumentType = document.Type?.ToString() ?? "Unknown",
                    IssuedDate = document.CreatedAt,
                    ExpiryDate = DateTime.UtcNow.AddHours(24)
                },
                downloadUrl: downloadUrl,
                language: student.PreferredLanguage ?? "fr");
            
            _logger.LogInformation("Email sent successfully for document {DocumentId}", documentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email for document {DocumentId}", documentId);
            throw;
        }
    }
}
