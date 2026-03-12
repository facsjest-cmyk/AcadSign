using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using AcadSign.Backend.Domain.Entities;
using System.Security.Claims;
using System.Text.Json;

namespace AcadSign.Backend.Application.Services;

public interface IAuditLogService
{
    Task LogEventAsync(AuditEventType eventType, Guid? documentId, object? metadata = null, Guid? userIdOverride = null);
}

public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _auditRepo;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditLogService> _logger;
    
    public AuditLogService(
        IAuditLogRepository auditRepo,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuditLogService> logger)
    {
        _auditRepo = auditRepo;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }
    
    public async Task LogEventAsync(AuditEventType eventType, Guid? documentId, object? metadata = null, Guid? userIdOverride = null)
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                DocumentId = documentId,
                EventType = eventType,
                UserId = userIdOverride ?? GetCurrentUserId(httpContext),
                IpAddress = GetIpAddress(httpContext),
                UserAgent = GetUserAgent(httpContext),
                CertificateSerial = GetCertificateSerial(metadata),
                Metadata = metadata != null ? JsonSerializer.SerializeToDocument(metadata) : null,
                CreatedAt = DateTime.UtcNow,
                CorrelationId = GetCorrelationId(httpContext)
            };
            
            await _auditRepo.AddAsync(auditLog);
            
            _logger.LogInformation("Audit event logged: {EventType} for document {DocumentId}", 
                eventType, documentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log audit event {EventType}", eventType);
        }
    }
    
    private Guid? GetCurrentUserId(HttpContext? context)
    {
        var userIdClaim = context?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return userIdClaim != null ? Guid.Parse(userIdClaim) : null;
    }
    
    private string? GetIpAddress(HttpContext? context)
    {
        return context?.Connection?.RemoteIpAddress?.ToString();
    }
    
    private string? GetUserAgent(HttpContext? context)
    {
        return context?.Request?.Headers["User-Agent"].ToString();
    }
    
    private string? GetCertificateSerial(object? metadata)
    {
        if (metadata == null) return null;
        
        var metadataDict = metadata as Dictionary<string, object>;
        return metadataDict?.ContainsKey("certificateSerial") == true 
            ? metadataDict["certificateSerial"]?.ToString() 
            : null;
    }
    
    private Guid GetCorrelationId(HttpContext? context)
    {
        if (context?.Items.ContainsKey("CorrelationId") == true)
        {
            return Guid.Parse(context.Items["CorrelationId"]!.ToString()!);
        }
        return Guid.NewGuid();
    }
}
