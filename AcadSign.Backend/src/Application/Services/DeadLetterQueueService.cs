using Hangfire;
using Microsoft.Extensions.Logging;
using AcadSign.Backend.Domain.Entities;

namespace AcadSign.Backend.Application.Services;

public class DeadLetterQueueService
{
    private readonly IDeadLetterQueueRepository _dlqRepo;
    private readonly ILogger<DeadLetterQueueService> _logger;
    
    public DeadLetterQueueService(
        IDeadLetterQueueRepository dlqRepo,
        ILogger<DeadLetterQueueService> logger)
    {
        _dlqRepo = dlqRepo;
        _logger = logger;
    }
    
    public async Task MoveToDeadLetterQueueAsync(Guid documentId, Exception ex, string? jobId = null)
    {
        var dlqEntry = new DeadLetterQueueEntry
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            JobId = jobId,
            ErrorMessage = ex.Message,
            StackTrace = ex.StackTrace,
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow
        };
        
        await _dlqRepo.AddAsync(dlqEntry);
        
        _logger.LogWarning("Document {DocumentId} moved to dead-letter queue: {Error}", 
            documentId, ex.Message);
    }
    
    public async Task<List<DeadLetterQueueEntry>> GetUnresolvedEntriesAsync()
    {
        return await _dlqRepo.GetUnresolvedAsync();
    }
    
    public async Task RetryFromDeadLetterQueueAsync(Guid dlqId, Guid adminUserId)
    {
        var entry = await _dlqRepo.GetByIdAsync(dlqId);
        
        if (entry == null)
        {
            throw new InvalidOperationException($"DLQ entry {dlqId} not found");
        }
        
        if (entry.ResolvedAt.HasValue)
        {
            throw new InvalidOperationException($"DLQ entry {dlqId} already resolved");
        }
        
        entry.RetryCount++;
        entry.LastRetryAt = DateTime.UtcNow;
        entry.ResolvedAt = DateTime.UtcNow;
        entry.ResolvedBy = adminUserId;
        
        await _dlqRepo.UpdateAsync(entry);
        
        _logger.LogInformation("DLQ entry {DlqId} resolved and retried by admin {AdminId}", 
            dlqId, adminUserId);
    }
    
    public bool IsRetryableException(Exception ex)
    {
        return ex is HttpRequestException 
            || ex is TimeoutException
            || ex is IOException
            || ex is TaskCanceledException;
    }
}

public class NonRetryableException : Exception
{
    public NonRetryableException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}
