using FluentAssertions;
using NUnit.Framework;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace AcadSign.Backend.Application.UnitTests.Security;

/// <summary>
/// Test ID: P0-008
/// Requirement: JWT token validation
/// Test Level: Unit
/// Risk Link: R-8 (JWT tokens volés permettent accès non autorisé)
/// </summary>
[TestFixture]
[Category("P0")]
[Category("Security")]
[Category("JWT")]
public class P0_008_JWTTokenValidationTests
{
    private const string SecretKey = "ThisIsASecretKeyForTestingPurposesOnly123456789";
    private JwtSecurityTokenHandler _tokenHandler = null!;
    private TokenValidationParameters _validationParameters = null!;

    [SetUp]
    public void SetUp()
    {
        _tokenHandler = new JwtSecurityTokenHandler();
        _validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "AcadSign.Backend",
            ValidAudience = "AcadSign.Desktop",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey)),
            ClockSkew = TimeSpan.Zero
        };
    }

    [Test]
    public void ValidToken_ShouldBeAccepted()
    {
        // Arrange
        var token = GenerateValidToken();

        // Act
        var principal = _tokenHandler.ValidateToken(token, _validationParameters, out var validatedToken);

        // Assert
        principal.Should().NotBeNull("Valid token should be accepted");
        validatedToken.Should().NotBeNull();
        principal.Identity!.IsAuthenticated.Should().BeTrue();
    }

    [Test]
    public void ExpiredToken_ShouldBeRejected()
    {
        // Arrange
        var expiredToken = GenerateExpiredToken();

        // Act
        Action act = () => _tokenHandler.ValidateToken(expiredToken, _validationParameters, out _);

        // Assert
        act.Should().Throw<SecurityTokenExpiredException>("Expired token should be rejected");
    }

    [Test]
    public void TokenWithInvalidSignature_ShouldBeRejected()
    {
        // Arrange
        var token = GenerateValidToken();
        var tamperedToken = token.Substring(0, token.Length - 10) + "tampered12";

        // Act
        Action act = () => _tokenHandler.ValidateToken(tamperedToken, _validationParameters, out _);

        // Assert
        act.Should().Throw<SecurityTokenException>("Token with invalid signature should be rejected");
    }

    [Test]
    public void TokenWithInvalidIssuer_ShouldBeRejected()
    {
        // Arrange
        var token = GenerateTokenWithInvalidIssuer();

        // Act
        Action act = () => _tokenHandler.ValidateToken(token, _validationParameters, out _);

        // Assert
        act.Should().Throw<SecurityTokenInvalidIssuerException>("Token with invalid issuer should be rejected");
    }

    [Test]
    public void TokenWithInvalidAudience_ShouldBeRejected()
    {
        // Arrange
        var token = GenerateTokenWithInvalidAudience();

        // Act
        Action act = () => _tokenHandler.ValidateToken(token, _validationParameters, out _);

        // Assert
        act.Should().Throw<SecurityTokenInvalidAudienceException>("Token with invalid audience should be rejected");
    }

    [Test]
    public void Token_ShouldContainRequiredClaims()
    {
        // Arrange
        var token = GenerateValidToken();

        // Act
        var principal = _tokenHandler.ValidateToken(token, _validationParameters, out _);
        var claims = principal.Claims.ToList();

        // Assert
        claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier, "Token should contain user ID claim");
        claims.Should().Contain(c => c.Type == ClaimTypes.Name, "Token should contain username claim");
        claims.Should().Contain(c => c.Type == ClaimTypes.Role, "Token should contain role claim");
    }

    [Test]
    public void Token_ShouldHaveCorrectExpiration()
    {
        // Arrange
        var token = GenerateValidToken();

        // Act
        var jwtToken = _tokenHandler.ReadJwtToken(token);
        var expirationTime = jwtToken.ValidTo;
        var expectedExpiration = DateTime.UtcNow.AddHours(1);

        // Assert
        expirationTime.Should().BeCloseTo(expectedExpiration, TimeSpan.FromMinutes(1), 
            "Token should expire in approximately 1 hour");
    }

    [Test]
    public void MalformedToken_ShouldBeRejected()
    {
        // Arrange
        var malformedToken = "not.a.valid.jwt.token";

        // Act
        Action act = () => _tokenHandler.ValidateToken(malformedToken, _validationParameters, out _);

        // Assert
        act.Should().Throw<ArgumentException>("Malformed token should be rejected");
    }

    // Helper methods
    private string GenerateValidToken()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-123"),
            new Claim(ClaimTypes.Name, "test.user@uh2.ac.ma"),
            new Claim(ClaimTypes.Role, "Registrar")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "AcadSign.Backend",
            audience: "AcadSign.Desktop",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return _tokenHandler.WriteToken(token);
    }

    private string GenerateExpiredToken()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-123"),
            new Claim(ClaimTypes.Name, "test.user@uh2.ac.ma")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "AcadSign.Backend",
            audience: "AcadSign.Desktop",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(-10), // Expired 10 minutes ago
            signingCredentials: credentials
        );

        return _tokenHandler.WriteToken(token);
    }

    private string GenerateTokenWithInvalidIssuer()
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "user-123") };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "InvalidIssuer",
            audience: "AcadSign.Desktop",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return _tokenHandler.WriteToken(token);
    }

    private string GenerateTokenWithInvalidAudience()
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "user-123") };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "AcadSign.Backend",
            audience: "InvalidAudience",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return _tokenHandler.WriteToken(token);
    }
}
