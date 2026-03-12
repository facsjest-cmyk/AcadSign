using System.Net;
using System.Net.Http.Json;
using AcadSign.Backend.Application.Common.Models;
using AcadSign.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AcadSign.Backend.Application.FunctionalTests.Documents.Queries;

using static Testing;

public class GetPendingDocumentsTests : BaseTestFixture
{
    [Test]
    public async Task ShouldReturnUnsignedAndPendingDocuments_FromDatabase()
    {
        var studentPublicId = Guid.NewGuid();
        var unsignedDocumentId = Guid.NewGuid();
        var pendingDocumentId = Guid.NewGuid();
        var signedDocumentId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        await ExecuteDbContextAsync(async db =>
        {
            db.Students.Add(new Student
            {
                PublicId = studentPublicId,
                FirstName = "Youssef",
                LastName = "Haddad",
                CIN = "CIN-123",
                CNE = "CNE-123",
                Email = "youssef.haddad@uh2.ac.ma",
                DateOfBirth = new DateTime(2001, 5, 14),
                InstitutionId = Guid.NewGuid()
            });

            db.Documents.AddRange(
                new Document
                {
                    PublicId = unsignedDocumentId,
                    StudentId = studentPublicId,
                    DocumentType = "ATTESTATION_SCOLARITE",
                    Status = "UNSIGNED",
                    S3ObjectPath = "documents/unsigned.pdf",
                    Created = now.AddMinutes(-2),
                    LastModified = now.AddMinutes(-2)
                },
                new Document
                {
                    PublicId = pendingDocumentId,
                    StudentId = studentPublicId,
                    DocumentType = "ATTESTATION_REUSSITE",
                    Status = "PENDING",
                    S3ObjectPath = "documents/pending.pdf",
                    Created = now.AddMinutes(-1),
                    LastModified = now.AddMinutes(-1)
                },
                new Document
                {
                    PublicId = signedDocumentId,
                    StudentId = studentPublicId,
                    DocumentType = "ATTESTATION_INSCRIPTION",
                    Status = "SIGNED",
                    S3ObjectPath = "documents/signed.pdf",
                    Created = now,
                    LastModified = now
                });

            await db.SaveChangesAsync();
        });

        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/documents/pending");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var documents = await response.Content.ReadFromJsonAsync<List<PendingDocumentDto>>();

        documents.ShouldNotBeNull();
        documents!.Select(d => d.Id).ShouldContain(unsignedDocumentId);
        documents.Select(d => d.Id).ShouldContain(pendingDocumentId);
        documents.Select(d => d.Id).ShouldNotContain(signedDocumentId);

        var unsigned = documents.Single(d => d.Id == unsignedDocumentId);
        unsigned.StudentName.ShouldBe("Youssef Haddad");
        unsigned.DocumentType.ShouldBe("ATTESTATION_SCOLARITE");
        unsigned.Status.ShouldBe("UNSIGNED");
        unsigned.StudentId.ShouldBe("CNE-123");
        unsigned.Cin.ShouldBe("CIN-123");
    }

    [Test]
    public async Task ShouldExcludeDocument_WhenStatusBecomesSigned()
    {
        var studentPublicId = Guid.NewGuid();
        var documentPublicId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        await ExecuteDbContextAsync(async db =>
        {
            db.Students.Add(new Student
            {
                PublicId = studentPublicId,
                FirstName = "Sara",
                LastName = "Bennani",
                CIN = "CIN-456",
                CNE = "CNE-456",
                Email = "sara.bennani@uh2.ac.ma",
                DateOfBirth = new DateTime(2000, 8, 12),
                InstitutionId = Guid.NewGuid()
            });

            db.Documents.Add(new Document
            {
                PublicId = documentPublicId,
                StudentId = studentPublicId,
                DocumentType = "ATTESTATION_SCOLARITE",
                Status = "UNSIGNED",
                S3ObjectPath = "documents/sara.pdf",
                Created = now,
                LastModified = now
            });

            await db.SaveChangesAsync();
        });

        var client = CreateClient();

        var beforeSign = await client.GetFromJsonAsync<List<PendingDocumentDto>>("/api/v1/documents/pending");
        beforeSign.ShouldNotBeNull();
        beforeSign!.Select(d => d.Id).ShouldContain(documentPublicId);

        await ExecuteDbContextAsync(async db =>
        {
            var document = await db.Documents.SingleAsync(d => d.PublicId == documentPublicId);
            document.Status = "SIGNED";
            document.SignedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        });

        var afterSign = await client.GetFromJsonAsync<List<PendingDocumentDto>>("/api/v1/documents/pending");
        afterSign.ShouldNotBeNull();
        afterSign!.Select(d => d.Id).ShouldNotContain(documentPublicId);
    }
}
