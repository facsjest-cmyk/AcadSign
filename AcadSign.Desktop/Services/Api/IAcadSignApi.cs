using Refit;
using AcadSign.Desktop.Models;

namespace AcadSign.Desktop.Services.Api;

public interface IAcadSignApi
{
    [Get("/api/v1/documents/pending")]
    Task<List<DocumentDto>> GetPendingDocumentsAsync();
    
    [Get("/api/v1/documents/{id}/download")]
    Task<DownloadUrlResponse> GetDownloadUrlAsync(Guid id);
    
    [Post("/api/v1/documents/{id}/signed")]
    [Multipart]
    Task UploadSignedDocumentAsync(Guid id, [AliasAs("file")] StreamPart file);
    
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
