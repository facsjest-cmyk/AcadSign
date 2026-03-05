using System.Security.Cryptography.X509Certificates;

namespace AcadSign.Backend.Application.Services;

public interface ISignatureVerificationService
{
    Task<SignatureVerificationResult> VerifySignatureAsync(byte[] signedPdf);
    Task<CertificateValidationResult> ValidateCertificateStatusAsync(X509Certificate2 certificate);
}
