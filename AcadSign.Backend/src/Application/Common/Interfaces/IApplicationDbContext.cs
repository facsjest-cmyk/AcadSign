using AcadSign.Backend.Domain.Entities;

namespace AcadSign.Backend.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<TodoList> TodoLists { get; }

    DbSet<TodoItem> TodoItems { get; }

    DbSet<AppUser> AppUsers { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
