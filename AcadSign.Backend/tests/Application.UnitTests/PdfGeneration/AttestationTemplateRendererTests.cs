using System.Text;
using AcadSign.Backend.Application.Common.Interfaces;
using AcadSign.Backend.Application.Common.Models;
using AcadSign.Backend.Domain.Enums;
using AcadSign.Backend.Infrastructure.Pdf;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace AcadSign.Backend.Application.UnitTests.PdfGeneration;

[TestFixture]
[Category("P0")]
[Category("PDF")]
[Category("Template")]
public class AttestationTemplateRendererTests
{
    [Test]
    public async Task TryRenderAsync_WhenTemplateExists_ReturnsNonEmptyPdf()
    {
        var templateRoot = CreateTemporaryTemplateRoot();

        try
        {
            const string imageFileName = "template.png";
            var imagePath = Path.Combine(templateRoot, imageFileName);
            await File.WriteAllBytesAsync(imagePath, CreateSinglePixelPng());

            var mappingPath = Path.Combine(templateRoot, "template-layouts.json");
            await File.WriteAllTextAsync(mappingPath, BuildMappingJson(imageFileName));

            var renderer = CreateRenderer(templateRoot);
            var studentData = CreateStudentData();

            var result = await renderer.TryRenderAsync(DocumentType.AttestationScolarite, studentData);

            result.Should().NotBeNull();
            result!.Length.Should().BeGreaterThan(0);
            Encoding.ASCII.GetString(result, 0, 4).Should().Be("%PDF");
        }
        finally
        {
            Directory.Delete(templateRoot, recursive: true);
        }
    }

    [Test]
    public async Task TryRenderAsync_WhenTemplateMappingIsMissing_ReturnsNull()
    {
        var templateRoot = CreateTemporaryTemplateRoot();

        try
        {
            var renderer = CreateRenderer(templateRoot);
            var studentData = CreateStudentData();

            var result = await renderer.TryRenderAsync(DocumentType.AttestationScolarite, studentData);

            result.Should().BeNull();
        }
        finally
        {
            Directory.Delete(templateRoot, recursive: true);
        }
    }

    [Test]
    public async Task PdfGenerationService_WhenTemplateRenderingFails_FallsBackToStandardQuestPdf()
    {
        var templateRenderer = new Mock<IAttestationTemplateRenderer>();
        templateRenderer
            .Setup(x => x.TryRenderAsync(It.IsAny<DocumentType>(), It.IsAny<StudentData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        var qrCodeService = new Mock<IQrCodeService>();
        qrCodeService
            .Setup(x => x.GenerateQrCode(It.IsAny<string>()))
            .Returns(CreateSinglePixelPng());

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["VerificationPortal:BaseUrl"] = "http://localhost:5000"
            })
            .Build();

        var logger = new Mock<ILogger<PdfGenerationService>>();
        var service = new PdfGenerationService(
            logger.Object,
            qrCodeService.Object,
            templateRenderer.Object,
            configuration);

        var studentData = CreateStudentData();

        var result = await service.GenerateDocumentAsync(DocumentType.AttestationScolarite, studentData);

        result.Should().NotBeNull();
        result.Length.Should().BeGreaterThan(0);

        templateRenderer.Verify(
            x => x.TryRenderAsync(DocumentType.AttestationScolarite, It.IsAny<StudentData>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static AttestationTemplateRenderer CreateRenderer(string templateRoot)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AttestationTemplateRenderer:TemplateRootPath"] = templateRoot,
                ["AttestationTemplateRenderer:MappingFile"] = "template-layouts.json"
            })
            .Build();

        var environment = new Mock<IHostEnvironment>();
        environment.SetupGet(x => x.ContentRootPath).Returns(templateRoot);

        var logger = new Mock<ILogger<AttestationTemplateRenderer>>();

        return new AttestationTemplateRenderer(configuration, environment.Object, logger.Object);
    }

    private static StudentData CreateStudentData()
    {
        return new StudentData
        {
            DocumentId = Guid.NewGuid(),
            FirstNameFr = "Aya",
            LastNameFr = "El Idrissi",
            CNE = "A12345",
            ProgramNameFr = "INFO",
            AcademicYear = "2025-2026"
        };
    }

    private static string CreateTemporaryTemplateRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), "AcadSign", "template-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }

    private static byte[] CreateSinglePixelPng()
    {
        return Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO2U4P8AAAAASUVORK5CYII=");
    }

    private static string BuildMappingJson(string imageFileName)
    {
        return $$"""
{
  "templates": [
    {
      "key": "unit-template",
      "documentType": "AttestationScolarite",
      "priority": 10,
      "programContains": "INFO",
      "backgroundImage": "{{imageFileName}}",
      "fields": [
        { "field": "FullNameFr", "x": 120, "y": 180, "fontSize": 14, "bold": true },
        { "field": "CNE", "x": 120, "y": 210, "fontSize": 11, "prefix": "Apogee: " }
      ]
    }
  ]
}
""";
    }
}
