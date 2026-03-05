using Refit;
using AcadSign.Desktop.Models;
using Microsoft.Extensions.Logging;

namespace AcadSign.Desktop.Services.Api;

public class RefitApiClientService : IApiClientService
{
    private readonly IAcadSignApi _api;
    private readonly ILogger<RefitApiClientService> _logger;
    
    public RefitApiClientService(IAcadSignApi api, ILogger<RefitApiClientService> logger)
    {
        _api = api;
        _logger = logger;
    }
    
    public async Task<List<DocumentDto>> GetPendingDocumentsAsync()
    {
        try
        {
            _logger.LogInformation("Fetching pending documents from API");
            return await _api.GetPendingDocumentsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch pending documents");
            throw;
        }
    }
    
    public async Task<byte[]> DownloadDocumentAsync(Guid documentId)
    {
        try
        {
            _logger.LogInformation("Downloading document {DocumentId}", documentId);
            
            var urlResponse = await _api.GetDownloadUrlAsync(documentId);
            
            using var httpClient = new HttpClient();
            var pdfBytes = await httpClient.GetByteArrayAsync(urlResponse.DownloadUrl);
            
            _logger.LogInformation("Document {DocumentId} downloaded successfully", documentId);
            return pdfBytes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download document {DocumentId}", documentId);
            throw;
        }
    }
    
    public async Task UploadSignedDocumentAsync(Guid documentId, byte[] signedData)
    {
        try
        {
            _logger.LogInformation("Uploading signed document {DocumentId}", documentId);
            
            var stream = new MemoryStream(signedData);
            var streamPart = new StreamPart(stream, "signed.pdf", "application/pdf");
            
            await _api.UploadSignedDocumentAsync(documentId, streamPart);
            
            _logger.LogInformation("Signed document {DocumentId} uploaded successfully", documentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload signed document {DocumentId}", documentId);
            throw;
        }
    }
}
