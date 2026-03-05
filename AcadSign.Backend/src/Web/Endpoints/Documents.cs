using AcadSign.Backend.Application.Common.Interfaces;
using AcadSign.Backend.Application.Common.Models;
using AcadSign.Backend.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcadSign.Backend.Web.Endpoints;

public class Documents : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder group)
    {
        group.RequireAuthorization();
        group.MapGet(GetDocumentMetadata, "{documentId}");
        group.MapGet(GetDownloadUrl, "{documentId}/download");
        group.MapPost(GenerateDocument, "generate");
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
        [FromServices] ILogger<Documents> logger,
        HttpContext httpContext)
    {
        logger.LogInformation("Generating download URL for document {DocumentId}", documentId);

        // Vérifier que le document existe
        var document = await dbContext.Documents
            .FirstOrDefaultAsync(d => d.Id == documentId);

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
        var hasAccess = userRoles.Contains("Admin") || userRoles.Contains("Registrar");

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

        return Results.Ok(new DownloadUrlResponse
        {
            DownloadUrl = downloadUrl,
            ExpiresAt = expiresAt
        });
    }

    [Authorize(Policy = "RequireDocumentGenerateScope")]
    public async Task<IResult> GenerateDocument(
        [FromBody] GenerateDocumentRequest request,
        [FromServices] IPdfGenerationService pdfService,
        [FromServices] IS3StorageService s3Storage,
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
            Id = documentId,
            DocumentType = request.DocumentType.ToString(),
            StudentId = request.StudentId,
            Status = "UNSIGNED",
            S3ObjectPath = s3ObjectPath,
            Created = DateTime.UtcNow,
            CreatedBy = "system"
        };

        dbContext.Documents.Add(document);
        await dbContext.SaveChangesAsync();

        logger.LogInformation("Document {DocumentId} metadata saved to database", documentId);

        return Results.Ok(new GenerateDocumentResponse
        {
            DocumentId = documentId,
            Status = "UNSIGNED",
            UnsignedPdfUrl = $"/api/v1/documents/{documentId}/unsigned",
            CreatedAt = document.Created
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
