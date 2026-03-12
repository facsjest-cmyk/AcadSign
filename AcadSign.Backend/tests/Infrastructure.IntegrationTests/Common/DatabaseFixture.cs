using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using AcadSign.Backend.Infrastructure.Data;

namespace AcadSign.Backend.Infrastructure.IntegrationTests.Common;

/// <summary>
/// Database fixture for integration tests.
/// Provides database context and cleanup utilities using Respawn.
/// </summary>
public class DatabaseFixture
{
    private readonly TestContainersFixture _containers;
    private Respawner? _respawner;

    public ApplicationDbContext DbContext { get; private set; } = null!;

    public DatabaseFixture(TestContainersFixture containers)
    {
        _containers = containers;
    }

    /// <summary>
    /// Initialize database context and apply migrations
    /// </summary>
    public async Task InitializeAsync()
    {
        var services = new ServiceCollection();

        // Configure DbContext with Testcontainers PostgreSQL
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(_containers.PostgresConnectionString));

        var serviceProvider = services.BuildServiceProvider();
        DbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();

        // Apply migrations
        await DbContext.Database.MigrateAsync();

        // Initialize Respawner for database cleanup
        await using var connection = DbContext.Database.GetDbConnection();
        await connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = new[] { "public" },
            TablesToIgnore = new[] { new Respawn.Graph.Table("__EFMigrationsHistory") }
        });
    }

    /// <summary>
    /// Reset database to clean state (delete all data, keep schema)
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        if (_respawner == null)
        {
            throw new InvalidOperationException("DatabaseFixture not initialized. Call InitializeAsync first.");
        }

        await using var connection = DbContext.Database.GetDbConnection();
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);
    }

    /// <summary>
    /// Dispose database context
    /// </summary>
    public async Task DisposeAsync()
    {
        if (DbContext != null)
        {
            await DbContext.DisposeAsync();
        }
    }
}
