using AcadSign.Desktop.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
                Status = "UNSIGNED"
            },
            new DocumentDto
            {
                Id = Guid.NewGuid(),
                StudentName = "Fatima Zahra",
                DocumentType = "Relevé de Notes",
                CreatedAt = DateTime.Now.AddDays(-1),
                Status = "UNSIGNED"
            }
        };
    }

    public async Task<AttestationBatchGenerationResponse> GenerateAttestationsFromSisAsync()
    {
        await Task.Delay(600);

        return new AttestationBatchGenerationResponse
        {
            Total = 2,
            Generated = 2,
            Failed = 0,
            DocumentType = 0,
            CreatedDocumentIds = new List<Guid>
            {
                Guid.NewGuid(),
                Guid.NewGuid()
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

    public async Task<DownloadUrlResponse> GetDownloadUrlAsync(Guid documentId)
    {
        await Task.Delay(100);
        return new DownloadUrlResponse
        {
            DownloadUrl = $"https://example.local/documents/{documentId}.pdf",
            ExpiresAt = DateTime.UtcNow.AddMinutes(60)
        };
    }

    public async Task ResendEmailAsync(Guid documentId)
    {
        await Task.Delay(100);
    }
}
