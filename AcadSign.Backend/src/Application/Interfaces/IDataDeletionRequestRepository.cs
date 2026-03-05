using AcadSign.Backend.Domain.Entities;

namespace AcadSign.Backend.Application.Services;

public interface IDataDeletionRequestRepository
{
    Task AddAsync(DataDeletionRequest request);
    Task<DataDeletionRequest?> GetByIdAsync(Guid id);
    Task<List<DataDeletionRequest>> GetByStudentIdAsync(string studentId);
    Task<List<DataDeletionRequest>> GetPendingRequestsAsync();
    Task UpdateAsync(DataDeletionRequest request);
}
