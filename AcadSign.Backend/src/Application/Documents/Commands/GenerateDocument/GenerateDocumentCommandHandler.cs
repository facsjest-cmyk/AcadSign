using AcadSign.Backend.Application.Common.Interfaces;

namespace AcadSign.Backend.Application.Documents.Commands.GenerateDocument;

public class GenerateDocumentCommandHandler : IRequestHandler<GenerateDocumentCommand, GenerateDocumentResponse>
{
    private readonly IPdfGenerationService _pdfService;
    private readonly ILogger<GenerateDocumentCommandHandler> _logger;

    public GenerateDocumentCommandHandler(
        IPdfGenerationService pdfService,
        ILogger<GenerateDocumentCommandHandler> logger)
    {
        _pdfService = pdfService;
        _logger = logger;
    }

    public async Task<GenerateDocumentResponse> Handle(GenerateDocumentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating document of type {DocumentType} for student {StudentId}", 
            request.DocumentType, request.StudentId);

        // Générer UUID v4 (FR3)
        var documentId = Guid.NewGuid();
        request.StudentData.DocumentId = documentId;

        // Générer le PDF
        var pdfBytes = await _pdfService.GenerateDocumentAsync(
            request.DocumentType,
            request.StudentData);

        _logger.LogInformation("Document {DocumentId} generated successfully. Size: {Size} bytes", 
            documentId, pdfBytes.Length);

        // Note: Le stockage en DB et MinIO sera implémenté dans les stories suivantes
        // Pour l'instant, on retourne juste la réponse avec l'ID

        return new GenerateDocumentResponse
        {
            DocumentId = documentId,
            Status = "UNSIGNED",
            UnsignedPdfUrl = $"/api/v1/documents/{documentId}/unsigned",
            CreatedAt = DateTime.UtcNow
        };
    }
}
