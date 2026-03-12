using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using AcadSign.Backend.Application.Services;
using AcadSign.Backend.Application.Common.Interfaces;

namespace AcadSign.Backend.Application.BackgroundJobs;

public class EmailNotificationJob
{
    private readonly IEmailService _emailService;
    private readonly IDocumentRepository _documentRepo;
    private readonly IStudentRepository _studentRepo;
    private readonly IStorageService _storageService;
    private readonly DeadLetterQueueService _dlqService;
    private readonly ILogger<EmailNotificationJob> _logger;
    
    public EmailNotificationJob(
        IEmailService emailService,
        IDocumentRepository documentRepo,
        IStudentRepository studentRepo,
        IStorageService storageService,
        DeadLetterQueueService dlqService,
        ILogger<EmailNotificationJob> logger)
    {
        _emailService = emailService;
        _documentRepo = documentRepo;
        _studentRepo = studentRepo;
        _storageService = storageService;
        _dlqService = dlqService;
        _logger = logger;
    }
    
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 300, 900 })]
    public async Task SendDocumentReadyEmailAsync(Guid documentId, PerformContext? context = null)
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
            
            var student = await _studentRepo.GetByIdAsync(document.StudentId);

            // Dev-friendly fallback: many flows currently store Guid.Empty for StudentId
            if (student == null && document.StudentId == Guid.Empty)
            {
                student = await _studentRepo.GetByIdAsync(Guid.Empty);
            }

            if (student == null || string.IsNullOrEmpty(student.Email))
            {
                _logger.LogWarning("Student {StudentId} has no email", document.StudentId);
                return;
            }
            
            var downloadUrl = await _storageService.GeneratePreSignedUrlAsync(
                document.S3ObjectPath,
                TimeSpan.FromHours(24));
            
            await _emailService.SendDocumentReadyEmailAsync(
                toEmail: student.Email,
                studentName: $"{student.FirstName} {student.LastName}",
                document: new DocumentMetadata
                {
                    DocumentId = documentId,
                    DocumentType = document.DocumentType ?? "Unknown",
                    IssuedDate = document.Created.DateTime,
                    ExpiryDate = DateTime.UtcNow.AddHours(24)
                },
                downloadUrl: downloadUrl,
                language: "fr");
            
            _logger.LogInformation("Email sent successfully for document {DocumentId}", documentId);
        }
        catch (Exception ex)
        {
            var retryCount = 0;
            try
            {
                retryCount = context?.GetJobParameter<int>("RetryCount") ?? 0;
            }
            catch
            {
                // ignore
            }

            if (retryCount >= 2)
            {
                await _dlqService.MoveToDeadLetterQueueAsync(documentId, ex, context?.BackgroundJob?.Id);
            }

            _logger.LogError(ex, "Failed to send email for document {DocumentId}", documentId);
            throw;
        }
    }
}
