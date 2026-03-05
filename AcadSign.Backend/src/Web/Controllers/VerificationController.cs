using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using AcadSign.Backend.Application.Services;

namespace AcadSign.Backend.Web.Controllers;

[ApiController]
[Route("api/v1/documents")]
public class VerificationController : ControllerBase
{
    private readonly IS3StorageService _storageService;
    private readonly ISignatureVerificationService _verificationService;
    private readonly IDocumentRepository _documentRepo;
    private readonly ILogger<VerificationController> _logger;
    
    public VerificationController(
        IS3StorageService storageService,
        ISignatureVerificationService verificationService,
        IDocumentRepository documentRepo,
        ILogger<VerificationController> logger)
    {
        _storageService = storageService;
        _verificationService = verificationService;
        _documentRepo = documentRepo;
        _logger = logger;
    }
    
    [HttpGet("verify/{documentId}")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyDocument(Guid documentId)
    {
        try
        {
            var document = await _documentRepo.GetByIdAsync(documentId);
            if (document == null)
            {
                return Ok(new VerificationResponse
                {
                    IsValid = false,
                    Error = "Document not found"
                });
            }
            
            var signedPdf = await _storageService.DownloadDocumentAsync(documentId.ToString());
            
            var verificationResult = await _verificationService.VerifySignatureAsync(signedPdf);
            
            if (!verificationResult.IsValid)
            {
                return Ok(new VerificationResponse
                {
                    IsValid = false,
                    Error = verificationResult.ErrorMessage,
                    Reason = verificationResult.FailureReason
                });
            }
            
            var certStatus = await _verificationService.ValidateCertificateStatusAsync(
                verificationResult.Certificate);
            
            if (certStatus.Status == CertificateStatus.Revoked)
            {
                return Ok(new VerificationResponse
                {
                    IsValid = false,
                    CertificateStatus = "REVOKED",
                    RevokedAt = certStatus.RevokedAt,
                    Error = "Certificate has been revoked"
                });
            }
            
            return Ok(new VerificationResponse
            {
                DocumentId = documentId,
                IsValid = true,
                DocumentType = document.Type?.ToString() ?? "Unknown",
                IssuedBy = "Université Hassan II Casablanca",
                StudentName = "Student Name",
                SignedAt = document.SignedAt,
                CertificateSerial = verificationResult.CertificateSerial,
                CertificateStatus = "VALID",
                CertificateValidUntil = verificationResult.CertificateValidUntil,
                CertificateIssuer = verificationResult.CertificateIssuer,
                SignatureAlgorithm = verificationResult.SignatureAlgorithm,
                TimestampAuthority = verificationResult.TimestampAuthority
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying document {DocumentId}", documentId);
            return Ok(new VerificationResponse
            {
                IsValid = false,
                Error = "Error during verification"
            });
        }
    }
}

public class VerificationResponse
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

public enum CertificateStatus
{
    Valid,
    Revoked,
    Expired,
    Unknown
}
