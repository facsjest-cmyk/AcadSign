using AcadSign.Backend.Application.Services;
using AcadSign.Backend.Domain.Entities;
using AcadSign.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AcadSign.Backend.Infrastructure.Data.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly ApplicationDbContext _context;

    public AuditLogRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(AuditLog auditLog)
    {
        await _context.AuditLogs.AddAsync(auditLog);
        await _context.SaveChangesAsync();
    }

    public async Task<List<AuditLog>> GetByDocumentIdAsync(Guid documentId)
    {
        return await _context.AuditLogs
            .AsNoTracking()
            .Where(l => l.DocumentId == documentId)
            .OrderBy(l => l.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<AuditLog>> GetByUserIdAsync(Guid userId)
    {
        return await _context.AuditLogs
            .AsNoTracking()
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.AuditLogs
            .AsNoTracking()
            .Where(l => l.CreatedAt >= startDate && l.CreatedAt <= endDate)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<AuditLog>> SearchAsync(
        string? eventType = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        Guid? userId = null,
        Guid? documentId = null,
        int limit = 100,
        int offset = 0)
    {
        var query = BuildQuery(eventType, startDate, endDate, userId, documentId);

        return await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<int> CountAsync(
        string? eventType = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        Guid? userId = null,
        Guid? documentId = null)
    {
        var query = BuildQuery(eventType, startDate, endDate, userId, documentId);
        return await query.CountAsync();
    }

    private IQueryable<AuditLog> BuildQuery(
        string? eventType,
        DateTime? startDate,
        DateTime? endDate,
        Guid? userId,
        Guid? documentId)
    {
        var query = _context.AuditLogs.AsNoTracking().AsQueryable();

        if (documentId.HasValue)
        {
            query = query.Where(l => l.DocumentId == documentId.Value);
        }

        if (userId.HasValue)
        {
            query = query.Where(l => l.UserId == userId.Value);
        }

        if (!string.IsNullOrWhiteSpace(eventType)
            && Enum.TryParse<AuditEventType>(eventType, ignoreCase: true, out var parsed))
        {
            query = query.Where(l => l.EventType == parsed);
        }

        if (startDate.HasValue)
        {
            query = query.Where(l => l.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(l => l.CreatedAt <= endDate.Value);
        }

        return query;
    }
}
