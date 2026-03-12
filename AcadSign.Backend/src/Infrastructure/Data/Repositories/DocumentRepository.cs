using AcadSign.Backend.Application.Common.Interfaces;
using AcadSign.Backend.Domain.Entities;
using AcadSign.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AcadSign.Backend.Infrastructure.Data.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly ApplicationDbContext _context;

    public DocumentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Documents.FirstOrDefaultAsync(d => d.PublicId == id, cancellationToken);
    }

    public async Task<List<Document>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Documents.ToListAsync(cancellationToken);
    }

    public async Task<List<Document>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        return await _context.Documents.Where(d => d.StudentId == studentId).ToListAsync(cancellationToken);
    }

    public async Task<Document> CreateAsync(Document document, CancellationToken cancellationToken = default)
    {
        await _context.Documents.AddAsync(document, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return document;
    }

    public async Task UpdateAsync(Document document, CancellationToken cancellationToken = default)
    {
        _context.Documents.Update(document);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var existing = await GetByIdAsync(id, cancellationToken);
        if (existing == null)
        {
            return;
        }

        _context.Documents.Remove(existing);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Documents.CountAsync(cancellationToken);
    }

    public async Task<int> GetSignedCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Documents.CountAsync(d => d.Status == "SIGNED", cancellationToken);
    }
}
