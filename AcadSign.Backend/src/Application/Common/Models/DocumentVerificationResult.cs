namespace AcadSign.Backend.Application.Common.Models;

public class DocumentVerificationResult
{
    public Guid DocumentId { get; set; }
    public bool IsValid { get; set; }
    public string? DocumentType { get; set; }
    public string? IssuedBy { get; set; }
    public string? StudentName { get; set; }
    public DateTime? SignedAt { get; set; }

    public string? CertificateSerial { get; set; }
    public string? CertificateStatus { get; set; }
    public DateTime? CertificateValidUntil { get; set; }
    public string? CertificateIssuer { get; set; }

    public string? SignatureAlgorithm { get; set; }
    public string? TimestampAuthority { get; set; }

    public string? Error { get; set; }
    public string? Reason { get; set; }
    public DateTime? RevokedAt { get; set; }
}
