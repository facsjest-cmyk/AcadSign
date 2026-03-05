using AcadSign.Backend.Application.Common.Models;
using AcadSign.Backend.Domain.Entities;

namespace AcadSign.Backend.Application.Common.Interfaces;

public interface ITemplateRepository
{
    Task<DocumentTemplate?> GetByIdAsync(Guid id);
    Task<IEnumerable<DocumentTemplate>> GetByInstitutionAsync(string institutionId);
    Task<IEnumerable<DocumentTemplate>> GetByInstitutionAndTypeAsync(string institutionId, DocumentType type);
    Task<DocumentTemplate?> GetActiveTemplateAsync(string institutionId, DocumentType type);
    Task AddAsync(DocumentTemplate template);
    Task UpdateAsync(DocumentTemplate template);
    Task<int> SaveChangesAsync();
}
