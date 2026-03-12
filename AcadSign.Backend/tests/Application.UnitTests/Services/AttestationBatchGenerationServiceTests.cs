using AcadSign.Backend.Application.Common.Interfaces;
using AcadSign.Backend.Application.Common.Models;
using AcadSign.Backend.Application.Interfaces;
using AcadSign.Backend.Application.Models;
using AcadSign.Backend.Application.Services;
using AcadSign.Backend.Domain.Entities;
using AcadSign.Backend.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace AcadSign.Backend.Application.UnitTests.Services;

[TestFixture]
[Category("P0")]
[Category("Batch")]
public class AttestationBatchGenerationServiceTests
{
    private Mock<ISisAttestationExportClient> _sisClient = null!;
    private Mock<IPdfGenerationService> _pdfService = null!;
    private Mock<IS3StorageService> _s3Storage = null!;
    private Mock<IDocumentRepository> _documentRepository = null!;
    private Mock<IStudentRepository> _studentRepository = null!;
    private Mock<ILogger<AttestationBatchGenerationService>> _logger = null!;

    [SetUp]
    public void SetUp()
    {
        _sisClient = new Mock<ISisAttestationExportClient>();
        _pdfService = new Mock<IPdfGenerationService>();
        _s3Storage = new Mock<IS3StorageService>();
        _documentRepository = new Mock<IDocumentRepository>();
        _studentRepository = new Mock<IStudentRepository>();
        _logger = new Mock<ILogger<AttestationBatchGenerationService>>();
    }

    [Test]
    public async Task GenerateFromSisAsync_WhenSisItemIsValid_GeneratesAndPersistsUnsignedDocument()
    {
        var sisResult = new SisAttestationExportResult
        {
            Items = new List<SisAttestationStudentDto>
            {
                new()
                {
                    Nom = "DOE",
                    Prenom = "JOHN",
                    Apogee = "A1",
                    Filiere = "INFO"
                }
            }
        };

        _sisClient
            .Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(sisResult);

        _studentRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Student?)null);

        _studentRepository
            .Setup(x => x.CreateAsync(It.IsAny<Student>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Student s, CancellationToken _) => s);

        _pdfService
            .Setup(x => x.GenerateDocumentAsync(DocumentType.AttestationScolarite, It.IsAny<StudentData>()))
            .ReturnsAsync(new byte[] { 1, 2, 3, 4 });

        _s3Storage
            .Setup(x => x.UploadDocumentAsync(It.IsAny<byte[]>(), It.IsAny<string>()))
            .ReturnsAsync("documents/unsigned/doc.pdf");

        _documentRepository
            .Setup(x => x.CreateAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document d, CancellationToken _) => d);

        var sut = CreateSut();

        var result = await sut.GenerateFromSisAsync(DocumentType.AttestationScolarite);

        result.Total.Should().Be(1);
        result.Generated.Should().Be(1);
        result.Failed.Should().Be(0);
        result.Failures.Should().BeEmpty();
        result.CreatedDocumentIds.Should().HaveCount(1);

        _pdfService.Verify(x => x.GenerateDocumentAsync(
            DocumentType.AttestationScolarite,
            It.Is<StudentData>(d =>
                d.FirstNameFr == "JOHN" &&
                d.LastNameFr == "DOE" &&
                d.CNE == "A1" &&
                d.ProgramNameFr == "INFO")), Times.Once);

        _documentRepository.Verify(x => x.CreateAsync(
            It.Is<Document>(d =>
                d.DocumentType == DocumentType.AttestationScolarite.ToString() &&
                d.Status == "UNSIGNED" &&
                d.PublicId != Guid.Empty),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GenerateFromSisAsync_WhenGenerationFails_ContinuesAndAggregatesFailures()
    {
        var sisResult = new SisAttestationExportResult
        {
            Items = new List<SisAttestationStudentDto>
            {
                new()
                {
                    Nom = "OK",
                    Prenom = "STUDENT",
                    Apogee = "A2",
                    Filiere = "MATH"
                }
            },
            Errors = new List<SisAttestationExportItemError>
            {
                new()
                {
                    ItemIndex = 0,
                    Code = "MISSING_REQUIRED_FIELDS",
                    Message = "filiere manquante",
                    Nom = "DOE",
                    Prenom = "JOHN",
                    Apogee = "A1"
                }
            }
        };

        _sisClient
            .Setup(x => x.GetStudentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(sisResult);

        _studentRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Student?)null);

        _studentRepository
            .Setup(x => x.CreateAsync(It.IsAny<Student>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Student s, CancellationToken _) => s);

        _pdfService
            .Setup(x => x.GenerateDocumentAsync(DocumentType.AttestationInscription, It.IsAny<StudentData>()))
            .ThrowsAsync(new InvalidOperationException("pdf generation failed"));

        var sut = CreateSut();

        var result = await sut.GenerateFromSisAsync(DocumentType.AttestationInscription);

        result.Total.Should().Be(2);
        result.Generated.Should().Be(0);
        result.Failed.Should().Be(2);
        result.Failures.Should().HaveCount(2);
        result.Failures.Should().ContainSingle(f => f.Code == "MISSING_REQUIRED_FIELDS" && f.Apogee == "A1");
        result.Failures.Should().ContainSingle(f => f.Code == "GENERATION_FAILED" && f.Apogee == "A2");
        result.CreatedDocumentIds.Should().BeEmpty();

        _documentRepository.Verify(x => x.CreateAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private AttestationBatchGenerationService CreateSut()
    {
        return new AttestationBatchGenerationService(
            _sisClient.Object,
            _pdfService.Object,
            _s3Storage.Object,
            _documentRepository.Object,
            _studentRepository.Object,
            _logger.Object);
    }
}
