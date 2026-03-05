using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Testcontainers.Minio;
using Testcontainers.PostgreSql;

namespace AcadSign.Backend.Infrastructure.IntegrationTests.Common;

/// <summary>
/// Testcontainers fixture for integration tests.
/// Provides isolated PostgreSQL and MinIO containers for testing.
/// Implements IAsyncLifetime for NUnit OneTimeSetUp/OneTimeTearDown.
/// </summary>
public class TestContainersFixture : IAsyncDisposable
{
    private PostgreSqlContainer? _postgresContainer;
    private MinioContainer? _minioContainer;

    public string PostgresConnectionString { get; private set; } = string.Empty;
    public string MinioEndpoint { get; private set; } = string.Empty;
    public string MinioAccessKey { get; private set; } = "minioadmin";
    public string MinioSecretKey { get; private set; } = "minioadmin";

    /// <summary>
    /// Initialize and start all containers
    /// </summary>
    public async Task InitializeAsync()
    {
        // PostgreSQL Container
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("acadsign_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .WithCleanUp(true)
            .Build();

        await _postgresContainer.StartAsync();
        PostgresConnectionString = _postgresContainer.GetConnectionString();

        // MinIO Container
        _minioContainer = new MinioBuilder()
            .WithImage("minio/minio:latest")
            .WithUsername(MinioAccessKey)
            .WithPassword(MinioSecretKey)
            .WithCleanUp(true)
            .Build();

        await _minioContainer.StartAsync();
        MinioEndpoint = _minioContainer.GetConnectionString();
    }

    /// <summary>
    /// Stop and dispose all containers
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_postgresContainer != null)
        {
            await _postgresContainer.StopAsync();
            await _postgresContainer.DisposeAsync();
        }

        if (_minioContainer != null)
        {
            await _minioContainer.StopAsync();
            await _minioContainer.DisposeAsync();
        }
    }
}
