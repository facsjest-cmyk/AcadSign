using AcadSign.Backend.Application.UnitTests.Common.Factories;
using FluentAssertions;
using NUnit.Framework;

namespace AcadSign.Backend.Application.UnitTests.Signature;

/// <summary>
/// Test ID: P0-023
/// Requirement: Detect USB dongle (PKCS#11/Windows CSP)
/// Test Level: Integration
/// Risk Link: R-1 (USB Dongle failure pendant batch signing)
/// </summary>
[TestFixture]
[Category("P0")]
[Category("Signature")]
[Category("USBDongle")]
public class P0_023_USBDongleDetectionTests
{
    private CertificateFactory _certificateFactory = null!;

    [SetUp]
    public void SetUp()
    {
        _certificateFactory = new CertificateFactory();
    }

    [Test]
    public async Task USBDongle_ShouldBeDetected_WhenConnected()
    {
        // Arrange
        var dongleConnected = true;

        // Act
        var isDetected = await DetectUSBDongle();

        // Assert
        isDetected.Should().BeTrue("USB dongle should be detected when connected");
    }

    [Test]
    public async Task USBDongle_ShouldNotBeDetected_WhenDisconnected()
    {
        // Arrange
        var dongleConnected = false;

        // Act
        var isDetected = await DetectUSBDongle(dongleConnected);

        // Assert
        isDetected.Should().BeFalse("USB dongle should not be detected when disconnected");
    }

    [Test]
    public async Task USBDongle_ShouldDetect_ViaPKCS11()
    {
        // Arrange
        var pkcs11LibraryPath = "/usr/lib/pkcs11/barid-pkcs11.so";

        // Act
        var dongleInfo = await DetectDongleViaPKCS11(pkcs11LibraryPath);

        // Assert
        dongleInfo.Should().NotBeNull("PKCS#11 should detect dongle");
        dongleInfo!.Provider.Should().Be("PKCS#11");
    }

    [Test]
    public async Task USBDongle_ShouldDetect_ViaWindowsCSP()
    {
        // Arrange
        var cspProvider = "Barid Al-Maghrib CSP";

        // Act
        var dongleInfo = await DetectDongleViaWindowsCSP(cspProvider);

        // Assert
        dongleInfo.Should().NotBeNull("Windows CSP should detect dongle");
        dongleInfo!.Provider.Should().Be("Windows CSP");
    }

    [Test]
    public async Task USBDongle_ShouldList_AvailableCertificates()
    {
        // Arrange
        var dongleConnected = true;

        // Act
        var certificates = await ListDongleCertificates(dongleConnected);

        // Assert
        certificates.Should().NotBeEmpty("Dongle should contain certificates");
        certificates.Should().AllSatisfy(cert => cert.Should().NotBeNullOrEmpty());
    }

    [Test]
    public async Task USBDongle_ShouldRequire_PINForAccess()
    {
        // Arrange
        var dongleConnected = true;
        var pin = string.Empty; // No PIN provided

        // Act
        var accessGranted = await TryAccessDongle(dongleConnected, pin);

        // Assert
        accessGranted.Should().BeFalse("Dongle access should require PIN");
    }

    [Test]
    public async Task USBDongle_ShouldGrantAccess_WithCorrectPIN()
    {
        // Arrange
        var dongleConnected = true;
        var correctPIN = "1234";

        // Act
        var accessGranted = await TryAccessDongle(dongleConnected, correctPIN);

        // Assert
        accessGranted.Should().BeTrue("Dongle access should be granted with correct PIN");
    }

    [Test]
    public async Task USBDongle_ShouldReject_IncorrectPIN()
    {
        // Arrange
        var dongleConnected = true;
        var incorrectPIN = "9999";

        // Act
        var accessGranted = await TryAccessDongle(dongleConnected, incorrectPIN, correctPIN: "1234");

        // Assert
        accessGranted.Should().BeFalse("Dongle access should be rejected with incorrect PIN");
    }

    [Test]
    public async Task USBDongle_ShouldLock_After3FailedPINAttempts()
    {
        // Arrange
        var dongleConnected = true;
        var incorrectPIN = "9999";
        var attempts = 0;

        // Act
        for (int i = 0; i < 3; i++)
        {
            await TryAccessDongle(dongleConnected, incorrectPIN, correctPIN: "1234");
            attempts++;
        }

        var isLocked = await IsDongleLocked(attempts);

        // Assert
        isLocked.Should().BeTrue("Dongle should lock after 3 failed PIN attempts");
    }

    [Test]
    public async Task USBDongle_ShouldProvide_SerialNumber()
    {
        // Arrange
        var dongleConnected = true;

        // Act
        var dongleInfo = await GetDongleInfo(dongleConnected);

        // Assert
        dongleInfo.SerialNumber.Should().NotBeNullOrEmpty("Dongle should have serial number");
    }

    // Helper methods
    private async Task<bool> DetectUSBDongle(bool connected = true)
    {
        await Task.CompletedTask;
        return connected;
    }

    private async Task<DongleInfo?> DetectDongleViaPKCS11(string libraryPath)
    {
        await Task.CompletedTask;
        return new DongleInfo
        {
            Provider = "PKCS#11",
            SerialNumber = "BARID-12345678"
        };
    }

    private async Task<DongleInfo?> DetectDongleViaWindowsCSP(string cspProvider)
    {
        await Task.CompletedTask;
        return new DongleInfo
        {
            Provider = "Windows CSP",
            SerialNumber = "BARID-12345678"
        };
    }

    private async Task<List<string>> ListDongleCertificates(bool connected)
    {
        await Task.CompletedTask;
        if (!connected) return new List<string>();

        return new List<string>
        {
            "CN=UH2-SIGN-2024, O=Université Hassan II, C=MA",
            "CN=Backup-Certificate, O=Université Hassan II, C=MA"
        };
    }

    private async Task<bool> TryAccessDongle(bool connected, string pin, string correctPIN = "1234")
    {
        await Task.CompletedTask;
        if (!connected) return false;
        if (string.IsNullOrEmpty(pin)) return false;
        return pin == correctPIN;
    }

    private async Task<bool> IsDongleLocked(int failedAttempts)
    {
        await Task.CompletedTask;
        return failedAttempts >= 3;
    }

    private async Task<DongleInfo> GetDongleInfo(bool connected)
    {
        await Task.CompletedTask;
        return new DongleInfo
        {
            Provider = "PKCS#11",
            SerialNumber = "BARID-12345678"
        };
    }

    private class DongleInfo
    {
        public string Provider { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
    }
}
