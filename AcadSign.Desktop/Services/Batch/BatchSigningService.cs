using Microsoft.Extensions.Logging;
using AcadSign.Desktop.Models;
using AcadSign.Desktop.Services.Signature;
using AcadSign.Desktop.Services.Api;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AcadSign.Desktop.Services.Batch;

public class BatchSigningService : IBatchSigningService
{
    private readonly ISignatureService _signatureService;
    private readonly IApiClientService _apiClient;
    private readonly ILogger<BatchSigningService> _logger;
    private CancellationTokenSource? _cancellationTokenSource;
    
    public BatchSigningService(
        ISignatureService signatureService,
        IApiClientService apiClient,
        ILogger<BatchSigningService> logger)
    {
        _signatureService = signatureService;
        _apiClient = apiClient;
        _logger = logger;
    }
    
    public async Task<BatchSigningResult> SignBatchAsync(
        List<DocumentDto> documents, 
        string pin, 
        IProgress<BatchProgress> progress)
    {
        _cancellationTokenSource = new CancellationTokenSource();
        var token = _cancellationTokenSource.Token;
        
        var stopwatch = Stopwatch.StartNew();
        var result = new BatchSigningResult
        {
            TotalDocuments = documents.Count
        };
        
        _logger.LogInformation("Starting batch signing for {Count} documents", documents.Count);
        
        var downloaded = await DownloadDocumentsInParallelAsync(documents, maxConcurrency: 5, token);

        for (int i = 0; i < downloaded.Count; i++)
        {
            if (token.IsCancellationRequested)
            {
                _logger.LogWarning("Batch signing cancelled at document {Index}", i);
                break;
            }
            
            var (document, unsignedPdf) = downloaded[i];
            
            progress?.Report(new BatchProgress
            {
                CurrentDocument = i + 1,
                TotalDocuments = documents.Count,
                CurrentDocumentName = $"{document.StudentName} - {document.DocumentType}",
                Status = $"Signature de {document.StudentName} - {document.DocumentType}..."
            });
            
            try
            {
                var signedPdf = await _signatureService.SignPdfAsync(unsignedPdf, pin);
                
                // Upload signed PDF
                await _apiClient.UploadSignedDocumentAsync(document.Id, signedPdf);
                
                result.Results.Add(new DocumentSignResult
                {
                    DocumentId = document.Id,
                    DocumentName = document.DocumentType,
                    Success = true,
                    SignedAt = DateTime.Now
                });
                
                result.SuccessfulSigns++;
                
                _logger.LogInformation("Document {DocumentId} signed successfully", document.Id);
            }
            catch (Exception ex)
            {
                result.Results.Add(new DocumentSignResult
                {
                    DocumentId = document.Id,
                    DocumentName = document.DocumentType,
                    Success = false,
                    ErrorMessage = ex.Message,
                    SignedAt = DateTime.Now
                });
                
                result.FailedSigns++;
                
                _logger.LogError(ex, "Failed to sign document {DocumentId}", document.Id);
            }
        }
        
        stopwatch.Stop();
        result.Duration = stopwatch.Elapsed;
        
        _logger.LogInformation(
            "Batch signing completed: {Success} successful, {Failed} failed, Duration: {Duration}",
            result.SuccessfulSigns,
            result.FailedSigns,
            result.Duration);
        
        return result;
    }

    private async Task<List<(DocumentDto Document, byte[] UnsignedPdf)>> DownloadDocumentsInParallelAsync(
        List<DocumentDto> documents,
        int maxConcurrency,
        CancellationToken token)
    {
        var semaphore = new SemaphoreSlim(maxConcurrency);
        var tasks = documents.Select(async doc =>
        {
            await semaphore.WaitAsync(token);
            try
            {
                var bytes = await _apiClient.DownloadDocumentAsync(doc.Id);
                return (Document: doc, UnsignedPdf: bytes);
            }
            finally
            {
                semaphore.Release();
            }
        }).ToList();

        return (await Task.WhenAll(tasks)).ToList();
    }
    
    public Task CancelBatchAsync()
    {
        _cancellationTokenSource?.Cancel();
        _logger.LogInformation("Batch signing cancellation requested");
        return Task.CompletedTask;
    }
}
