using AcadSign.Backend.Application.UnitTests.Common.Factories;
using AcadSign.Backend.Infrastructure.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace AcadSign.Backend.Infrastructure.IntegrationTests.P0_Infrastructure;

/// <summary>
/// Test ID: P0-002
/// Requirement: PostgreSQL connection established
/// Test Level: Integration
/// </summary>
[TestFixture]
[Category("P0")]
[Category("Infrastructure")]
[Category("Database")]
public class P0_002_PostgreSqlConnectionTests : IntegrationTestBase
{
    private StudentFactory _studentFactory = null!;

    [SetUp]
    public new async Task SetUp()
    {
        await base.SetUp();
        _studentFactory = new StudentFactory();
    }

    [Test]
    public async Task PostgreSql_ShouldConnect_Successfully()
    {
        // Arrange & Act
        var canConnect = await Database.DbContext.Database.CanConnectAsync();

        // Assert
        canConnect.Should().BeTrue("PostgreSQL connection should be established");
    }

    [Test]
    public async Task DatabaseMigrations_ShouldBeApplied_Successfully()
    {
        // Arrange & Act
        var appliedMigrations = await Database.DbContext.Database.GetAppliedMigrationsAsync();
        var pendingMigrations = await Database.DbContext.Database.GetPendingMigrationsAsync();

        // Assert
        appliedMigrations.Should().NotBeEmpty("At least one migration should be applied");
        pendingMigrations.Should().BeEmpty("No pending migrations should exist");
    }

    [Test]
    public async Task Database_ShouldInsert_Student()
    {
        // Arrange
        var student = _studentFactory.Generate();

        // Act
        Database.DbContext.Set<Domain.Entities.Student>().Add(student);
        await Database.DbContext.SaveChangesAsync();

        // Assert
        var savedStudent = await Database.DbContext.Set<Domain.Entities.Student>()
            .FirstOrDefaultAsync(s => s.Id == student.Id);

        savedStudent.Should().NotBeNull("Student should be saved to database");
        savedStudent!.CIN.Should().Be(student.CIN);
        savedStudent.CNE.Should().Be(student.CNE);
    }

    [Test]
    public async Task Database_ShouldQuery_Students()
    {
        // Arrange
        var students = _studentFactory.Generate(5);
        Database.DbContext.Set<Domain.Entities.Student>().AddRange(students);
        await Database.DbContext.SaveChangesAsync();

        // Act
        var count = await Database.DbContext.Set<Domain.Entities.Student>().CountAsync();

        // Assert
        count.Should().Be(5, "All 5 students should be queryable");
    }

    [Test]
    public async Task Database_ShouldUpdate_Student()
    {
        // Arrange
        var student = _studentFactory.Generate();
        Database.DbContext.Set<Domain.Entities.Student>().Add(student);
        await Database.DbContext.SaveChangesAsync();

        // Act
        student.FirstName = "Updated";
        await Database.DbContext.SaveChangesAsync();

        // Assert
        var updatedStudent = await Database.DbContext.Set<Domain.Entities.Student>()
            .FirstOrDefaultAsync(s => s.Id == student.Id);

        updatedStudent!.FirstName.Should().Be("Updated");
    }

    [Test]
    public async Task Database_ShouldDelete_Student()
    {
        // Arrange
        var student = _studentFactory.Generate();
        Database.DbContext.Set<Domain.Entities.Student>().Add(student);
        await Database.DbContext.SaveChangesAsync();

        // Act
        Database.DbContext.Set<Domain.Entities.Student>().Remove(student);
        await Database.DbContext.SaveChangesAsync();

        // Assert
        var deletedStudent = await Database.DbContext.Set<Domain.Entities.Student>()
            .FirstOrDefaultAsync(s => s.Id == student.Id);

        deletedStudent.Should().BeNull("Student should be deleted from database");
    }

    [Test]
    public async Task Database_ShouldIsolate_TestsBetweenRuns()
    {
        // This test verifies that Respawn cleanup works correctly
        // Each test should start with a clean database

        // Arrange & Act
        var count = await Database.DbContext.Set<Domain.Entities.Student>().CountAsync();

        // Assert
        count.Should().Be(0, "Database should be clean at start of each test (Respawn cleanup)");
    }
}
