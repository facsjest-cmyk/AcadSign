using System.Security.Cryptography.X509Certificates;
using System;
using System.Threading.Tasks;

namespace AcadSign.Desktop.Services.Validation;

public interface ICertificateValidationService
{
    Task<CertificateValidationResult> ValidateCertificateAsync(X509Certificate2 cert);
}

public class CertificateValidationResult
{
    public CertificateStatus Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime? RevocationDate { get; set; }
    public ValidationMethod Method { get; set; }
}

public enum CertificateStatus
{
    Valid,
    Revoked,
    Expired,
    Unknown
}

public enum ValidationMethod
{
    OCSP,
    CRL,
    None
}
