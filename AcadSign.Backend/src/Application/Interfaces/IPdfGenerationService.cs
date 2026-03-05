namespace AcadSign.Backend.Application.Services;

public interface IPdfGenerationService
{
    Task<byte[]> GenerateDocumentAsync(DocumentType type, object studentData);
}
