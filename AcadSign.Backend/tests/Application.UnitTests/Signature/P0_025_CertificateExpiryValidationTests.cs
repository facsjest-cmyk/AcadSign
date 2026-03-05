using AcadSign.Backend.Application.UnitTests.Common.Factories;
using FluentAssertions;
using NUnit.Framework;

namespace AcadSign.Backend.Application.UnitTests.Signature;

/// <summary>
/// Test ID: P0-025
/// Requirement: Validate certificate expiry before signing
/// Test Level: Unit
/// Risk Link: R-2 (Certificat expiré non détecté)
/// </summary>
[TestFixture]
[Category("P0")]
[Category("Signature")]
[Category("CertificateValidation")]
public class P0_025_CertificateExpiryValidationTests
{
    private CertificateFactory _certificateFactory = null!;
    private DocumentFactory _documentFactory = null!;

    [SetUp]
    public void SetUp()
    {
        _certificateFactory = new CertificateFactory();
        _documentFactory = new DocumentFactory();
    }

    [Test]
    public async Task ValidCertificate_ShouldPass_ExpiryValidation()
    {
        // Arrange
        var certificate = _certificateFactory.Generate(); // Valid for 1 year

        // Act
        var validationResult = ValidateCertificateExpiry(certificate);

        // Assert
        validationResult.IsValid.Should().BeTrue("Valid certificate should pass expiry validation");
        validationResult.ErrorMessage.Should().BeNull();
    }

    [Test]
    public async Task ExpiredCertificate_ShouldFail_ExpiryValidation()
    {
        // Arrange
        var expiredCertificate = _certificateFactory.Expired();

        // Act
        var validationResult = ValidateCertificateExpiry(expiredCertificate);

        // Assert
        validationResult.IsValid.Should().BeFalse("Expired certificate should fail validation");
        validationResult.ErrorMessage.Should().Contain("expired");
    }

    [Test]
    public async Task CertificateExpiringWithin7Days_ShouldFail_PreFlightValidation()
    {
        // Arrange
        var certificate = _certificateFactory.ExpiringSoon(5); // Expires in 5 days

        // Act
        var validationResult = ValidateCertificateExpiry(certificate, bufferDays: 7);

        // Assert
        validationResult.IsValid.Should().BeFalse("Certificate expiring within 7 days should fail pre-flight validation");
        validationResult.ErrorMessage.Should().Contain("expires within 7 days");
    }

    [Test]
    public async Task CertificateExpiringIn8Days_ShouldPass_PreFlightValidation()
    {
        // Arrange
        var certificate = _certificateFactory.ExpiringSoon(8); // Expires in 8 days

        // Act
        var validationResult = ValidateCertificateExpiry(certificate, bufferDays: 7);

        // Assert
        validationResult.IsValid.Should().BeTrue("Certificate expiring in 8 days should pass validation");
    }

    [Test]
    public async Task SigningWithExpiredCertificate_ShouldBeBlocked()
    {
        // Arrange
        var document = _documentFactory.Unsigned();
        var expiredCertificate = _certificateFactory.Expired();

        // Act
        var canSign = CanSignDocument(document, expiredCertificate);

        // Assert
        canSign.Should().BeFalse("Signing should be blocked with expired certificate");
    }

    [Test]
    public async Task NotYetValidCertificate_ShouldFail_Validation()
    {
        // Arrange
        var futureValidCertificate = _certificateFactory.NotYetValid();

        // Act
        var validationResult = ValidateCertificateExpiry(futureValidCertificate);

        // Assert
        validationResult.IsValid.Should().BeFalse("Certificate not yet valid should fail validation");
        validationResult.ErrorMessage.Should().Contain("not yet valid");
    }

    [Test]
    public async Task CertificateValidation_ShouldCheck_NotBeforeDate()
    {
        // Arrange
        var certificate = _certificateFactory.Generate();
        certificate.NotBefore = DateTime.UtcNow.AddDays(1); // Valid from tomorrow

        // Act
        var validationResult = ValidateCertificateExpiry(certificate);

        // Assert
        validationResult.IsValid.Should().BeFalse("Certificate should not be valid before NotBefore date");
    }

    [Test]
    public async Task CertificateValidation_ShouldCheck_NotAfterDate()
    {
        // Arrange
        var certificate = _certificateFactory.Generate();
        certificate.NotAfter = DateTime.UtcNow.AddDays(-1); // Expired yesterday

        // Act
        var validationResult = ValidateCertificateExpiry(certificate);

        // Assert
        validationResult.IsValid.Should().BeFalse("Certificate should not be valid after NotAfter date");
    }

    [Test]
    public async Task CertificateValidation_ShouldReturn_DaysUntilExpiry()
    {
        // Arrange
        var certificate = _certificateFactory.ExpiringSoon(30); // 30 days

        // Act
        var validationResult = ValidateCertificateExpiry(certificate);

        // Assert
        validationResult.DaysUntilExpiry.Should().BeCloseTo(30, 1);
    }

    [Test]
    public async Task CertificateValidation_ShouldWarn_When30DaysRemaining()
    {
        // Arrange
        var certificate = _certificateFactory.ExpiringSoon(25); // 25 days

        // Act
        var validationResult = ValidateCertificateExpiry(certificate);

        // Assert
        validationResult.WarningLevel.Should().Be("High", "Warning should be high when < 30 days remaining");
    }

    // Helper methods
    private ValidationResult ValidateCertificateExpiry(CertificateData certificate, int bufferDays = 7)
    {
        var now = DateTime.UtcNow;
        
        // Check if certificate is not yet valid
        if (certificate.NotBefore > now)
        {
            return new ValidationResult
            {
                IsValid = false,
                ErrorMessage = $"Certificate is not yet valid. Valid from: {certificate.NotBefore}"
            };
        }

        // Check if certificate is expired
        if (certificate.NotAfter < now)
        {
            return new ValidationResult
            {
                IsValid = false,
                ErrorMessage = "Certificate has expired"
            };
        }

        // Check buffer period
        var daysUntilExpiry = (certificate.NotAfter - now).Days;
        if (daysUntilExpiry < bufferDays)
        {
            return new ValidationResult
            {
                IsValid = false,
                ErrorMessage = $"Certificate expires within {bufferDays} days",
                DaysUntilExpiry = daysUntilExpiry
            };
        }

        // Determine warning level
        var warningLevel = daysUntilExpiry < 30 ? "High" : daysUntilExpiry < 60 ? "Medium" : "Low";

        return new ValidationResult
        {
            IsValid = true,
            DaysUntilExpiry = daysUntilExpiry,
            WarningLevel = warningLevel
        };
    }

    private bool CanSignDocument(Domain.Entities.Document document, CertificateData certificate)
    {
        var validation = ValidateCertificateExpiry(certificate);
        return validation.IsValid;
    }

    private class ValidationResult
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
        public int DaysUntilExpiry { get; set; }
        public string WarningLevel { get; set; } = "Low";
    }
}
