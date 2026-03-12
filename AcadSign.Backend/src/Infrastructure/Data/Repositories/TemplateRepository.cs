using AcadSign.Backend.Application.Common.Interfaces;
using AcadSign.Backend.Application.Common.Models;
using AcadSign.Backend.Domain.Entities;
using AcadSign.Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AcadSign.Backend.Infrastructure.Data.Repositories;

public class TemplateRepository : ITemplateRepository
{
    private readonly ApplicationDbContext _context;

    public TemplateRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DocumentTemplate?> GetByIdAsync(Guid id)
    {
        return await _context.DocumentTemplates
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<DocumentTemplate>> GetByInstitutionAsync(string institutionId)
    {
        return await _context.DocumentTemplates
            .Where(t => t.InstitutionId == institutionId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<DocumentTemplate>> GetByInstitutionAndTypeAsync(string institutionId, DocumentType type)
    {
        return await _context.DocumentTemplates
            .Where(t => t.InstitutionId == institutionId && t.Type == type)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<DocumentTemplate?> GetActiveTemplateAsync(string institutionId, DocumentType type)
    {
        return await _context.DocumentTemplates
            .Where(t => t.InstitutionId == institutionId 
                     && t.Type == type 
                     && t.IsActive)
            .FirstOrDefaultAsync();
    }

    public async Task AddAsync(DocumentTemplate template)
    {
        await _context.DocumentTemplates.AddAsync(template);
    }

    public async Task UpdateAsync(DocumentTemplate template)
    {
        _context.DocumentTemplates.Update(template);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
