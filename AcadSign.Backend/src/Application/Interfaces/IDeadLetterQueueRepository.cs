using AcadSign.Backend.Domain.Entities;

namespace AcadSign.Backend.Application.Services;

public interface IDeadLetterQueueRepository
{
    Task<DeadLetterQueueEntry?> GetByIdAsync(Guid id);
    Task<List<DeadLetterQueueEntry>> GetUnresolvedAsync();
    Task AddAsync(DeadLetterQueueEntry entry);
    Task UpdateAsync(DeadLetterQueueEntry entry);
}
