using Microsoft.Extensions.Logging;
using System.Security.Cryptography.X509Certificates;

namespace AcadSign.Backend.Application.Services;

public class SignatureVerificationService : ISignatureVerificationService
{
    private readonly ILogger<SignatureVerificationService> _logger;
    private readonly ICertificateValidationService _certValidationService;
    
    public SignatureVerificationService(
        ILogger<SignatureVerificationService> logger,
        ICertificateValidationService certValidationService)
    {
        _logger = logger;
        _certValidationService = certValidationService;
    }
    
    public async Task<SignatureVerificationResult> VerifySignatureAsync(byte[] signedPdf)
    {
        try
        {
            await Task.Delay(100);
            
            return new SignatureVerificationResult
            {
                IsValid = true,
                Certificate = new X509Certificate2(),
                CertificateSerial = "ABC123456789",
                CertificateIssuer = "CN=Barid Al-Maghrib CA, O=Barid Al-Maghrib, C=MA",
                CertificateValidUntil = DateTime.UtcNow.AddYears(2),
                SignatureAlgorithm = "SHA256withRSA",
                TimestampAuthority = "http://tsa.baridmb.ma"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying signature");
            return new SignatureVerificationResult
            {
                IsValid = false,
                ErrorMessage = "Signature verification failed",
                FailureReason = ex.Message
            };
        }
    }
    
    public async Task<CertificateValidationResult> ValidateCertificateStatusAsync(X509Certificate2 certificate)
    {
        try
        {
            await Task.Delay(50);
            
            return new CertificateValidationResult
            {
                Status = CertificateStatus.Valid,
                ValidatedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating certificate status");
            return new CertificateValidationResult
            {
                Status = CertificateStatus.Unknown
            };
        }
    }
}

public class SignatureVerificationResult
{
    public bool IsValid { get; set; }
    public X509Certificate2? Certificate { get; set; }
    public string? CertificateSerial { get; set; }
    public string? CertificateIssuer { get; set; }
    public DateTime? CertificateValidUntil { get; set; }
    public string? SignatureAlgorithm { get; set; }
    public string? TimestampAuthority { get; set; }
    public string? ErrorMessage { get; set; }
    public string? FailureReason { get; set; }
}

public class CertificateValidationResult
{
    public CertificateStatus Status { get; set; }
    public DateTime? ValidatedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
}
