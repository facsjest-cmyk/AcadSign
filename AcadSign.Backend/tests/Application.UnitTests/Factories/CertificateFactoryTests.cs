using AcadSign.Backend.Application.UnitTests.Common.Factories;
using FluentAssertions;
using NUnit.Framework;

namespace AcadSign.Backend.Application.UnitTests.Factories;

/// <summary>
/// Tests for CertificateFactory to ensure test data generation works correctly
/// Test ID: P0-025 to P0-028 (Certificate validation tests)
/// </summary>
[TestFixture]
public class CertificateFactoryTests
{
    private CertificateFactory _factory = null!;

    [SetUp]
    public void Setup()
    {
        _factory = new CertificateFactory();
    }

    [Test]
    public void Generate_ShouldCreateValidCertificate()
    {
        // Act
        var certificate = _factory.Generate();

        // Assert
        certificate.Should().NotBeNull();
        certificate.SerialNumber.Should().NotBeNullOrEmpty();
        certificate.SerialNumber.Length.Should().Be(16);
        certificate.Subject.Should().Contain("CN=UH2-SIGN-2024");
        certificate.Issuer.Should().Contain("Barid Al-Maghrib Root CA");
        certificate.NotBefore.Should().BeBefore(DateTime.UtcNow);
        certificate.NotAfter.Should().BeAfter(DateTime.UtcNow);
        certificate.Thumbprint.Should().NotBeNullOrEmpty();
        certificate.PublicKey.Should().NotBeNullOrEmpty();
        certificate.IsValid.Should().BeTrue();
        certificate.IsExpired.Should().BeFalse();
    }

    [Test]
    public void ExpiringSoon_ShouldCreateCertificateExpiringWithinSpecifiedDays()
    {
        // Arrange
        var daysUntilExpiry = 15;

        // Act
        var certificate = _factory.ExpiringSoon(daysUntilExpiry);

        // Assert
        certificate.DaysUntilExpiry.Should().BeLessOrEqualTo(daysUntilExpiry);
        certificate.DaysUntilExpiry.Should().BeGreaterOrEqualTo(daysUntilExpiry - 1); // Allow for timing precision
        certificate.ExpiresWithin(30).Should().BeTrue();
        certificate.IsExpired.Should().BeFalse();
        certificate.IsValid.Should().BeTrue();
    }

    [Test]
    public void Expired_ShouldCreateExpiredCertificate()
    {
        // Act
        var certificate = _factory.Expired();

        // Assert
        certificate.NotAfter.Should().BeBefore(DateTime.UtcNow);
        certificate.IsExpired.Should().BeTrue();
        certificate.IsValid.Should().BeFalse();
        certificate.DaysUntilExpiry.Should().BeNegative();
    }

    [Test]
    public void NotYetValid_ShouldCreateCertificateNotYetValid()
    {
        // Act
        var certificate = _factory.NotYetValid();

        // Assert
        certificate.NotBefore.Should().BeAfter(DateTime.UtcNow);
        certificate.IsValid.Should().BeFalse();
    }

    [Test]
    public void WithExpiry_ShouldSetSpecificExpiryDate()
    {
        // Arrange
        var expiryDate = DateTime.UtcNow.AddMonths(6);

        // Act
        var certificate = _factory.WithExpiry(expiryDate);

        // Assert
        certificate.NotAfter.Should().BeCloseTo(expiryDate, TimeSpan.FromSeconds(1));
        certificate.IsValid.Should().BeTrue();
    }

    [Test]
    public void ForInstitution_ShouldSetCustomInstitutionName()
    {
        // Arrange
        var institutionName = "Université Mohammed V";

        // Act
        var certificate = _factory.ForInstitution(institutionName);

        // Assert
        certificate.Subject.Should().Contain(institutionName);
    }

    [Test]
    public void Revoked_ShouldCreateRevokedCertificate()
    {
        // Act
        var certificate = _factory.Revoked();

        // Assert
        certificate.IsValid.Should().BeFalse();
        certificate.RevocationReason.Should().Be("KEY_COMPROMISE");
        certificate.RevokedAt.Should().NotBeNull();
        certificate.RevokedAt.Should().BeBefore(DateTime.UtcNow);
    }

    [Test]
    public void ExpiresWithin_ShouldReturnTrueWhenExpiringWithinDays()
    {
        // Arrange
        var certificate = _factory.ExpiringSoon(10);

        // Act & Assert
        certificate.ExpiresWithin(30).Should().BeTrue();
        certificate.ExpiresWithin(10).Should().BeTrue();
        certificate.ExpiresWithin(5).Should().BeFalse();
    }

    [Test]
    public void DaysUntilExpiry_ShouldCalculateCorrectly()
    {
        // Arrange
        var daysUntilExpiry = 45;
        var certificate = _factory.ExpiringSoon(daysUntilExpiry);

        // Act
        var actualDays = certificate.DaysUntilExpiry;

        // Assert
        actualDays.Should().BeCloseTo(daysUntilExpiry, 1);
    }

    [Test]
    public void Generate_ShouldCreateDifferentCertificatesEachTime()
    {
        // Act
        var cert1 = _factory.Generate();
        var cert2 = _factory.Generate();

        // Assert
        cert1.SerialNumber.Should().NotBe(cert2.SerialNumber);
        cert1.Thumbprint.Should().NotBe(cert2.Thumbprint);
        cert1.PublicKey.Should().NotBe(cert2.PublicKey);
    }
}
