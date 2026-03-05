namespace AcadSign.Backend.Domain.Entities;

public class DataDeletionRequest
{
    public Guid Id { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public Guid RequestedBy { get; set; }
    public DateTime RequestedAt { get; set; }
    public DeletionRequestStatus Status { get; set; }
    public string Reason { get; set; } = string.Empty;
    public Guid? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewNotes { get; set; }
}

public enum DeletionRequestStatus
{
    Pending,
    Approved,
    Rejected,
    Completed
}
