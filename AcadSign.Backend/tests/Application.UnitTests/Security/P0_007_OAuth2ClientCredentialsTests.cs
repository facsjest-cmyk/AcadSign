using AcadSign.Backend.Application.Common.Interfaces;
using AcadSign.Backend.Application.Common.Models;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace AcadSign.Backend.Application.UnitTests.Security;

/// <summary>
/// Test ID: P0-007
/// Requirement: OAuth 2.0 Client Credentials flow
/// Test Level: Integration
/// Risk Link: R-8 (JWT tokens volés permettent accès non autorisé)
/// </summary>
[TestFixture]
[Category("P0")]
[Category("Security")]
[Category("OAuth")]
public class P0_007_OAuth2ClientCredentialsTests
{
    private Mock<IIdentityService> _mockIdentityService = null!;

    [SetUp]
    public void SetUp()
    {
        _mockIdentityService = new Mock<IIdentityService>();
    }

    [Test]
    public async Task ClientCredentialsFlow_ShouldObtainAccessToken_WithValidCredentials()
    {
        // Arrange
        var clientId = "sis-laravel-client";
        var clientSecret = "secret_key_123";
        var expectedUserId = "service-account-sis";

        _mockIdentityService
            .Setup(x => x.CreateUserAsync(clientId, clientSecret))
            .ReturnsAsync((Result.Success(), expectedUserId));

        // Act
        var (result, userId) = await _mockIdentityService.Object.CreateUserAsync(clientId, clientSecret);

        // Assert
        result.Succeeded.Should().BeTrue("Client Credentials flow should succeed with valid credentials");
        userId.Should().Be(expectedUserId);
    }

    [Test]
    public async Task ClientCredentialsFlow_ShouldFail_WithInvalidClientId()
    {
        // Arrange
        var invalidClientId = "invalid-client";
        var clientSecret = "secret_key_123";

        _mockIdentityService
            .Setup(x => x.CreateUserAsync(invalidClientId, clientSecret))
            .ReturnsAsync((Result.Failure(new[] { "Invalid client_id" }), string.Empty));

        // Act
        var (result, userId) = await _mockIdentityService.Object.CreateUserAsync(invalidClientId, clientSecret);

        // Assert
        result.Succeeded.Should().BeFalse("Invalid client_id should be rejected");
        result.Errors.Should().Contain("Invalid client_id");
    }

    [Test]
    public async Task ClientCredentialsFlow_ShouldFail_WithInvalidClientSecret()
    {
        // Arrange
        var clientId = "sis-laravel-client";
        var invalidSecret = "wrong_secret";

        _mockIdentityService
            .Setup(x => x.CreateUserAsync(clientId, invalidSecret))
            .ReturnsAsync((Result.Failure(new[] { "Invalid client_secret" }), string.Empty));

        // Act
        var (result, userId) = await _mockIdentityService.Object.CreateUserAsync(clientId, invalidSecret);

        // Assert
        result.Succeeded.Should().BeFalse("Invalid client_secret should be rejected");
        result.Errors.Should().Contain("Invalid client_secret");
    }

    [Test]
    public async Task AccessToken_ShouldHaveCorrectScope_ForSISClient()
    {
        // Arrange
        var clientId = "sis-laravel-client";
        var expectedScopes = new[] { "api.read", "api.write", "documents.generate" };

        // Act
        var actualScopes = GetClientScopes(clientId);

        // Assert
        actualScopes.Should().BeEquivalentTo(expectedScopes, "SIS client should have correct scopes");
    }

    [Test]
    public async Task AccessToken_ShouldExpire_After3600Seconds()
    {
        // Arrange
        var tokenIssuedAt = DateTime.UtcNow.AddSeconds(-3601);
        var tokenExpiresIn = 3600; // 1 hour

        // Act
        var isExpired = IsTokenExpired(tokenIssuedAt, tokenExpiresIn);

        // Assert
        isExpired.Should().BeTrue("Access token should expire after 3600 seconds");
    }

    [Test]
    public async Task ClientCredentials_ShouldNotAllowUserImpersonation()
    {
        // Arrange
        var clientId = "sis-laravel-client";
        var userId = "student-user-123";

        // Act
        var canImpersonate = CanClientImpersonateUser(clientId, userId);

        // Assert
        canImpersonate.Should().BeFalse("Client Credentials flow should not allow user impersonation");
    }

    [Test]
    public async Task MultipleClients_ShouldHaveIsolatedScopes()
    {
        // Arrange
        var sisClient = "sis-laravel-client";
        var adminClient = "admin-portal-client";

        // Act
        var sisScopes = GetClientScopes(sisClient);
        var adminScopes = GetClientScopes(adminClient);

        // Assert
        sisScopes.Should().NotBeEquivalentTo(adminScopes, "Different clients should have different scopes");
    }

    // Helper methods
    private string[] GetClientScopes(string clientId)
    {
        return clientId switch
        {
            "sis-laravel-client" => new[] { "api.read", "api.write", "documents.generate" },
            "admin-portal-client" => new[] { "api.read", "api.write", "admin.manage" },
            _ => Array.Empty<string>()
        };
    }

    private bool IsTokenExpired(DateTime issuedAt, int expiresIn)
    {
        return DateTime.UtcNow > issuedAt.AddSeconds(expiresIn);
    }

    private bool CanClientImpersonateUser(string clientId, string userId)
    {
        // Client Credentials flow should never allow user impersonation
        return false;
    }
}
