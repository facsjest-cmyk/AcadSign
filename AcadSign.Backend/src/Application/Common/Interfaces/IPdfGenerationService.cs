using AcadSign.Backend.Application.Common.Models;

namespace AcadSign.Backend.Application.Common.Interfaces;

public interface IPdfGenerationService
{
    Task<byte[]> GenerateDocumentAsync(DocumentType type, StudentData data);
}
