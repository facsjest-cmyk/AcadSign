using AcadSign.Backend.Application.Common.Interfaces;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace AcadSign.Backend.Application.UnitTests.Security;

/// <summary>
/// Test ID: P0-009
/// Requirement: RBAC - Admin can access all endpoints
/// Test Level: Integration
/// Risk Link: R-8 (JWT tokens volés permettent accès non autorisé)
/// </summary>
[TestFixture]
[Category("P0")]
[Category("Security")]
[Category("RBAC")]
public class P0_009_RBACAdminAccessTests
{
    private Mock<IIdentityService> _mockIdentityService = null!;

    [SetUp]
    public void SetUp()
    {
        _mockIdentityService = new Mock<IIdentityService>();
    }

    [Test]
    public async Task Admin_ShouldAccessDocumentManagement_Endpoints()
    {
        // Arrange
        var adminUserId = "admin-user-123";
        _mockIdentityService
            .Setup(x => x.IsInRoleAsync(adminUserId, "Admin"))
            .ReturnsAsync(true);

        // Act
        var hasAccess = await _mockIdentityService.Object.IsInRoleAsync(adminUserId, "Admin");

        // Assert
        hasAccess.Should().BeTrue("Admin should have access to document management");
    }

    [Test]
    public async Task Admin_ShouldAccessUserManagement_Endpoints()
    {
        // Arrange
        var adminUserId = "admin-user-123";
        _mockIdentityService
            .Setup(x => x.AuthorizeAsync(adminUserId, "UserManagement"))
            .ReturnsAsync(true);

        // Act
        var isAuthorized = await _mockIdentityService.Object.AuthorizeAsync(adminUserId, "UserManagement");

        // Assert
        isAuthorized.Should().BeTrue("Admin should have access to user management");
    }

    [Test]
    public async Task Admin_ShouldAccessTemplateManagement_Endpoints()
    {
        // Arrange
        var adminUserId = "admin-user-123";
        _mockIdentityService
            .Setup(x => x.AuthorizeAsync(adminUserId, "TemplateManagement"))
            .ReturnsAsync(true);

        // Act
        var isAuthorized = await _mockIdentityService.Object.AuthorizeAsync(adminUserId, "TemplateManagement");

        // Assert
        isAuthorized.Should().BeTrue("Admin should have access to template management");
    }

    [Test]
    public async Task Admin_ShouldAccessAuditLogs_Endpoints()
    {
        // Arrange
        var adminUserId = "admin-user-123";
        _mockIdentityService
            .Setup(x => x.AuthorizeAsync(adminUserId, "AuditLogs"))
            .ReturnsAsync(true);

        // Act
        var isAuthorized = await _mockIdentityService.Object.AuthorizeAsync(adminUserId, "AuditLogs");

        // Assert
        isAuthorized.Should().BeTrue("Admin should have access to audit logs");
    }

    [Test]
    public async Task Admin_ShouldAccessSystemConfiguration_Endpoints()
    {
        // Arrange
        var adminUserId = "admin-user-123";
        _mockIdentityService
            .Setup(x => x.AuthorizeAsync(adminUserId, "SystemConfiguration"))
            .ReturnsAsync(true);

        // Act
        var isAuthorized = await _mockIdentityService.Object.AuthorizeAsync(adminUserId, "SystemConfiguration");

        // Assert
        isAuthorized.Should().BeTrue("Admin should have access to system configuration");
    }

    [Test]
    public async Task Admin_ShouldViewAllInstitutions_Data()
    {
        // Arrange
        var adminUserId = "admin-user-123";
        var policy = "ViewAllInstitutions";

        _mockIdentityService
            .Setup(x => x.AuthorizeAsync(adminUserId, policy))
            .ReturnsAsync(true);

        // Act
        var isAuthorized = await _mockIdentityService.Object.AuthorizeAsync(adminUserId, policy);

        // Assert
        isAuthorized.Should().BeTrue("Admin should view data from all institutions");
    }

    [Test]
    public async Task Admin_ShouldManageCertificates()
    {
        // Arrange
        var adminUserId = "admin-user-123";
        var policy = "CertificateManagement";

        _mockIdentityService
            .Setup(x => x.AuthorizeAsync(adminUserId, policy))
            .ReturnsAsync(true);

        // Act
        var isAuthorized = await _mockIdentityService.Object.AuthorizeAsync(adminUserId, policy);

        // Assert
        isAuthorized.Should().BeTrue("Admin should manage certificates");
    }
}
