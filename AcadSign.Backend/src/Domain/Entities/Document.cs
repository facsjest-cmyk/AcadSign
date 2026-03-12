using AcadSign.Backend.Domain.Common;

namespace AcadSign.Backend.Domain.Entities;

public class Document : BaseAuditableEntity
{
    public Guid PublicId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public Guid StudentId { get; set; }
    public string Status { get; set; } = "UNSIGNED";
    public string S3ObjectPath { get; set; } = string.Empty;
    public DateTime? SignedAt { get; set; }
    public string? SignerName { get; set; }
    public string? SignatureData { get; set; }
}
