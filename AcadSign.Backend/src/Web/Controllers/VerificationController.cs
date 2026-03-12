using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using AcadSign.Backend.Application.Services;
using AcadSign.Backend.Application.Common.Interfaces;
using AcadSign.Backend.Application.Common.Models;
using AcadSign.Backend.Domain.Entities;
using Minio.Exceptions;

namespace AcadSign.Backend.Web.Controllers;

[ApiController]
[Route("api/v1/documents")]
public class VerificationController : ControllerBase
{
    private readonly IS3StorageService _storageService;
    private readonly ISignatureVerificationService _verificationService;
    private readonly IVerificationReportService _reportService;
    private readonly IDocumentRepository _documentRepo;
    private readonly IAuditLogService _auditService;
    private readonly ILogger<VerificationController> _logger;
    
    public VerificationController(
        IS3StorageService storageService,
        ISignatureVerificationService verificationService,
        IVerificationReportService reportService,
        IDocumentRepository documentRepo,
        IAuditLogService auditService,
        ILogger<VerificationController> logger)
    {
        _storageService = storageService;
        _verificationService = verificationService;
        _reportService = reportService;
        _documentRepo = documentRepo;
        _auditService = auditService;
        _logger = logger;
    }
    
    [HttpGet("verify/{documentId}")]
    [AllowAnonymous]
    [EnableRateLimiting("verification")]
    public async Task<IActionResult> VerifyDocument(Guid documentId)
    {
        var (result, _) = await VerifyDocumentCore(documentId, logAudit: true);
        return result;
    }

    [HttpGet("verify/{documentId}/report")]
    [AllowAnonymous]
    [EnableRateLimiting("verification")]
    public async Task<IActionResult> DownloadVerificationReport(Guid documentId, CancellationToken cancellationToken)
    {
        var (verifyResult, verification) = await VerifyDocumentCore(documentId, logAudit: false);

        if (verifyResult is NotFoundObjectResult notFound)
        {
            return notFound;
        }

        if (verifyResult is ObjectResult && verification != null)
        {
            var pdf = await _reportService.GenerateVerificationReportAsync(verification, cancellationToken);
            return File(pdf, "application/pdf", $"verification-report-{documentId}.pdf");
        }

        if (verifyResult is ObjectResult errorResult)
        {
            return errorResult;
        }

        return StatusCode(500, new { error = "Internal server error" });
    }

    private async Task<(IActionResult Result, DocumentVerificationResult? Verification)> VerifyDocumentCore(Guid documentId, bool logAudit)
    {
        try
        {
            var document = await _documentRepo.GetByIdAsync(documentId);
            if (document == null)
            {
                return (NotFound(new { error = "Document not found" }), null);
            }

            _logger.LogInformation("Downloading signed PDF for {DocumentId} from {S3ObjectPath}", documentId, document.S3ObjectPath);

            byte[] signedPdf;
            try
            {
                signedPdf = await _storageService.DownloadDocumentAsync(document.S3ObjectPath);
            }
            catch (ObjectNotFoundException)
            {
                return (NotFound(new { error = "Signed document file not found" }), null);
            }

            var verificationResult = await _verificationService.VerifySignatureAsync(signedPdf);

            if (!verificationResult.IsValid)
            {
                var invalid = new DocumentVerificationResult
                {
                    DocumentId = documentId,
                    IsValid = false,
                    Error = verificationResult.ErrorMessage,
                    Reason = verificationResult.FailureReason
                };

                if (logAudit)
                {
                    await _auditService.LogEventAsync(AuditEventType.DOCUMENT_VERIFIED, documentId, new
                    {
                        isValid = false,
                        reason = verificationResult.FailureReason,
                        error = verificationResult.ErrorMessage
                    });
                }

                return (Ok(invalid), invalid);
            }

            var certStatus = verificationResult.Certificate != null
                ? await _verificationService.ValidateCertificateStatusAsync(verificationResult.Certificate)
                : null;

            if (certStatus?.Status == Domain.Enums.CertificateStatus.Revoked)
            {
                var revoked = new DocumentVerificationResult
                {
                    DocumentId = documentId,
                    IsValid = false,
                    CertificateStatus = "REVOKED",
                    RevokedAt = certStatus.RevokedAt,
                    Error = "Certificate has been revoked"
                };

                if (logAudit)
                {
                    await _auditService.LogEventAsync(AuditEventType.DOCUMENT_VERIFIED, documentId, new
                    {
                        isValid = false,
                        certificateStatus = "REVOKED"
                    });
                }

                return (Ok(revoked), revoked);
            }

            var valid = new DocumentVerificationResult
            {
                DocumentId = documentId,
                IsValid = true,
                DocumentType = document.DocumentType,
                IssuedBy = "Université Hassan II Casablanca",
                StudentName = document.SignerName ?? "",
                SignedAt = document.SignedAt,
                CertificateSerial = verificationResult.CertificateSerial,
                CertificateStatus = certStatus?.Status == Domain.Enums.CertificateStatus.Expired ? "EXPIRED" : "VALID",
                CertificateValidUntil = verificationResult.CertificateValidUntil,
                CertificateIssuer = verificationResult.CertificateIssuer,
                SignatureAlgorithm = verificationResult.SignatureAlgorithm,
                TimestampAuthority = verificationResult.TimestampAuthority
            };

            if (logAudit)
            {
                await _auditService.LogEventAsync(AuditEventType.DOCUMENT_VERIFIED, documentId, new
                {
                    isValid = true,
                    certificateStatus = valid.CertificateStatus,
                    certificateSerial = valid.CertificateSerial
                });
            }

            return (Ok(valid), valid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying document {DocumentId}", documentId);
            return (StatusCode(500, new { error = "Internal server error" }), null);
        }
    }
}
