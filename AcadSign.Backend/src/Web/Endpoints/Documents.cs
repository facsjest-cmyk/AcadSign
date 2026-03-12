using AcadSign.Backend.Application.Common.Interfaces;
using AcadSign.Backend.Application.Common.Models;
using AcadSign.Backend.Application.BackgroundJobs;
using AcadSign.Backend.Domain.Enums;
using AcadSign.Backend.Domain.Entities;
using AcadSign.Backend.Application.Services;
using AcadSign.Backend.Infrastructure.Data;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Text;

namespace AcadSign.Backend.Web.Endpoints;

public class Documents : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder group)
    {
        group.RequireAuthorization();
        group.MapGet(GetPendingDocuments, "pending").AllowAnonymous();
        group.MapGet(GetDocumentMetadata, "{documentId}");
        group.MapGet(GetDownloadUrl, "{documentId}/download").AllowAnonymous();
        group.MapGet(GetRawDocument, "{documentId}/raw").AllowAnonymous();
        group.MapPost(UploadSignedDocument, "{documentId}/signed").DisableAntiforgery().AllowAnonymous();
        group.MapPost(ResendEmail, "{documentId}/resend-email");
        group.MapPost(GenerateDocument, "generate");
    }

    [AllowAnonymous]
    public async Task<IResult> GetPendingDocuments(
        [FromServices] ApplicationDbContext dbContext)
    {
        var pendingRows = await (
                from d in dbContext.Documents.AsNoTracking()
                join s in dbContext.Students.AsNoTracking() on d.StudentId equals s.PublicId into students
                from s in students.DefaultIfEmpty()
                where d.Status == "UNSIGNED" || d.Status == "PENDING"
                orderby d.Created descending
                select new
                {
                    Document = d,
                    Student = s
                })
            .ToListAsync();

        var docs = pendingRows
            .Select(x => PendingDocumentDtoMapper.Map(x.Document, x.Student))
            .ToList();

        return Results.Ok(docs);
    }

    [Authorize(Policy = "RequireDocumentReadScope")]
    public async Task<IResult> GetDocumentMetadata(Guid documentId)
    {
        // TODO: Story 3.x - Implement document retrieval
        return Results.Ok(new
        {
            documentId,
            status = "pending",
            message = "Document metadata endpoint - to be implemented in Story 3.x"
        });
    }

    [Authorize]
    public async Task<IResult> GetDownloadUrl(
        Guid documentId,
        [FromServices] IS3StorageService s3Storage,
        [FromServices] ApplicationDbContext dbContext,
        [FromServices] IAuditLogService auditService,
        [FromServices] ILogger<Documents> logger,
        HttpContext httpContext)
    {
        logger.LogInformation("Generating download URL for document {DocumentId}", documentId);

        var env = httpContext.RequestServices.GetService<IHostEnvironment>();
        if (env?.IsDevelopment() == true)
        {
            // Dev-friendly behavior: return a URL to the raw endpoint so Desktop can preview PDFs
            var baseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";
            return Results.Ok(new DownloadUrlResponse
            {
                DownloadUrl = $"{baseUrl}/api/v1/documents/{documentId}/raw",
                ExpiresAt = DateTime.UtcNow.AddMinutes(60)
            });
        }

        // Vérifier que le document existe
        // Note: documentId est un Guid mais Document.Id est un int, conversion nécessaire
        var document = await dbContext.Documents
            .FirstOrDefaultAsync(d => d.PublicId == documentId);

        if (document == null)
        {
            logger.LogWarning("Document {DocumentId} not found", documentId);
            return Results.NotFound(new { error = "Document not found" });
        }

        // Vérifier que le document est signé (pour l'instant, on accepte aussi UNSIGNED pour les tests)
        // En production, décommenter cette vérification
        // if (document.Status != "SIGNED")
        // {
        //     logger.LogWarning("Document {DocumentId} is not signed yet", documentId);
        //     return Results.BadRequest(new { error = "Document is not signed yet" });
        // }

        // Vérifier que l'utilisateur a accès au document
        var userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var userRoles = httpContext.User.Claims
            .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        // Admin et Registrar peuvent accéder à tous les documents
        var hasAccess = userRoles.Contains("Administrator") || userRoles.Contains("Registrar");

        // L'étudiant peut accéder à ses propres documents
        // Pour l'instant, on autorise tous les utilisateurs authentifiés (MVP)
        // En production, vérifier que userId correspond au StudentId du document
        hasAccess = true; // MVP: autoriser tous les utilisateurs authentifiés

        if (!hasAccess)
        {
            logger.LogWarning("User {UserId} does not have access to document {DocumentId}", userId, documentId);
            return Results.Forbid();
        }

        // Générer la pre-signed URL (1 heure)
        var downloadUrl = await s3Storage.GeneratePresignedDownloadUrlAsync(
            documentId.ToString(),
            expiryMinutes: 60);

        var expiresAt = DateTime.UtcNow.AddHours(1);

        logger.LogInformation("Download URL generated for document {DocumentId}, expires at {ExpiresAt}",
            documentId, expiresAt);

        await auditService.LogEventAsync(AuditEventType.DOCUMENT_DOWNLOADED, documentId, new
        {
            type = "PRESIGNED_URL",
            expiresAt
        });

        return Results.Ok(new DownloadUrlResponse
        {
            DownloadUrl = downloadUrl,
            ExpiresAt = expiresAt
        });
    }

    [AllowAnonymous]
    public IResult GetRawDocument(Guid documentId)
    {
        var pdfBytes = BuildSimplePdf($"AcadSign - {documentId}");
        return Results.File(pdfBytes, "application/pdf", $"{documentId}.pdf");
    }

    [AllowAnonymous]
    public async Task<IResult> UploadSignedDocument(
        Guid documentId,
        [FromForm] IFormFile file,
        [FromServices] IS3StorageService s3Storage,
        [FromServices] ApplicationDbContext dbContext,
        [FromServices] IAuditLogService auditService,
        [FromServices] ILogger<Documents> logger,
        [FromServices] EmailNotificationJob emailJob,
        HttpContext httpContext)
    {
        logger.LogInformation("Received signed document upload for {DocumentId}. File: {FileName} ({Length} bytes)", documentId, file.FileName, file.Length);

        byte[] bytes;
        using (var ms = new MemoryStream())
        {
            await file.CopyToAsync(ms);
            bytes = ms.ToArray();
        }

        var document = await dbContext.Documents.FirstOrDefaultAsync(d => d.PublicId == documentId);
        if (document == null)
        {
            return Results.NotFound(new { error = "Document not found" });
        }

        var objectPath = await s3Storage.UploadDocumentAsync(bytes, documentId.ToString());
        document.S3ObjectPath = objectPath;
        document.Status = "SIGNED";
        document.SignedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        await auditService.LogEventAsync(AuditEventType.DOCUMENT_UPLOADED, documentId, new
        {
            kind = "SIGNED_UPLOAD",
            fileName = file.FileName,
            length = file.Length,
            contentType = file.ContentType
        });

        BackgroundJob.Enqueue(() => emailJob.SendDocumentReadyEmailAsync(documentId));

        return Results.Ok(new { success = true });
    }

    [Authorize]
    public async Task<IResult> ResendEmail(
        Guid documentId,
        [FromServices] ApplicationDbContext dbContext,
        [FromServices] ILogger<Documents> logger,
        [FromServices] EmailNotificationJob emailJob,
        HttpContext httpContext)
    {
        var document = await dbContext.Documents.FirstOrDefaultAsync(d => d.PublicId == documentId);
        if (document == null)
        {
            return Results.NotFound(new { error = "Document not found" });
        }

        // MVP authorization: any authenticated user can request a resend.
        // Future: validate student ownership based on claims.
        var jobId = BackgroundJob.Enqueue(() => emailJob.SendDocumentReadyEmailAsync(documentId));

        logger.LogInformation("Email resend requested for document {DocumentId} by {UserId} (job {JobId})",
            documentId,
            httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
            jobId);

        return Results.Accepted($"/hangfire/jobs/details/{jobId}", new { message = "Email queued", jobId });
    }

    private static byte[] BuildSimplePdf(string text)
    {
        // Minimal valid PDF generated on-the-fly with correct xref offsets.
        // This is used for dev preview only.

        static string EscapePdfString(string value) => value.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");

        using var ms = new MemoryStream();

        void Write(string s)
        {
            var bytes = Encoding.ASCII.GetBytes(s);
            ms.Write(bytes, 0, bytes.Length);
        }

        Write("%PDF-1.4\n");

        var offsets = new long[6];
        offsets[0] = 0;

        offsets[1] = ms.Position;
        Write("1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n");

        offsets[2] = ms.Position;
        Write("2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n");

        offsets[3] = ms.Position;
        Write("3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >>\nendobj\n");

        var content = $"BT /F1 24 Tf 72 720 Td ({EscapePdfString(text)}) Tj ET\n";
        var contentBytes = Encoding.ASCII.GetBytes(content);

        offsets[4] = ms.Position;
        Write($"4 0 obj\n<< /Length {contentBytes.Length} >>\nstream\n");
        ms.Write(contentBytes, 0, contentBytes.Length);
        Write("endstream\nendobj\n");

        offsets[5] = ms.Position;
        Write("5 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>\nendobj\n");

        var xrefPos = ms.Position;
        Write("xref\n0 6\n");
        Write("0000000000 65535 f \n");
        for (var i = 1; i <= 5; i++)
        {
            Write($"{offsets[i]:D10} 00000 n \n");
        }

        Write("trailer\n<< /Size 6 /Root 1 0 R >>\n");
        Write($"startxref\n{xrefPos}\n%%EOF\n");

        return ms.ToArray();
    }

    [Authorize(Policy = "RequireDocumentGenerateScope")]
    public async Task<IResult> GenerateDocument(
        [FromBody] GenerateDocumentRequest request,
        [FromServices] IPdfGenerationService pdfService,
        [FromServices] IS3StorageService s3Storage,
        [FromServices] IAuditLogService auditService,
        [FromServices] ApplicationDbContext dbContext,
        [FromServices] ILogger<Documents> logger)
    {
        logger.LogInformation("Generating document of type {DocumentType} for student {StudentId}",
            request.DocumentType, request.StudentId);

        // Générer UUID v4 (FR3)
        var documentId = Guid.NewGuid();
        request.StudentData.DocumentId = documentId;

        // Générer le PDF
        var pdfBytes = await pdfService.GenerateDocumentAsync(
            request.DocumentType,
            request.StudentData);

        logger.LogInformation("Document {DocumentId} generated successfully. Size: {Size} bytes",
            documentId, pdfBytes.Length);

        // Upload vers MinIO S3
        var s3ObjectPath = await s3Storage.UploadDocumentAsync(pdfBytes, documentId.ToString());
        logger.LogInformation("Document {DocumentId} uploaded to S3: {S3Path}", documentId, s3ObjectPath);

        // Sauvegarder les métadonnées en DB
        var document = new Domain.Entities.Document
        {
            PublicId = documentId,
            DocumentType = request.DocumentType.ToString(),
            StudentId = request.StudentId,
            Status = "UNSIGNED",
            S3ObjectPath = s3ObjectPath
        };

        dbContext.Documents.Add(document);
        await dbContext.SaveChangesAsync();

        logger.LogInformation("Document {DocumentId} metadata saved to database", documentId);

        await auditService.LogEventAsync(AuditEventType.DOCUMENT_GENERATED, documentId, new
        {
            documentType = request.DocumentType.ToString(),
            studentId = request.StudentId
        });

        await auditService.LogEventAsync(AuditEventType.DOCUMENT_UPLOADED, documentId, new
        {
            s3ObjectPath = s3ObjectPath
        });

        return Results.Ok(new GenerateDocumentResponse
        {
            DocumentId = documentId,
            Status = "UNSIGNED",
            UnsignedPdfUrl = $"/api/v1/documents/{documentId}/unsigned",
            CreatedAt = document.Created.DateTime
        });
    }
}

public record GenerateDocumentRequest
{
    public DocumentType DocumentType { get; init; }
    public Guid StudentId { get; init; }
    public StudentData StudentData { get; init; } = null!;
}

public record GenerateDocumentResponse
{
    public Guid DocumentId { get; init; }
    public string Status { get; init; } = string.Empty;
    public string UnsignedPdfUrl { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

public record DownloadUrlResponse
{
    public string DownloadUrl { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
}
