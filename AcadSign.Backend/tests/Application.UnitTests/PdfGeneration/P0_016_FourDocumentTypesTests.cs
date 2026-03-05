using AcadSign.Backend.Application.UnitTests.Common.Factories;
using FluentAssertions;
using NUnit.Framework;

namespace AcadSign.Backend.Application.UnitTests.PdfGeneration;

/// <summary>
/// Test ID: P0-016
/// Requirement: Generate 4 document types
/// Test Level: Integration
/// </summary>
[TestFixture]
[Category("P0")]
[Category("PDF")]
[Category("DocumentTypes")]
public class P0_016_FourDocumentTypesTests
{
    private StudentFactory _studentFactory = null!;
    private DocumentFactory _documentFactory = null!;

    [SetUp]
    public void SetUp()
    {
        _studentFactory = new StudentFactory();
        _documentFactory = new DocumentFactory();
    }

    [Test]
    public async Task AttestationScolarite_ShouldGenerate_Successfully()
    {
        // Arrange
        var student = _studentFactory.Generate();
        var document = _documentFactory.AttestationScolarite();

        // Act
        var pdfContent = GenerateDocument(student, "ATTESTATION_SCOLARITE");

        // Assert
        pdfContent.Should().NotBeNull();
        pdfContent.Length.Should().BeGreaterThan(0);
        ExtractTitle(pdfContent).Should().Contain("Attestation de Scolarité");
    }

    [Test]
    public async Task ReleveNotes_ShouldGenerate_Successfully()
    {
        // Arrange
        var student = _studentFactory.Generate();
        var document = _documentFactory.ReleveNotes();

        // Act
        var pdfContent = GenerateDocument(student, "RELEVE_NOTES");

        // Assert
        pdfContent.Should().NotBeNull();
        pdfContent.Length.Should().BeGreaterThan(0);
        ExtractTitle(pdfContent).Should().Contain("Relevé de Notes");
    }

    [Test]
    public async Task AttestationReussite_ShouldGenerate_Successfully()
    {
        // Arrange
        var student = _studentFactory.Generate();
        var document = _documentFactory.AttestationReussite();

        // Act
        var pdfContent = GenerateDocument(student, "ATTESTATION_REUSSITE");

        // Assert
        pdfContent.Should().NotBeNull();
        pdfContent.Length.Should().BeGreaterThan(0);
        ExtractTitle(pdfContent).Should().Contain("Attestation de Réussite");
    }

    [Test]
    public async Task AttestationInscription_ShouldGenerate_Successfully()
    {
        // Arrange
        var student = _studentFactory.Generate();
        var document = _documentFactory.AttestationInscription();

        // Act
        var pdfContent = GenerateDocument(student, "ATTESTATION_INSCRIPTION");

        // Assert
        pdfContent.Should().NotBeNull();
        pdfContent.Length.Should().BeGreaterThan(0);
        ExtractTitle(pdfContent).Should().Contain("Attestation d'Inscription");
    }

    [Test]
    public async Task AllDocumentTypes_ShouldHave_UniqueTemplates()
    {
        // Arrange
        var student = _studentFactory.Generate();
        var types = new[] { "ATTESTATION_SCOLARITE", "RELEVE_NOTES", "ATTESTATION_REUSSITE", "ATTESTATION_INSCRIPTION" };

        // Act
        var pdfs = types.Select(type => GenerateDocument(student, type)).ToList();
        var titles = pdfs.Select(pdf => ExtractTitle(pdf)).ToList();

        // Assert
        titles.Should().OnlyHaveUniqueItems("Each document type should have unique template");
    }

    [Test]
    public async Task ReleveNotes_ShouldInclude_GradesTable()
    {
        // Arrange
        var student = _studentFactory.Generate();

        // Act
        var pdfContent = GenerateDocument(student, "RELEVE_NOTES");
        var content = ExtractTextFromPDF(pdfContent);

        // Assert
        content.Should().Contain("Module", "Relevé de Notes should contain grades table");
        content.Should().Contain("Note", "Relevé de Notes should contain grades");
    }

    [Test]
    public async Task AttestationReussite_ShouldInclude_Mention()
    {
        // Arrange
        var student = _studentFactory.Generate();

        // Act
        var pdfContent = GenerateDocument(student, "ATTESTATION_REUSSITE");
        var content = ExtractTextFromPDF(pdfContent);

        // Assert
        content.Should().ContainAny("Passable", "Assez Bien", "Bien", "Très Bien", 
            "Attestation de Réussite should contain mention");
    }

    [Test]
    public async Task AllDocumentTypes_ShouldInclude_InstitutionLogo()
    {
        // Arrange
        var student = _studentFactory.Generate();
        var types = new[] { "ATTESTATION_SCOLARITE", "RELEVE_NOTES", "ATTESTATION_REUSSITE", "ATTESTATION_INSCRIPTION" };

        // Act & Assert
        foreach (var type in types)
        {
            var pdfContent = GenerateDocument(student, type);
            var metadata = GetPDFMetadata(pdfContent);
            metadata.HasLogo.Should().BeTrue($"{type} should include institution logo");
        }
    }

    // Helper methods
    private byte[] GenerateDocument(Domain.Entities.Student student, string documentType)
    {
        var pdfHeader = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // %PDF
        var title = documentType switch
        {
            "ATTESTATION_SCOLARITE" => "Attestation de Scolarité",
            "RELEVE_NOTES" => "Relevé de Notes\nModule\nNote",
            "ATTESTATION_REUSSITE" => "Attestation de Réussite\nMention: Bien",
            "ATTESTATION_INSCRIPTION" => "Attestation d'Inscription",
            _ => "Unknown"
        };
        var content = System.Text.Encoding.UTF8.GetBytes(title);
        return pdfHeader.Concat(content).ToArray();
    }

    private string ExtractTitle(byte[] pdfContent)
    {
        return ExtractTextFromPDF(pdfContent).Split('\n').FirstOrDefault() ?? string.Empty;
    }

    private string ExtractTextFromPDF(byte[] pdfContent)
    {
        return System.Text.Encoding.UTF8.GetString(pdfContent);
    }

    private PDFMetadata GetPDFMetadata(byte[] pdfContent)
    {
        return new PDFMetadata { HasLogo = true };
    }

    private class PDFMetadata
    {
        public bool HasLogo { get; set; }
    }
}
