using System.Reflection;
using AcadSign.Backend.Application.Common.Interfaces;
using AcadSign.Backend.Domain.Entities;
using AcadSign.Backend.Infrastructure.Identity;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AcadSign.Backend.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext, IDataProtectionKeyContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<TodoList> TodoLists => Set<TodoList>();

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    public DbSet<Student> Students => Set<Student>();

    public DbSet<Document> Documents => Set<Document>();

    public DbSet<DocumentTemplate> DocumentTemplates => Set<DocumentTemplate>();

    public DbSet<Batch> Batches => Set<Batch>();

    public DbSet<BatchDocument> BatchDocuments => Set<BatchDocument>();

    public DbSet<DeadLetterQueueEntry> DeadLetterQueueEntries => Set<DeadLetterQueueEntry>();

    public DbSet<AppUser> AppUsers => Set<AppUser>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        
        // Configure OpenIddict entities
        builder.UseOpenIddict();
    }
}
