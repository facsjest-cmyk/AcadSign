using AcadSign.Backend.Application.Common.Models;

namespace AcadSign.Backend.Application.Documents.Commands.GenerateDocument;

public record GenerateDocumentCommand : IRequest<GenerateDocumentResponse>
{
    public DocumentType DocumentType { get; init; }
    public Guid StudentId { get; init; }
    public StudentData StudentData { get; init; } = null!;
}

public record GenerateDocumentResponse
{
    public Guid DocumentId { get; init; }
    public string Status { get; init; } = string.Empty;
    public string UnsignedPdfUrl { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
