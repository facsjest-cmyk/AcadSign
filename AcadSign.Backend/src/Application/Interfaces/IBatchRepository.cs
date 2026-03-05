using AcadSign.Backend.Domain.Entities;

namespace AcadSign.Backend.Application.Services;

public interface IBatchRepository
{
    Task<Batch> GetByIdAsync(Guid id);
    Task AddAsync(Batch batch);
    Task UpdateAsync(Batch batch);
    Task IncrementProcessedCountAsync(Guid batchId);
    Task IncrementFailedCountAsync(Guid batchId);
}
