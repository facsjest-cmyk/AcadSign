using AcadSign.Backend.Application.Common.Models;
using AcadSign.Backend.Domain.Entities;
using FluentAssertions;
using NUnit.Framework;

namespace AcadSign.Backend.Application.UnitTests.API;

[TestFixture]
[Category("P0")]
[Category("Documents")]
public class PendingDocumentDtoMapperTests
{
    [Test]
    public void Map_WhenStudentExists_MapsRequiredAndOptionalFields()
    {
        var studentPublicId = Guid.NewGuid();
        var createdAt = new DateTimeOffset(2026, 3, 10, 18, 30, 0, TimeSpan.Zero);

        var document = new Document
        {
            PublicId = Guid.NewGuid(),
            StudentId = studentPublicId,
            DocumentType = "ATTESTATION_SCOLARITE",
            Status = "UNSIGNED",
            Created = createdAt
        };

        var student = new Student
        {
            PublicId = studentPublicId,
            FirstName = "Aya",
            LastName = "El Idrissi",
            CNE = "CNE-001",
            CIN = "CIN-001"
        };

        var dto = PendingDocumentDtoMapper.Map(document, student);

        dto.Id.Should().Be(document.PublicId);
        dto.StudentName.Should().Be("Aya El Idrissi");
        dto.DocumentType.Should().Be("ATTESTATION_SCOLARITE");
        dto.CreatedAt.Should().Be(createdAt.UtcDateTime);
        dto.Status.Should().Be("UNSIGNED");
        dto.StudentId.Should().Be("CNE-001");
        dto.Cin.Should().Be("CIN-001");
        dto.Program.Should().BeNull();
        dto.Level.Should().BeNull();
        dto.Reference.Should().BeNull();
    }

    [Test]
    public void Map_WhenStudentIsMissing_UsesFallbackValues()
    {
        var studentPublicId = Guid.NewGuid();

        var document = new Document
        {
            PublicId = Guid.NewGuid(),
            StudentId = studentPublicId,
            DocumentType = "ATTESTATION_REUSSITE",
            Status = "PENDING",
            Created = DateTimeOffset.UtcNow
        };

        var dto = PendingDocumentDtoMapper.Map(document, null);

        dto.StudentName.Should().BeEmpty();
        dto.StudentId.Should().Be(studentPublicId.ToString());
        dto.Cin.Should().BeNull();
        dto.Status.Should().Be("PENDING");
    }
}
