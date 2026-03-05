using Hangfire;
using Microsoft.Extensions.Logging;
using AcadSign.Backend.Application.Services;
using AcadSign.Backend.Domain.Entities;

namespace AcadSign.Backend.Application.BackgroundJobs;

public class DocumentGenerationJob : BaseJob
{
    private readonly IPdfGenerationService _pdfService;
    private readonly IS3StorageService _storageService;
    
    public DocumentGenerationJob(
        IPdfGenerationService pdfService,
        IS3StorageService storageService,
        ILogger<DocumentGenerationJob> logger) : base(logger)
    {
        _pdfService = pdfService;
        _storageService = storageService;
    }
    
    [AutomaticRetry(Attempts = 5)]
    public async Task GenerateDocumentAsync(Guid documentId, DocumentType documentType, object studentData)
    {
        try
        {
            _logger.LogInformation("Starting document generation for {DocumentId}", documentId);
            
            var pdfBytes = await _pdfService.GenerateDocumentAsync(documentType, studentData);
            
            await _storageService.UploadDocumentAsync(pdfBytes, $"{documentId}.pdf");
            
            _logger.LogInformation("Document {DocumentId} generated successfully", documentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate document {DocumentId}", documentId);
            throw;
        }
    }
}
