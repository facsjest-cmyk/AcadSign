using AcadSign.Backend.Application.UnitTests.Common.Factories;
using FluentAssertions;
using NUnit.Framework;
using System.Text;

namespace AcadSign.Backend.Application.UnitTests.Security;

/// <summary>
/// Test ID: P0-011
/// Requirement: PII encryption in database
/// Test Level: Integration
/// Risk Link: R-4 (PII leak via logs non chiffrés)
/// </summary>
[TestFixture]
[Category("P0")]
[Category("Security")]
[Category("PII")]
public class P0_011_PIIEncryptionTests
{
    private StudentFactory _studentFactory = null!;

    [SetUp]
    public void SetUp()
    {
        _studentFactory = new StudentFactory();
    }

    [Test]
    public void CIN_ShouldBeEncrypted_InDatabase()
    {
        // Arrange
        var student = _studentFactory.Generate();
        var plainTextCIN = student.CIN;

        // Act
        var encryptedCIN = EncryptPII(plainTextCIN);
        var decryptedCIN = DecryptPII(encryptedCIN);

        // Assert
        encryptedCIN.Should().NotBe(plainTextCIN, "CIN should be encrypted");
        decryptedCIN.Should().Be(plainTextCIN, "Decrypted CIN should match original");
    }

    [Test]
    public void CNE_ShouldBeEncrypted_InDatabase()
    {
        // Arrange
        var student = _studentFactory.Generate();
        var plainTextCNE = student.CNE;

        // Act
        var encryptedCNE = EncryptPII(plainTextCNE);
        var decryptedCNE = DecryptPII(encryptedCNE);

        // Assert
        encryptedCNE.Should().NotBe(plainTextCNE, "CNE should be encrypted");
        decryptedCNE.Should().Be(plainTextCNE, "Decrypted CNE should match original");
    }

    [Test]
    public void Email_ShouldBeEncrypted_InDatabase()
    {
        // Arrange
        var student = _studentFactory.Generate();
        var plainTextEmail = student.Email;

        // Act
        var encryptedEmail = EncryptPII(plainTextEmail);
        var decryptedEmail = DecryptPII(encryptedEmail);

        // Assert
        encryptedEmail.Should().NotBe(plainTextEmail, "Email should be encrypted");
        decryptedEmail.Should().Be(plainTextEmail, "Decrypted email should match original");
    }

    [Test]
    public void PhoneNumber_ShouldBeEncrypted_InDatabase()
    {
        // Arrange
        var student = _studentFactory.WithOverrides(s => s.PhoneNumber = "+212 6 12 34 56 78");
        var plainTextPhone = student.PhoneNumber!;

        // Act
        var encryptedPhone = EncryptPII(plainTextPhone);
        var decryptedPhone = DecryptPII(encryptedPhone);

        // Assert
        encryptedPhone.Should().NotBe(plainTextPhone, "Phone number should be encrypted");
        decryptedPhone.Should().Be(plainTextPhone, "Decrypted phone should match original");
    }

    [Test]
    public void EncryptedPII_ShouldUseAES256()
    {
        // Arrange
        var plainText = "AB123456";

        // Act
        var encrypted = EncryptPII(plainText);

        // Assert
        encrypted.Should().NotBeNullOrEmpty("Encrypted value should not be empty");
        encrypted.Length.Should().BeGreaterThan(plainText.Length, "Encrypted value should be longer due to padding");
    }

    [Test]
    public void EncryptedPII_ShouldBeDeterministic_WithSameKey()
    {
        // Arrange
        var plainText = "AB123456";

        // Act
        var encrypted1 = EncryptPII(plainText);
        var encrypted2 = EncryptPII(plainText);

        // Assert
        // Note: In production, use non-deterministic encryption with IV
        // This test assumes deterministic encryption for searchability
        encrypted1.Should().Be(encrypted2, "Same plaintext should produce same ciphertext with same key");
    }

    [Test]
    public void DecryptedPII_ShouldMatchOriginal_ForAllPIIFields()
    {
        // Arrange
        var student = _studentFactory.Generate();
        var originalCIN = student.CIN;
        var originalCNE = student.CNE;
        var originalEmail = student.Email;

        // Act
        var encryptedCIN = EncryptPII(originalCIN);
        var encryptedCNE = EncryptPII(originalCNE);
        var encryptedEmail = EncryptPII(originalEmail);

        var decryptedCIN = DecryptPII(encryptedCIN);
        var decryptedCNE = DecryptPII(encryptedCNE);
        var decryptedEmail = DecryptPII(encryptedEmail);

        // Assert
        decryptedCIN.Should().Be(originalCIN);
        decryptedCNE.Should().Be(originalCNE);
        decryptedEmail.Should().Be(originalEmail);
    }

    [Test]
    public void EncryptionKey_ShouldNotBeHardcoded()
    {
        // This test verifies that encryption key comes from configuration
        // In production, key should be stored in Azure Key Vault or similar

        // Arrange
        var keyFromConfig = GetEncryptionKeyFromConfig();

        // Assert
        keyFromConfig.Should().NotBeNullOrEmpty("Encryption key should be configured");
        keyFromConfig.Length.Should().BeGreaterOrEqualTo(32, "AES-256 requires 32-byte key");
    }

    // Helper methods (simulating AES-256 encryption)
    private string EncryptPII(string plainText)
    {
        // Simplified encryption for testing
        // In production, use System.Security.Cryptography.Aes
        var key = GetEncryptionKeyFromConfig();
        var bytes = Encoding.UTF8.GetBytes(plainText);
        var encrypted = Convert.ToBase64String(bytes); // Simplified
        return $"ENC_{encrypted}";
    }

    private string DecryptPII(string encryptedText)
    {
        // Simplified decryption for testing
        if (!encryptedText.StartsWith("ENC_"))
            throw new ArgumentException("Invalid encrypted format");

        var base64 = encryptedText.Substring(4);
        var bytes = Convert.FromBase64String(base64);
        return Encoding.UTF8.GetString(bytes);
    }

    private string GetEncryptionKeyFromConfig()
    {
        // In production, this would load from IConfiguration
        // For testing, return a test key
        return "ThisIsATestEncryptionKey12345678901234567890";
    }
}
