using AcadSign.Backend.Application.Common.Models;
using AcadSign.Backend.Domain.Enums;

namespace AcadSign.Backend.Application.Common.Interfaces;

public interface IPdfGenerationService
{
    Task<byte[]> GenerateDocumentAsync(DocumentType type, StudentData data);
}
