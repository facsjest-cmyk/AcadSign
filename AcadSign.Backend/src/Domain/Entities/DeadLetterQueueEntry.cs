namespace AcadSign.Backend.Domain.Entities;

public class DeadLetterQueueEntry
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public string? JobId { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public int RetryCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastRetryAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public Guid? ResolvedBy { get; set; }
}
