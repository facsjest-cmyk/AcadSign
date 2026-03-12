using AcadSign.Backend.Application.Services;
using AcadSign.Backend.Domain.Entities;
using AcadSign.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AcadSign.Backend.Infrastructure.Data.Repositories;

public class BatchRepository : IBatchRepository
{
    private readonly ApplicationDbContext _context;

    public BatchRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Batch?> GetByIdAsync(Guid id)
    {
        return await _context.Batches
            .Include(b => b.Documents)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task AddAsync(Batch batch)
    {
        await _context.Batches.AddAsync(batch);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Batch batch)
    {
        _context.Batches.Update(batch);
        await _context.SaveChangesAsync();
    }

    public async Task IncrementProcessedCountAsync(Guid batchId)
    {
        var batch = await _context.Batches.FirstOrDefaultAsync(b => b.Id == batchId);
        if (batch == null)
        {
            return;
        }

        batch.ProcessedDocuments++;
        await _context.SaveChangesAsync();
    }

    public async Task IncrementFailedCountAsync(Guid batchId)
    {
        var batch = await _context.Batches.FirstOrDefaultAsync(b => b.Id == batchId);
        if (batch == null)
        {
            return;
        }

        batch.FailedDocuments++;
        batch.ProcessedDocuments++;
        await _context.SaveChangesAsync();
    }

    public async Task<BatchDocument?> GetBatchDocumentAsync(Guid batchId, string studentId)
    {
        return await _context.BatchDocuments
            .FirstOrDefaultAsync(d => d.BatchId == batchId && d.StudentId == studentId);
    }

    public async Task UpdateBatchDocumentAsync(BatchDocument batchDocument)
    {
        _context.BatchDocuments.Update(batchDocument);
        await _context.SaveChangesAsync();
    }
}
