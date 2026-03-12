using NUnit.Framework;

namespace AcadSign.Backend.Infrastructure.IntegrationTests.Common;

/// <summary>
/// Base class for all integration tests.
/// Provides Testcontainers setup and database cleanup between tests.
/// </summary>
[TestFixture]
public abstract class IntegrationTestBase
{
    protected static TestContainersFixture Containers { get; private set; } = null!;
    protected DatabaseFixture Database { get; private set; } = null!;

    /// <summary>
    /// OneTimeSetUp: Start Testcontainers (once per test class)
    /// </summary>
    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        try
        {
            Containers = new TestContainersFixture();
            await Containers.InitializeAsync();

            Database = new DatabaseFixture(Containers);
            await Database.InitializeAsync();
        }
        catch (Exception ex) when (ex.GetType().Name.Contains("DockerUnavailableException", StringComparison.OrdinalIgnoreCase))
        {
            Assert.Ignore($"Docker indisponible/mal configuré (Testcontainers). Tests d'intégration ignorés. {ex.Message}");
        }
    }

    /// <summary>
    /// SetUp: Reset database before each test (isolation)
    /// </summary>
    [SetUp]
    public async Task SetUp()
    {
        await Database.ResetDatabaseAsync();
    }

    /// <summary>
    /// OneTimeTearDown: Stop Testcontainers (once per test class)
    /// </summary>
    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (Database != null)
        {
            await Database.DisposeAsync();
        }

        if (Containers != null)
        {
            await Containers.DisposeAsync();
        }
    }
}
