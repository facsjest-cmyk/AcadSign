using Bogus;

namespace AcadSign.Backend.Application.UnitTests.Common.Factories;

/// <summary>
/// Factory for generating test Certificate data with realistic fake data.
/// Represents Barid Al-Maghrib e-Sign certificates for testing.
/// </summary>
public class CertificateFactory
{
    private readonly Faker _faker;

    public CertificateFactory()
    {
        _faker = new Faker();
    }

    /// <summary>
    /// Generate a valid certificate with default expiry (1 year from now)
    /// </summary>
    public CertificateData Generate()
    {
        return new CertificateData
        {
            SerialNumber = _faker.Random.AlphaNumeric(16).ToUpper(),
            Subject = $"CN=UH2-SIGN-2024, O=Université Hassan II, C=MA",
            Issuer = "CN=Barid Al-Maghrib Root CA, O=Barid eBank, C=MA",
            NotBefore = DateTime.UtcNow.AddDays(-30),
            NotAfter = DateTime.UtcNow.AddYears(1),
            Thumbprint = _faker.Random.AlphaNumeric(40).ToUpper(),
            PublicKey = Convert.ToBase64String(_faker.Random.Bytes(294)),
            IsValid = true
        };
    }

    /// <summary>
    /// Generate a certificate that expires soon (within 30 days)
    /// </summary>
    public CertificateData ExpiringSoon(int daysUntilExpiry = 15)
    {
        var cert = Generate();
        cert.NotAfter = DateTime.UtcNow.AddDays(daysUntilExpiry);
        return cert;
    }

    /// <summary>
    /// Generate an expired certificate
    /// </summary>
    public CertificateData Expired()
    {
        var cert = Generate();
        cert.NotBefore = DateTime.UtcNow.AddYears(-2);
        cert.NotAfter = DateTime.UtcNow.AddDays(-1);
        cert.IsValid = false;
        return cert;
    }

    /// <summary>
    /// Generate a certificate not yet valid (NotBefore in future)
    /// </summary>
    public CertificateData NotYetValid()
    {
        var cert = Generate();
        cert.NotBefore = DateTime.UtcNow.AddDays(1);
        cert.NotAfter = DateTime.UtcNow.AddYears(1).AddDays(1);
        cert.IsValid = false;
        return cert;
    }

    /// <summary>
    /// Generate a certificate with custom expiry date
    /// </summary>
    public CertificateData WithExpiry(DateTime expiryDate)
    {
        var cert = Generate();
        cert.NotAfter = expiryDate;
        cert.IsValid = expiryDate > DateTime.UtcNow;
        return cert;
    }

    /// <summary>
    /// Generate a certificate for a specific institution
    /// </summary>
    public CertificateData ForInstitution(string institutionName)
    {
        var cert = Generate();
        cert.Subject = $"CN={institutionName}-SIGN-2024, O={institutionName}, C=MA";
        return cert;
    }

    /// <summary>
    /// Generate a revoked certificate
    /// </summary>
    public CertificateData Revoked()
    {
        var cert = Generate();
        cert.IsValid = false;
        cert.RevocationReason = "KEY_COMPROMISE";
        cert.RevokedAt = DateTime.UtcNow.AddDays(-7);
        return cert;
    }
}

/// <summary>
/// Certificate data model for testing
/// </summary>
public class CertificateData
{
    public string SerialNumber { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public DateTime NotBefore { get; set; }
    public DateTime NotAfter { get; set; }
    public string Thumbprint { get; set; } = string.Empty;
    public string PublicKey { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public string? RevocationReason { get; set; }
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// Check if certificate is expired
    /// </summary>
    public bool IsExpired => DateTime.UtcNow > NotAfter;

    /// <summary>
    /// Get days until expiry (negative if expired)
    /// </summary>
    public int DaysUntilExpiry => (NotAfter - DateTime.UtcNow).Days;

    /// <summary>
    /// Check if certificate expires within specified days
    /// </summary>
    public bool ExpiresWithin(int days) => DaysUntilExpiry <= days && DaysUntilExpiry >= 0;
}
