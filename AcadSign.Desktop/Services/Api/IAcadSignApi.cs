using Refit;
using AcadSign.Desktop.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

namespace AcadSign.Desktop.Services.Api;

public interface IAcadSignApi
{
    // FSJEST: liste des documents en attente de signature (attestations)
    // GET /api/v1/admin/documents/pending?status=READY_FOR_SIGNATURE
    [Get("/api/v1/admin/documents/pending?status=READY_FOR_SIGNATURE")]
    Task<PendingDocumentsResponse> GetPendingDocumentsAsync();

    [Post("/api/v1/admin/attestations/generate-from-sis")]
    Task<AttestationBatchGenerationResponse> GenerateAttestationsFromSisAsync();
    
    // Endpoint hérité pour obtenir une URL de téléchargement (ancien backend)
    [Get("/api/v1/documents/{id}/download")]
    Task<DownloadUrlResponse> GetDownloadUrlAsync(Guid id);

    // Upload du PDF signé vers FSJEST
    [Post("/api/v1/admin/documents/{id}/signed")]
    [Multipart]
    Task UploadSignedDocumentAsync(Guid id, [AliasAs("file")] StreamPart file);

    [Post("/api/v1/documents/{id}/resend-email")]
    Task ResendEmailAsync(Guid id);
    
    // Les endpoints d'authentification sont gérés par AuthenticationService, pas via Refit
}

public class DownloadUrlResponse
{
    public string DownloadUrl { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

// DTOs pour la réponse des documents en attente côté FSJEST

public class PendingDocumentsResponse
{
    public List<PendingDocumentDto> Data { get; set; } = new();
    public string? RequestId { get; set; }
}

public class PendingDocumentDto
{
    public int Id { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int Apogee { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentLabel { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public string PdfUrl { get; set; } = string.Empty;
}

public class AttestationBatchGenerationFailure
{
    public int? ItemIndex { get; set; }
    public string? Apogee { get; set; }
    public string? Nom { get; set; }
    public string? Prenom { get; set; }
    public string? Filiere { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class AttestationBatchGenerationResponse
{
    public int Total { get; set; }
    public int Generated { get; set; }
    public int Failed { get; set; }
    public int DocumentType { get; set; }
    public List<AttestationBatchGenerationFailure> Failures { get; set; } = new();
    public List<Guid> CreatedDocumentIds { get; set; } = new();
}
