using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace AcadSign.Backend.Application.Services;

public class CachedPdfGenerationService : IPdfGenerationService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachedPdfGenerationService> _logger;
    
    public CachedPdfGenerationService(
        IMemoryCache cache,
        ILogger<CachedPdfGenerationService> logger)
    {
        _cache = cache;
        _logger = logger;
    }
    
    public async Task<byte[]> GenerateDocumentAsync(DocumentType type, object studentData)
    {
        var template = await _cache.GetOrCreateAsync($"template_{type}", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24);
            _logger.LogInformation("Loading template {Type} into cache", type);
            return await LoadTemplateAsync(type);
        });
        
        return await GenerateFromTemplateAsync(template, studentData);
    }
    
    private async Task<object> LoadTemplateAsync(DocumentType type)
    {
        await Task.Delay(100);
        return new object();
    }
    
    private async Task<byte[]> GenerateFromTemplateAsync(object template, object studentData)
    {
        await Task.Delay(100);
        return new byte[] { 1, 2, 3 };
    }
}

public enum DocumentType
{
    AttestationScolarite,
    Releve,
    Diplome,
    Convention
}
