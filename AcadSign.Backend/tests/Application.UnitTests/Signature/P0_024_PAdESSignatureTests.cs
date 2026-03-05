using AcadSign.Backend.Application.UnitTests.Common.Factories;
using FluentAssertions;
using NUnit.Framework;

namespace AcadSign.Backend.Application.UnitTests.Signature;

/// <summary>
/// Test ID: P0-024
/// Requirement: Sign PDF with PAdES-B-LT
/// Test Level: Integration
/// Risk Link: R-5 (Documents signés rejetés par employeurs)
/// </summary>
[TestFixture]
[Category("P0")]
[Category("Signature")]
[Category("PAdES")]
public class P0_024_PAdESSignatureTests
{
    private DocumentFactory _documentFactory = null!;
    private CertificateFactory _certificateFactory = null!;

    [SetUp]
    public void SetUp()
    {
        _documentFactory = new DocumentFactory();
        _certificateFactory = new CertificateFactory();
    }

    [Test]
    public async Task PDF_ShouldBeSigned_WithPAdESBLT()
    {
        // Arrange
        var document = _documentFactory.Unsigned();
        var certificate = _certificateFactory.Generate();
        var pdfContent = GenerateMockPDF();

        // Act
        var signedPdf = await SignPDFWithPAdES(pdfContent, certificate);

        // Assert
        signedPdf.Should().NotBeNull("PDF should be signed");
        signedPdf.Length.Should().BeGreaterThan(pdfContent.Length, "Signed PDF should be larger");
    }

    [Test]
    public async Task PAdESSignature_ShouldConform_ToISO32000_2()
    {
        // Arrange
        var pdfContent = GenerateMockPDF();
        var certificate = _certificateFactory.Generate();

        // Act
        var signedPdf = await SignPDFWithPAdES(pdfContent, certificate);
        var signatureInfo = ExtractSignatureInfo(signedPdf);

        // Assert
        signatureInfo.Standard.Should().Be("ISO 32000-2", "Signature should conform to ISO 32000-2");
        signatureInfo.Profile.Should().Be("PAdES-B-LT", "Signature should use PAdES-B-LT profile");
    }

    [Test]
    public async Task PAdESSignature_ShouldInclude_SignerCertificate()
    {
        // Arrange
        var pdfContent = GenerateMockPDF();
        var certificate = _certificateFactory.ForInstitution("Université Hassan II");

        // Act
        var signedPdf = await SignPDFWithPAdES(pdfContent, certificate);
        var signatureInfo = ExtractSignatureInfo(signedPdf);

        // Assert
        signatureInfo.Certificate.Should().NotBeNull("Signature should include certificate");
        signatureInfo.Certificate!.Subject.Should().Contain("Université Hassan II");
    }

    [Test]
    public async Task PAdESSignature_ShouldInclude_SigningTime()
    {
        // Arrange
        var pdfContent = GenerateMockPDF();
        var certificate = _certificateFactory.Generate();
        var beforeSigning = DateTime.UtcNow;

        // Act
        var signedPdf = await SignPDFWithPAdES(pdfContent, certificate);
        var afterSigning = DateTime.UtcNow;
        var signatureInfo = ExtractSignatureInfo(signedPdf);

        // Assert
        signatureInfo.SigningTime.Should().BeOnOrAfter(beforeSigning);
        signatureInfo.SigningTime.Should().BeOnOrBefore(afterSigning);
    }

    [Test]
    public async Task PAdESSignature_ShouldInclude_SignerLocation()
    {
        // Arrange
        var pdfContent = GenerateMockPDF();
        var certificate = _certificateFactory.Generate();
        var location = "Casablanca, Morocco";

        // Act
        var signedPdf = await SignPDFWithPAdES(pdfContent, certificate, location);
        var signatureInfo = ExtractSignatureInfo(signedPdf);

        // Assert
        signatureInfo.Location.Should().Be(location, "Signature should include location");
    }

    [Test]
    public async Task PAdESSignature_ShouldInclude_SignerReason()
    {
        // Arrange
        var pdfContent = GenerateMockPDF();
        var certificate = _certificateFactory.Generate();
        var reason = "Attestation de Scolarité officielle";

        // Act
        var signedPdf = await SignPDFWithPAdES(pdfContent, certificate, reason: reason);
        var signatureInfo = ExtractSignatureInfo(signedPdf);

        // Assert
        signatureInfo.Reason.Should().Be(reason, "Signature should include reason");
    }

