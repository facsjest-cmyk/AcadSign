using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using AcadSign.Backend.Application.Common.Interfaces;
using AcadSign.Backend.Application.Common.Models;
using AcadSign.Backend.Domain.Enums;

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
    
    public async Task<byte[]> GenerateDocumentAsync(DocumentType type, StudentData studentData)
    {
        var template = await _cache.GetOrCreateAsync($"template_{type}", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24);
            _logger.LogInformation("Loading template {Type} into cache", type);
            return await LoadTemplateAsync(type);
        });
        
        if (template == null)
        {
            throw new InvalidOperationException($"Template for {type} could not be loaded");
        }
        
        return await GenerateFromTemplateAsync(template, studentData);
    }
    
    private async Task<object> LoadTemplateAsync(DocumentType type)
    {
        await Task.Delay(100);
        return new object();
    }
    
    private async Task<byte[]> GenerateFromTemplateAsync(object template, StudentData studentData)
    {
        await Task.Delay(100);
        return new byte[] { 1, 2, 3 };
    }
}
