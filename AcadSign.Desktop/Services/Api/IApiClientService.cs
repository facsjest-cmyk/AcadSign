using AcadSign.Desktop.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AcadSign.Desktop.Services.Api;

public interface IApiClientService
{
    Task<List<DocumentDto>> GetPendingDocumentsAsync();
    Task<AttestationBatchGenerationResponse> GenerateAttestationsFromSisAsync();
    Task<byte[]> DownloadDocumentAsync(Guid documentId);
    Task UploadSignedDocumentAsync(Guid documentId, byte[] signedData);
    Task<DownloadUrlResponse> GetDownloadUrlAsync(Guid documentId);
    Task ResendEmailAsync(Guid documentId);
}
