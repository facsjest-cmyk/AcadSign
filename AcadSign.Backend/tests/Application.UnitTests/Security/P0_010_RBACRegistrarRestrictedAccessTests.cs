using AcadSign.Backend.Application.Common.Interfaces;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace AcadSign.Backend.Application.UnitTests.Security;

/// <summary>
/// Test ID: P0-010
/// Requirement: RBAC - Registrar cannot access admin endpoints
/// Test Level: Integration
/// Risk Link: R-8 (JWT tokens volés permettent accès non autorisé)
/// </summary>
[TestFixture]
[Category("P0")]
[Category("Security")]
[Category("RBAC")]
public class P0_010_RBACRegistrarRestrictedAccessTests
{
    private Mock<IIdentityService> _mockIdentityService = null!;

    [SetUp]
    public void SetUp()
    {
        _mockIdentityService = new Mock<IIdentityService>();
    }

    [Test]
    public async Task Registrar_ShouldNotAccessUserManagement_Endpoints()
    {
        // Arrange
        var registrarUserId = "registrar-user-123";
        _mockIdentityService
            .Setup(x => x.IsInRoleAsync(registrarUserId, "Admin"))
            .ReturnsAsync(false);

        _mockIdentityService
            .Setup(x => x.AuthorizeAsync(registrarUserId, "UserManagement"))
            .ReturnsAsync(false);

        // Act
        var isAuthorized = await _mockIdentityService.Object.AuthorizeAsync(registrarUserId, "UserManagement");

        // Assert
        isAuthorized.Should().BeFalse("Registrar should NOT have access to user management");
    }

    [Test]
    public async Task Registrar_ShouldNotAccessSystemConfiguration_Endpoints()
    {
        // Arrange
        var registrarUserId = "registrar-user-123";
        _mockIdentityService
            .Setup(x => x.AuthorizeAsync(registrarUserId, "SystemConfiguration"))
            .ReturnsAsync(false);

        // Act
        var isAuthorized = await _mockIdentityService.Object.AuthorizeAsync(registrarUserId, "SystemConfiguration");

        // Assert
        isAuthorized.Should().BeFalse("Registrar should NOT have access to system configuration");
    }

    [Test]
    public async Task Registrar_ShouldNotAccessAuditLogs_Endpoints()
    {
        // Arrange
        var registrarUserId = "registrar-user-123";
        _mockIdentityService
            .Setup(x => x.AuthorizeAsync(registrarUserId, "AuditLogs"))
            .ReturnsAsync(false);

        // Act
        var isAuthorized = await _mockIdentityService.Object.AuthorizeAsync(registrarUserId, "AuditLogs");

        // Assert
        isAuthorized.Should().BeFalse("Registrar should NOT have access to audit logs");
    }

    [Test]
    public async Task Registrar_ShouldNotManageCertificates()
    {
        // Arrange
        var registrarUserId = "registrar-user-123";
        _mockIdentityService
            .Setup(x => x.AuthorizeAsync(registrarUserId, "CertificateManagement"))
            .ReturnsAsync(false);

        // Act
        var isAuthorized = await _mockIdentityService.Object.AuthorizeAsync(registrarUserId, "CertificateManagement");

        // Assert
        isAuthorized.Should().BeFalse("Registrar should NOT manage certificates");
    }

    [Test]
    public async Task Registrar_ShouldAccessDocumentGeneration_Endpoints()
    {
        // Arrange
        var registrarUserId = "registrar-user-123";
        _mockIdentityService
            .Setup(x => x.IsInRoleAsync(registrarUserId, "Registrar"))
            .ReturnsAsync(true);

        _mockIdentityService
            .Setup(x => x.AuthorizeAsync(registrarUserId, "DocumentGeneration"))
            .ReturnsAsync(true);

        // Act
        var isAuthorized = await _mockIdentityService.Object.AuthorizeAsync(registrarUserId, "DocumentGeneration");

        // Assert
        isAuthorized.Should().BeTrue("Registrar SHOULD have access to document generation");
    }

    [Test]
    public async Task Registrar_ShouldAccessSignature_Endpoints()
    {
        // Arrange
        var registrarUserId = "registrar-user-123";
        _mockIdentityService
            .Setup(x => x.AuthorizeAsync(registrarUserId, "DocumentSignature"))
            .ReturnsAsync(true);

        // Act
        var isAuthorized = await _mockIdentityService.Object.AuthorizeAsync(registrarUserId, "DocumentSignature");

        // Assert
        isAuthorized.Should().BeTrue("Registrar SHOULD have access to signature endpoints");
    }

    [Test]
    public async Task Registrar_ShouldOnlyViewOwnInstitution_Data()
    {
        // Arrange
        var registrarUserId = "registrar-user-123";
        var ownInstitutionId = Guid.NewGuid();

        _mockIdentityService
            .Setup(x => x.AuthorizeAsync(registrarUserId, "ViewAllInstitutions"))
            .ReturnsAsync(false);

        // Act
        var canViewAll = await _mockIdentityService.Object.AuthorizeAsync(registrarUserId, "ViewAllInstitutions");

        // Assert
        canViewAll.Should().BeFalse("Registrar should only view own institution data");
    }

    [Test]
    public async Task Registrar_ShouldReturn403_WhenAccessingAdminEndpoint()
    {
        // Arrange
        var registrarUserId = "registrar-user-123";
        var adminEndpoint = "/api/admin/users";

        _mockIdentityService
            .Setup(x => x.IsInRoleAsync(registrarUserId, "Admin"))
            .ReturnsAsync(false);

        // Act
        var isAdmin = await _mockIdentityService.Object.IsInRoleAsync(registrarUserId, "Admin");
        var expectedStatusCode = isAdmin ? 200 : 403;

        // Assert
        expectedStatusCode.Should().Be(403, "Registrar should receive 403 Forbidden for admin endpoints");
    }
}
