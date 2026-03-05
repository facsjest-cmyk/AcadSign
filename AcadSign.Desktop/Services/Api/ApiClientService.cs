using AcadSign.Desktop.Models;

namespace AcadSign.Desktop.Services.Api;

public class ApiClientService : IApiClientService
{
    public async Task<List<DocumentDto>> GetPendingDocumentsAsync()
    {
        await Task.Delay(100);
        
        return new List<DocumentDto>
        {
            new DocumentDto
            {
                Id = Guid.NewGuid(),
                StudentName = "Ahmed El Mansouri",
                DocumentType = "Attestation de Scolarité",
                CreatedAt = DateTime.Now.AddDays(-2),
                Status = "En attente"
            },
            new DocumentDto
            {
                Id = Guid.NewGuid(),
                StudentName = "Fatima Zahra",
                DocumentType = "Relevé de Notes",
                CreatedAt = DateTime.Now.AddDays(-1),
                Status = "En attente"
            }
        };
    }
    
    public async Task<byte[]> DownloadDocumentAsync(Guid documentId)
    {
        await Task.Delay(500);
        return Array.Empty<byte>();
    }
    
    public async Task UploadSignedDocumentAsync(Guid documentId, byte[] signedData)
    {
        await Task.Delay(500);
    }
}
