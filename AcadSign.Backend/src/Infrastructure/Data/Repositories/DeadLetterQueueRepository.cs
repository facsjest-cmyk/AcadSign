using AcadSign.Backend.Application.Services;
using AcadSign.Backend.Domain.Entities;
using AcadSign.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AcadSign.Backend.Infrastructure.Data.Repositories;

public class DeadLetterQueueRepository : IDeadLetterQueueRepository
{
    private readonly ApplicationDbContext _context;

    public DeadLetterQueueRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DeadLetterQueueEntry?> GetByIdAsync(Guid id)
    {
        return await _context.DeadLetterQueueEntries.FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<List<DeadLetterQueueEntry>> GetUnresolvedAsync()
    {
        return await _context.DeadLetterQueueEntries
            .Where(e => e.ResolvedAt == null)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(DeadLetterQueueEntry entry)
    {
        await _context.DeadLetterQueueEntries.AddAsync(entry);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(DeadLetterQueueEntry entry)
    {
        _context.DeadLetterQueueEntries.Update(entry);
        await _context.SaveChangesAsync();
    }
}
