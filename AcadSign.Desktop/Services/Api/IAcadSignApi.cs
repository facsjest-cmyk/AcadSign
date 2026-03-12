using Refit;
using AcadSign.Desktop.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

namespace AcadSign.Desktop.Services.Api;

public interface IAcadSignApi
{
    [Get("/api/v1/documents/pending")]
    Task<List<DocumentDto>> GetPendingDocumentsAsync();

    [Post("/api/v1/admin/attestations/generate-from-sis")]
    Task<AttestationBatchGenerationResponse> GenerateAttestationsFromSisAsync();
    
    [Get("/api/v1/documents/{id}/download")]
    Task<DownloadUrlResponse> GetDownloadUrlAsync(Guid id);
    
    [Post("/api/v1/documents/{id}/signed")]
    [Multipart]
    Task UploadSignedDocumentAsync(Guid id, [AliasAs("file")] StreamPart file);

    [Post("/api/v1/documents/{id}/resend-email")]
    Task ResendEmailAsync(Guid id);
    
    [Post("/api/v1/auth/token")]
    Task<TokenResponse> GetTokenAsync([Body] TokenRequest request);
    
    [Post("/api/v1/auth/refresh")]
    Task<TokenResponse> RefreshTokenAsync([Body] RefreshTokenRequest request);
}

public class DownloadUrlResponse
{
    public string DownloadUrl { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public class TokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
}

public class TokenRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
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
