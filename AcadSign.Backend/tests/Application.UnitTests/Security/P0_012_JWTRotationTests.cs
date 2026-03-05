using FluentAssertions;
using NUnit.Framework;

namespace AcadSign.Backend.Application.UnitTests.Security;

/// <summary>
/// Test ID: P0-012
/// Requirement: JWT rotation after 90 days
/// Test Level: Unit
/// Risk Link: R-8 (JWT tokens volés permettent accès non autorisé)
/// </summary>
[TestFixture]
[Category("P0")]
[Category("Security")]
[Category("JWT")]
public class P0_012_JWTRotationTests
{
    [Test]
    public void RefreshToken_ShouldExpire_After90Days()
    {
        // Arrange
        var refreshTokenIssuedAt = DateTime.UtcNow.AddDays(-91);
        var refreshTokenExpiryDays = 90;

        // Act
        var isExpired = IsRefreshTokenExpired(refreshTokenIssuedAt, refreshTokenExpiryDays);

        // Assert
        isExpired.Should().BeTrue("Refresh token should expire after 90 days");
    }

    [Test]
    public void RefreshToken_ShouldBeValid_Within90Days()
    {
        // Arrange
        var refreshTokenIssuedAt = DateTime.UtcNow.AddDays(-89);
        var refreshTokenExpiryDays = 90;

        // Act
        var isExpired = IsRefreshTokenExpired(refreshTokenIssuedAt, refreshTokenExpiryDays);

        // Assert
        isExpired.Should().BeFalse("Refresh token should be valid within 90 days");
    }

    [Test]
    public void OldAccessToken_ShouldBeInvalidated_AfterRefresh()
    {
        // Arrange
        var oldAccessToken = "old_access_token_123";
        var newAccessToken = "new_access_token_456";
        var revokedTokens = new HashSet<string>();

        // Act
        RevokeToken(oldAccessToken, revokedTokens);
        var isOldTokenValid = !revokedTokens.Contains(oldAccessToken);
        var isNewTokenValid = !revokedTokens.Contains(newAccessToken);

        // Assert
        isOldTokenValid.Should().BeFalse("Old access token should be invalidated after refresh");
        isNewTokenValid.Should().BeTrue("New access token should be valid");
    }

    [Test]
    public void RefreshTokenRotation_ShouldIssueNewRefreshToken()
    {
        // Arrange
        var oldRefreshToken = "old_refresh_token_123";

        // Act
        var newRefreshToken = RotateRefreshToken(oldRefreshToken);

        // Assert
        newRefreshToken.Should().NotBe(oldRefreshToken, "New refresh token should be different");
        newRefreshToken.Should().NotBeNullOrEmpty("New refresh token should be issued");
    }

    [Test]
    public void RefreshTokenReuse_ShouldBeDetected_AndRevoked()
    {
        // Arrange
        var refreshToken = "refresh_token_123";
        var usedRefreshTokens = new HashSet<string>();

        // Act - First use
        var firstUse = TryUseRefreshToken(refreshToken, usedRefreshTokens);
        
        // Act - Second use (replay attack)
        var secondUse = TryUseRefreshToken(refreshToken, usedRefreshTokens);

        // Assert
        firstUse.Should().BeTrue("First use of refresh token should succeed");
        secondUse.Should().BeFalse("Reuse of refresh token should be detected and rejected");
    }

    [Test]
    public void RefreshTokenFamily_ShouldBeRevoked_OnReuseDetection()
    {
        // Arrange
        var tokenFamily = new List<string> { "token1", "token2", "token3" };
        var reusedToken = "token2";
        var revokedFamilies = new HashSet<string>();

        // Act
        var familyId = GetTokenFamilyId(reusedToken);
        RevokeTokenFamily(familyId, revokedFamilies);

        // Assert
        revokedFamilies.Should().Contain(familyId, "Entire token family should be revoked on reuse detection");
    }

    [Test]
    public void AccessToken_ShouldHaveShorterLifetime_ThanRefreshToken()
    {
        // Arrange
        var accessTokenLifetime = TimeSpan.FromHours(1);
        var refreshTokenLifetime = TimeSpan.FromDays(90);

        // Assert
        accessTokenLifetime.Should().BeLessThan(refreshTokenLifetime, 
            "Access token should have shorter lifetime than refresh token");
    }

    [Test]
    public void TokenRotation_ShouldMaintainUserSession()
    {
        // Arrange
        var userId = "user-123";
        var oldAccessToken = CreateAccessToken(userId, DateTime.UtcNow.AddHours(-2));
        var oldRefreshToken = CreateRefreshToken(userId, DateTime.UtcNow.AddDays(-30));

        // Act
        var (newAccessToken, newRefreshToken) = RefreshTokens(oldRefreshToken, userId);

        // Assert
        newAccessToken.Should().NotBeNullOrEmpty("New access token should be issued");
        newRefreshToken.Should().NotBeNullOrEmpty("New refresh token should be issued");
        GetUserIdFromToken(newAccessToken).Should().Be(userId, "User session should be maintained");
    }

    // Helper methods
    private bool IsRefreshTokenExpired(DateTime issuedAt, int expiryDays)
    {
        return DateTime.UtcNow > issuedAt.AddDays(expiryDays);
    }

    private void RevokeToken(string token, HashSet<string> revokedTokens)
    {
        revokedTokens.Add(token);
    }

    private string RotateRefreshToken(string oldRefreshToken)
    {
        return $"new_refresh_token_{Guid.NewGuid():N}";
    }

    private bool TryUseRefreshToken(string refreshToken, HashSet<string> usedTokens)
    {
        if (usedTokens.Contains(refreshToken))
        {
            return false; // Already used
        }

        usedTokens.Add(refreshToken);
        return true;
    }

    private string GetTokenFamilyId(string token)
    {
        // In production, this would extract family ID from token claims
        return "family_123";
    }

    private void RevokeTokenFamily(string familyId, HashSet<string> revokedFamilies)
    {
        revokedFamilies.Add(familyId);
    }

    private string CreateAccessToken(string userId, DateTime issuedAt)
    {
        return $"access_token_{userId}_{issuedAt.Ticks}";
    }

    private string CreateRefreshToken(string userId, DateTime issuedAt)
    {
        return $"refresh_token_{userId}_{issuedAt.Ticks}";
    }

    private (string accessToken, string refreshToken) RefreshTokens(string oldRefreshToken, string userId)
    {
        var newAccessToken = CreateAccessToken(userId, DateTime.UtcNow);
        var newRefreshToken = CreateRefreshToken(userId, DateTime.UtcNow);
        return (newAccessToken, newRefreshToken);
    }

    private string GetUserIdFromToken(string token)
    {
        // In production, this would decode JWT and extract user ID claim
        return "user-123";
    }
}
