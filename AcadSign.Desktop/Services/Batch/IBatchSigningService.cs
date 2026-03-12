using AcadSign.Desktop.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AcadSign.Desktop.Services.Batch;

public interface IBatchSigningService
{
    Task<BatchSigningResult> SignBatchAsync(List<DocumentDto> documents, string pin, IProgress<BatchProgress> progress);
    Task CancelBatchAsync();
}

public class BatchSigningResult
{
    public int TotalDocuments { get; set; }
    public int SuccessfulSigns { get; set; }
    public int FailedSigns { get; set; }
    public List<DocumentSignResult> Results { get; set; } = new();
    public TimeSpan Duration { get; set; }
}

public class DocumentSignResult
{
    public Guid DocumentId { get; set; }
    public string DocumentName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime SignedAt { get; set; }
}

public class BatchProgress
{
    public int CurrentDocument { get; set; }
    public int TotalDocuments { get; set; }
    public string CurrentDocumentName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public double PercentComplete => TotalDocuments > 0 ? (double)CurrentDocument / TotalDocuments * 100 : 0;
}
