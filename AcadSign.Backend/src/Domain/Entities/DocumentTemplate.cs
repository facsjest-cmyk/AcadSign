using AcadSign.Backend.Application.Common.Models;

namespace AcadSign.Backend.Domain.Entities;

public class DocumentTemplate
{
    public Guid Id { get; set; }
    public DocumentType Type { get; set; }
    public string InstitutionId { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public byte[] TemplateData { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? Description { get; set; }
}
