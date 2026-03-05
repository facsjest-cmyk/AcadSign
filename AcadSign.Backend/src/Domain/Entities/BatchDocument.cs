namespace AcadSign.Backend.Domain.Entities;

public class BatchDocument
{
    public Guid Id { get; set; }
    public Guid BatchId { get; set; }
    public Batch Batch { get; set; } = null!;
    public Guid? DocumentId { get; set; }
    public Document? Document { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public DocumentStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}
