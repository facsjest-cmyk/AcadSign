using AcadSign.Backend.Application.UnitTests.Common.Factories;
using FluentAssertions;
using NUnit.Framework;

namespace AcadSign.Backend.Application.UnitTests.Factories;

/// <summary>
/// Tests for DocumentFactory to ensure test data generation works correctly
/// </summary>
[TestFixture]
public class DocumentFactoryTests
{
    private DocumentFactory _factory = null!;

    [SetUp]
    public void Setup()
    {
        _factory = new DocumentFactory();
    }

    [Test]
    public void Generate_ShouldCreateValidDocument()
    {
        // Act
        var document = _factory.Generate();

        // Assert
        document.Should().NotBeNull();
        document.Id.Should().BeGreaterThan(0);
        document.DocumentType.Should().NotBeNullOrEmpty();
        document.StudentId.Should().NotBeEmpty();
        document.Status.Should().Be("UNSIGNED");
        document.S3ObjectPath.Should().NotBeNullOrEmpty();
        document.SignedAt.Should().BeNull();
        document.SignerName.Should().BeNull();
        document.SignatureData.Should().BeNull();
    }

    [Test]
    public void Generate_WithCount_ShouldCreateMultipleDocuments()
    {
        // Act
        var documents = _factory.Generate(10);

        // Assert
        documents.Should().HaveCount(10);
        documents.Should().OnlyHaveUniqueItems(d => d.Id);
        documents.Should().OnlyHaveUniqueItems(d => d.S3ObjectPath);
    }

    [Test]
    public void Unsigned_ShouldCreateUnsignedDocument()
    {
        // Act
        var document = _factory.Unsigned();

        // Assert
        document.Status.Should().Be("UNSIGNED");
        document.SignedAt.Should().BeNull();
        document.SignerName.Should().BeNull();
        document.SignatureData.Should().BeNull();
    }

    [Test]
    public void Signed_ShouldCreateSignedDocument()
    {
        // Act
        var document = _factory.Signed();

        // Assert
        document.Status.Should().Be("SIGNED");
        document.SignedAt.Should().NotBeNull();
        document.SignedAt.Should().BeOnOrBefore(DateTime.UtcNow);
        document.SignerName.Should().NotBeNullOrEmpty();
        document.SignatureData.Should().NotBeNullOrEmpty();
    }

    [Test]
    public void WithError_ShouldCreateDocumentWithErrorStatus()
    {
        // Act
        var document = _factory.WithError();

        // Assert
        document.Status.Should().Be("ERROR");
    }

    [Test]
    public void ForStudent_ShouldSetSpecificStudentId()
    {
        // Arrange
        var studentId = Guid.NewGuid();

        // Act
        var document = _factory.ForStudent(studentId);

        // Assert
        document.StudentId.Should().Be(studentId);
    }

    [Test]
    public void OfType_ShouldSetSpecificDocumentType()
    {
        // Arrange
        var documentType = "ATTESTATION_SCOLARITE";

        // Act
        var document = _factory.OfType(documentType);

        // Assert
        document.DocumentType.Should().Be(documentType);
    }

    [Test]
    public void AttestationScolarite_ShouldCreateCorrectType()
    {
        // Act
        var document = _factory.AttestationScolarite();

        // Assert
        document.DocumentType.Should().Be("ATTESTATION_SCOLARITE");
    }

    [Test]
    public void ReleveNotes_ShouldCreateCorrectType()
    {
        // Act
        var document = _factory.ReleveNotes();

        // Assert
        document.DocumentType.Should().Be("RELEVE_NOTES");
    }

    [Test]
    public void AttestationReussite_ShouldCreateCorrectType()
    {
        // Act
        var document = _factory.AttestationReussite();

        // Assert
        document.DocumentType.Should().Be("ATTESTATION_REUSSITE");
    }

    [Test]
    public void AttestationInscription_ShouldCreateCorrectType()
    {
        // Act
        var document = _factory.AttestationInscription();

        // Assert
        document.DocumentType.Should().Be("ATTESTATION_INSCRIPTION");
    }

    [Test]
    public void BatchForStudent_ShouldCreateMultipleDocumentsForSameStudent()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var count = 5;

        // Act
        var documents = _factory.BatchForStudent(studentId, count);

        // Assert
        documents.Should().HaveCount(count);
        documents.Should().AllSatisfy(d => d.StudentId.Should().Be(studentId));
    }

    [Test]
    public void UnsignedBatch_ShouldCreateMultipleUnsignedDocuments()
    {
        // Arrange
        var count = 10;

        // Act
        var documents = _factory.UnsignedBatch(count);

        // Assert
        documents.Should().HaveCount(count);
        documents.Should().AllSatisfy(d =>
        {
            d.Status.Should().Be("UNSIGNED");
            d.SignedAt.Should().BeNull();
            d.SignerName.Should().BeNull();
            d.SignatureData.Should().BeNull();
        });
    }

    [Test]
    public void WithOverrides_ShouldApplyCustomConfiguration()
    {
        // Arrange
        var customPath = "custom/path/document.pdf";

        // Act
        var document = _factory.WithOverrides(d =>
        {
            d.S3ObjectPath = customPath;
        });

        // Assert
        document.S3ObjectPath.Should().Be(customPath);
    }
}
