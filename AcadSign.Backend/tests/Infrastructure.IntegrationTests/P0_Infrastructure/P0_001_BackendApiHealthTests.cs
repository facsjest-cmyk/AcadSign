using AcadSign.Backend.Infrastructure.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace AcadSign.Backend.Infrastructure.IntegrationTests.P0_Infrastructure;

/// <summary>
/// Test ID: P0-001
/// Requirement: Backend API starts successfully
/// Test Level: Integration
/// </summary>
[TestFixture]
[Category("P0")]
[Category("Infrastructure")]
public class P0_001_BackendApiHealthTests : IntegrationTestBase
{
    [Test]
    public async Task BackendApi_ShouldStart_Successfully()
    {
        // Arrange & Act
        var canConnect = await Database.DbContext.Database.CanConnectAsync();

        // Assert
        canConnect.Should().BeTrue("Backend API should be able to connect to database");
    }

    [Test]
    public async Task HealthCheck_ShouldReturn_DatabaseConnected()
    {
        // Arrange & Act
        var connectionState = Database.DbContext.Database.GetDbConnection().State;
        var canConnect = await Database.DbContext.Database.CanConnectAsync();

        // Assert
        canConnect.Should().BeTrue("Health check should confirm database connection");
    }

    [Test]
    public async Task Database_ShouldHave_MigrationsApplied()
    {
        // Arrange & Act
        var pendingMigrations = await Database.DbContext.Database.GetPendingMigrationsAsync();

        // Assert
        pendingMigrations.Should().BeEmpty("All migrations should be applied on startup");
    }
}
