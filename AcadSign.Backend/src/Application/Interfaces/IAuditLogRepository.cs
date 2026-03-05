using AcadSign.Backend.Domain.Entities;

namespace AcadSign.Backend.Application.Services;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog auditLog);
    Task<List<AuditLog>> GetByDocumentIdAsync(Guid documentId);
    Task<List<AuditLog>> GetByUserIdAsync(Guid userId);
    Task<List<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<List<AuditLog>> SearchAsync(
        string? eventType = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        Guid? userId = null,
        Guid? documentId = null,
        int limit = 100,
        int offset = 0);
    Task<int> CountAsync(
        string? eventType = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        Guid? userId = null,
        Guid? documentId = null);
}
