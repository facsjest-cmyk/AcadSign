namespace AcadSign.Backend.Domain.Entities;

public class Batch
{
    public Guid Id { get; set; }
    public int TotalDocuments { get; set; }
    public int ProcessedDocuments { get; set; }
    public int FailedDocuments { get; set; }
    public BatchStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public Guid CreatedBy { get; set; }
    
    public ICollection<BatchDocument> Documents { get; set; } = new List<BatchDocument>();
}

public enum BatchStatus
{
    PENDING,
    PROCESSING,
    COMPLETED,
    PARTIAL,
    FAILED
}