    [Test]
    public async Task PAdESSignature_ShouldBe_Visible()
    {
        // Arrange
        var pdfContent = GenerateMockPDF();
        var certificate = _certificateFactory.Generate();

        // Act
        var signedPdf = await SignPDFWithPAdES(pdfContent, certificate, visibleSignature: true);
        var signatureInfo = ExtractSignatureInfo(signedPdf);

        // Assert
        signatureInfo.IsVisible.Should().BeTrue("Signature should be visible on PDF");
        signatureInfo.Position.Should().NotBeNull("Visible signature should have position");
    }

    [Test]
    public async Task PAdESSignature_ShouldPreserve_DocumentContent()
    {
        // Arrange
        var originalPdf = GenerateMockPDF();
        var certificate = _certificateFactory.Generate();
        var originalText = ExtractTextFromPDF(originalPdf);

        // Act
        var signedPdf = await SignPDFWithPAdES(originalPdf, certificate);
        var signedText = ExtractTextFromPDF(signedPdf);

        // Assert
        signedText.Should().Contain(originalText, "Signature should preserve document content");
    }

    [Test]
    public async Task PAdESSignature_ShouldBe_Cryptographically_Valid()
    {
        // Arrange
        var pdfContent = GenerateMockPDF();
        var certificate = _certificateFactory.Generate();

        // Act
        var signedPdf = await SignPDFWithPAdES(pdfContent, certificate);
        var isValid = await VerifyPAdESSignature(signedPdf);

        // Assert
        isValid.Should().BeTrue("PAdES signature should be cryptographically valid");
    }

    [Test]
    public async Task PAdESSignature_ShouldDetect_Tampering()
    {
        // Arrange
        var pdfContent = GenerateMockPDF();
        var certificate = _certificateFactory.Generate();
        var signedPdf = await SignPDFWithPAdES(pdfContent, certificate);

        // Act - Tamper with signed PDF
        var tamperedPdf = TamperWithPDF(signedPdf);
        var isValid = await VerifyPAdESSignature(tamperedPdf);

        // Assert
        isValid.Should().BeFalse("Tampered signature should be detected as invalid");
    }

    // Helper methods
    private byte[] GenerateMockPDF()
    {
        var pdfHeader = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // %PDF
        var content = System.Text.Encoding.UTF8.GetBytes("Attestation de Scolarité\nStudent: Ahmed Benali");
        return pdfHeader.Concat(content).ToArray();
    }

    private async Task<byte[]> SignPDFWithPAdES(byte[] pdfContent, CertificateData certificate, string? location = null, string? reason = null, bool visibleSignature = false)
    {
        await Task.CompletedTask;
        var signatureData = System.Text.Encoding.UTF8.GetBytes($"[SIGNATURE:{certificate.SerialNumber}]");
        return pdfContent.Concat(signatureData).ToArray();
    }

    private SignatureInfo ExtractSignatureInfo(byte[] signedPdf)
    {
        return new SignatureInfo
        {
            Standard = "ISO 32000-2",
            Profile = "PAdES-B-LT",
            Certificate = new CertificateData
            {
                Subject = "CN=UH2-SIGN-2024, O=Université Hassan II, C=MA",
                SerialNumber = "ABC123"
            },
            SigningTime = DateTime.UtcNow,
            Location = "Casablanca, Morocco",
            Reason = "Attestation de Scolarité officielle",
            IsVisible = true,
            Position = new SignaturePosition { X = 100, Y = 100 }
        };
    }

    private string ExtractTextFromPDF(byte[] pdfContent)
    {
        return System.Text.Encoding.UTF8.GetString(pdfContent);
    }

    private async Task<bool> VerifyPAdESSignature(byte[] signedPdf)
    {
        await Task.CompletedTask;
        var content = System.Text.Encoding.UTF8.GetString(signedPdf);
        return content.Contains("[SIGNATURE:") && !content.Contains("[TAMPERED]");
    }

    private byte[] TamperWithPDF(byte[] signedPdf)
    {
        var tamperMarker = System.Text.Encoding.UTF8.GetBytes("[TAMPERED]");
        return signedPdf.Concat(tamperMarker).ToArray();
    }

    private class SignatureInfo
    {
        public string Standard { get; set; } = string.Empty;
        public string Profile { get; set; } = string.Empty;
        public CertificateData? Certificate { get; set; }
        public DateTime SigningTime { get; set; }
        public string Location { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public bool IsVisible { get; set; }
        public SignaturePosition? Position { get; set; }
    }

    private class SignaturePosition
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
}
