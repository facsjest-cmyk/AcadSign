using AcadSign.Backend.Domain.Entities;

namespace AcadSign.Backend.Application.Common.Interfaces;

public interface IDocumentRepository
{
    Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Document>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<Document>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default);
    Task<Document> CreateAsync(Document document, CancellationToken cancellationToken = default);
    Task UpdateAsync(Document document, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
    Task<int> GetSignedCountAsync(CancellationToken cancellationToken = default);
}
