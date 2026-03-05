using System.Text.Json;

namespace AcadSign.Backend.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; set; }
    public Guid? DocumentId { get; set; }
    public AuditEventType EventType { get; set; }
    public Guid? UserId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? CertificateSerial { get; set; }
    public JsonDocument? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CorrelationId { get; set; }
}

public enum AuditEventType
{
    DOCUMENT_GENERATED,
    DOCUMENT_SIGNED,
    DOCUMENT_UPLOADED,
    DOCUMENT_DOWNLOADED,
    DOCUMENT_VERIFIED,
    CERTIFICATE_VALIDATED,
    TEMPLATE_UPLOADED,
    USER_LOGIN,
    USER_LOGOUT,
    BATCH_CREATED,
    BATCH_COMPLETED,
    WEBHOOK_TRIGGERED,
    EMAIL_SENT
}
