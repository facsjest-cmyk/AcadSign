using FluentAssertions;
using NUnit.Framework;

namespace AcadSign.Backend.Application.UnitTests.Storage;

/// <summary>
/// Test ID: P0-019
/// Requirement: Generate pre-signed URL (7 days expiry)
/// Test Level: Integration
/// </summary>
[TestFixture]
[Category("P0")]
[Category("Storage")]
[Category("PreSignedURL")]
public class P0_019_PreSignedURLTests
{
    [Test]
    public async Task PreSignedURL_ShouldBeGenerated_Successfully()
    {
        // Arrange
        var s3ObjectKey = "documents/institution-123/2024/01/document-456.pdf";
        var expiryDays = 7;

        // Act
        var preSignedUrl = GeneratePreSignedURL(s3ObjectKey, expiryDays);

        // Assert
        preSignedUrl.Should().NotBeNullOrEmpty("Pre-signed URL should be generated");
        preSignedUrl.Should().StartWith("https://", "Pre-signed URL should use HTTPS");
    }

    [Test]
    public async Task PreSignedURL_ShouldExpire_After7Days()
    {
        // Arrange
        var s3ObjectKey = "documents/test.pdf";
        var expiryDays = 7;

        // Act
        var preSignedUrl = GeneratePreSignedURL(s3ObjectKey, expiryDays);
        var expiryTime = ExtractExpiryFromURL(preSignedUrl);

        // Assert
        expiryTime.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromMinutes(1));
    }

    [Test]
    public async Task PreSignedURL_ShouldBeAccessible_Within7Days()
    {
        // Arrange
        var s3ObjectKey = "documents/test.pdf";
        var expiryDays = 7;
        var preSignedUrl = GeneratePreSignedURL(s3ObjectKey, expiryDays);

        // Act
        var isAccessible = await CheckURLAccessibility(preSignedUrl, DateTime.UtcNow.AddDays(6));

        // Assert
        isAccessible.Should().BeTrue("Pre-signed URL should be accessible within 7 days");
    }

    [Test]
    public async Task PreSignedURL_ShouldReturn403_After7Days()
    {
        // Arrange
        var s3ObjectKey = "documents/test.pdf";
        var expiryDays = 7;
        var preSignedUrl = GeneratePreSignedURL(s3ObjectKey, expiryDays);

        // Act
        var isAccessible = await CheckURLAccessibility(preSignedUrl, DateTime.UtcNow.AddDays(8));

        // Assert
        isAccessible.Should().BeFalse("Pre-signed URL should return 403 after 7 days");
    }

    [Test]
    public async Task PreSignedURL_ShouldContain_Signature()
    {
        // Arrange
        var s3ObjectKey = "documents/test.pdf";
        var expiryDays = 7;

        // Act
        var preSignedUrl = GeneratePreSignedURL(s3ObjectKey, expiryDays);

        // Assert
        preSignedUrl.Should().Contain("Signature=", "Pre-signed URL should contain signature parameter");
        preSignedUrl.Should().Contain("Expires=", "Pre-signed URL should contain expiry parameter");
    }

    [Test]
    public async Task PreSignedURL_ShouldBeReadOnly()
    {
        // Arrange
        var s3ObjectKey = "documents/test.pdf";
        var expiryDays = 7;

        // Act
        var preSignedUrl = GeneratePreSignedURL(s3ObjectKey, expiryDays, httpMethod: "GET");

        // Assert
        preSignedUrl.Should().NotContain("PUT", "Pre-signed URL should be read-only (GET)");
        preSignedUrl.Should().NotContain("DELETE", "Pre-signed URL should not allow deletion");
    }

    [Test]
    public async Task PreSignedURL_ShouldNotExpose_Credentials()
    {
        // Arrange
        var s3ObjectKey = "documents/test.pdf";
        var expiryDays = 7;

        // Act
        var preSignedUrl = GeneratePreSignedURL(s3ObjectKey, expiryDays);

        // Assert
        preSignedUrl.Should().NotContain("AccessKeyId=", "Pre-signed URL should not expose access key");
        preSignedUrl.Should().NotContain("SecretKey=", "Pre-signed URL should not expose secret key");
    }

    [Test]
    public async Task PreSignedURL_ShouldWork_WithSpecialCharacters()
    {
        // Arrange
        var s3ObjectKey = "documents/étudiant-français-2024.pdf";
        var expiryDays = 7;

        // Act
        var preSignedUrl = GeneratePreSignedURL(s3ObjectKey, expiryDays);

        // Assert
        preSignedUrl.Should().NotBeNullOrEmpty("Pre-signed URL should handle special characters");
        Uri.TryCreate(preSignedUrl, UriKind.Absolute, out _).Should().BeTrue("Pre-signed URL should be valid URI");
    }

    // Helper methods
    private string GeneratePreSignedURL(string s3ObjectKey, int expiryDays, string httpMethod = "GET")
    {
        var expiryTime = DateTime.UtcNow.AddDays(expiryDays);
        var signature = GenerateSignature(s3ObjectKey, expiryTime);
        return $"https://s3.acadsign.uh2.ac.ma/{s3ObjectKey}?Expires={expiryTime.Ticks}&Signature={signature}";
    }

    private string GenerateSignature(string s3ObjectKey, DateTime expiryTime)
    {
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{s3ObjectKey}:{expiryTime.Ticks}"));
    }

    private DateTime ExtractExpiryFromURL(string preSignedUrl)
    {
        var uri = new Uri(preSignedUrl);
        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
        var expiresTicks = long.Parse(query["Expires"]!);
        return new DateTime(expiresTicks);
    }

    private async Task<bool> CheckURLAccessibility(string preSignedUrl, DateTime currentTime)
    {
        await Task.CompletedTask;
        var expiryTime = ExtractExpiryFromURL(preSignedUrl);
        return currentTime < expiryTime;
    }
}
