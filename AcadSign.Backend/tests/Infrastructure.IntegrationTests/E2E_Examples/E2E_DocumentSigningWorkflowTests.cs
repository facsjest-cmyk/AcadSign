using AcadSign.Backend.Application.UnitTests.Common.Factories;
using AcadSign.Backend.Infrastructure.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace AcadSign.Backend.Infrastructure.IntegrationTests.E2E_Examples;

/// <summary>
/// End-to-End test example demonstrating complete document signing workflow.
/// This test shows how to use all factories together in a realistic scenario.
/// 
/// Workflow:
/// 1. Create student
/// 2. Generate unsigned document for student
/// 3. Validate certificate before signing
/// 4. Sign document
/// 5. Verify signature and document status
/// </summary>
[TestFixture]
[Category("E2E")]
[Category("Example")]
public class E2E_DocumentSigningWorkflowTests : IntegrationTestBase
{
    private StudentFactory _studentFactory = null!;
    private DocumentFactory _documentFactory = null!;
    private CertificateFactory _certificateFactory = null!;

    [SetUp]
    public new async Task SetUp()
    {
        await base.SetUp();
        _studentFactory = new StudentFactory();
        _documentFactory = new DocumentFactory();
        _certificateFactory = new CertificateFactory();
    }

    [Test]
    public async Task CompleteWorkflow_ShouldSignDocument_Successfully()
    {
        // ============================================
        // STEP 1: Create Student
        // ============================================
        var student = _studentFactory.WithOverrides(s =>
        {
            s.FirstName = "Ahmed";
            s.LastName = "Benali";
            s.Email = "ahmed.benali@uh2.ac.ma";
        });

        Database.DbContext.Set<Domain.Entities.Student>().Add(student);
        await Database.DbContext.SaveChangesAsync();

        student.Id.Should().BeGreaterThan(0, "Student should be saved with ID");

        // ============================================
        // STEP 2: Generate Unsigned Document
        // ============================================
        var document = _documentFactory.AttestationScolarite();
        document.StudentId = student.PublicId;

        document.S3ObjectPath = $"documents/{student.Id}/attestation-{DateTime.UtcNow:yyyyMMdd}.pdf";

        Database.DbContext.Set<Domain.Entities.Document>().Add(document);
        await Database.DbContext.SaveChangesAsync();

        document.Status.Should().Be("UNSIGNED", "Document should start as unsigned");
        document.SignedAt.Should().BeNull("Document should not have signature timestamp yet");

        // ============================================
        // STEP 3: Validate Certificate
        // ============================================
        var certificate = _certificateFactory.Generate();

        certificate.IsValid.Should().BeTrue("Certificate must be valid before signing");
        certificate.IsExpired.Should().BeFalse("Certificate must not be expired");
        certificate.ExpiresWithin(7).Should().BeFalse("Certificate should not expire within 7 days");

        // ============================================
        // STEP 4: Sign Document (Simulated)
        // ============================================
        // In real implementation, this would call SignatureService
        document.Status = "SIGNED";
        document.SignedAt = DateTime.UtcNow;
        document.SignerName = "Ahmed Benali";
        document.SignatureData = Convert.ToBase64String(new byte[] { 1, 2, 3, 4, 5 }); // Mock signature

        await Database.DbContext.SaveChangesAsync();

        // ============================================
        // STEP 5: Verify Signature and Status
        // ============================================
        var signedDocument = await Database.DbContext.Set<Domain.Entities.Document>()
            .FirstOrDefaultAsync(d => d.Id == document.Id);

        signedDocument.Should().NotBeNull("Signed document should exist in database");
        signedDocument!.Status.Should().Be("SIGNED", "Document status should be SIGNED");
        signedDocument.SignedAt.Should().NotBeNull("Document should have signature timestamp");
        signedDocument.SignedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        signedDocument.SignerName.Should().Be("Ahmed Benali");
        signedDocument.SignatureData.Should().NotBeNullOrEmpty("Document should have signature data");

        // ============================================
        // STEP 6: Verify Student Can Access Document
        // ============================================
        var studentDocuments = await Database.DbContext.Set<Domain.Entities.Document>()
            .Where(d => d.StudentId == document.StudentId)
            .ToListAsync();

        studentDocuments.Should().HaveCount(1, "Student should have 1 document");
        studentDocuments.First().DocumentType.Should().Be("ATTESTATION_SCOLARITE");
    }

    [Test]
    public async Task BatchSigningWorkflow_ShouldSignMultipleDocuments_Successfully()
    {
        // ============================================
        // STEP 1: Create Student
        // ============================================
        var student = _studentFactory.Generate();
        Database.DbContext.Set<Domain.Entities.Student>().Add(student);
        await Database.DbContext.SaveChangesAsync();

        // ============================================
        // STEP 2: Generate Batch of Unsigned Documents
        // ============================================
        var documents = _documentFactory.UnsignedBatch(10);
        foreach (var doc in documents)
        {
            doc.StudentId = Guid.NewGuid(); // In real scenario, different students
        }

        Database.DbContext.Set<Domain.Entities.Document>().AddRange(documents);
        await Database.DbContext.SaveChangesAsync();

        documents.Should().AllSatisfy(d => d.Status.Should().Be("UNSIGNED"));

        // ============================================
        // STEP 3: Validate Certificate
        // ============================================
        var certificate = _certificateFactory.Generate();
        certificate.IsValid.Should().BeTrue();

        // ============================================
        // STEP 4: Batch Sign Documents
        // ============================================
        foreach (var doc in documents)
        {
            doc.Status = "SIGNED";
            doc.SignedAt = DateTime.UtcNow;
            doc.SignerName = "Batch Signer";
            doc.SignatureData = Convert.ToBase64String(new byte[] { 1, 2, 3 });
        }

        await Database.DbContext.SaveChangesAsync();

        // ============================================
        // STEP 5: Verify All Documents Signed
        // ============================================
        var signedCount = await Database.DbContext.Set<Domain.Entities.Document>()
            .CountAsync(d => d.Status == "SIGNED");

        signedCount.Should().Be(10, "All 10 documents should be signed");
    }

    [Test]
    public async Task SigningWithExpiredCertificate_ShouldFail()
    {
        // ============================================
        // STEP 1: Create Document
        // ============================================
        var document = _documentFactory.Unsigned();
        Database.DbContext.Set<Domain.Entities.Document>().Add(document);
        await Database.DbContext.SaveChangesAsync();

        // ============================================
        // STEP 2: Validate Certificate (Expired)
        // ============================================
        var expiredCertificate = _certificateFactory.Expired();

        expiredCertificate.IsValid.Should().BeFalse("Expired certificate should not be valid");
        expiredCertificate.IsExpired.Should().BeTrue("Certificate should be expired");

        // ============================================
        // STEP 3: Attempt to Sign (Should Fail)
        // ============================================
        // In real implementation, SignatureService would reject this
        var shouldAllowSigning = expiredCertificate.IsValid && !expiredCertificate.IsExpired;

        shouldAllowSigning.Should().BeFalse("Signing should not be allowed with expired certificate");

        // Document should remain unsigned
        document.Status.Should().Be("UNSIGNED");
        document.SignedAt.Should().BeNull();
    }

    [Test]
    public async Task SigningWithExpiringSoonCertificate_ShouldWarn()
    {
        // ============================================
        // STEP 1: Create Document
        // ============================================
        var document = _documentFactory.Unsigned();
        Database.DbContext.Set<Domain.Entities.Document>().Add(document);
        await Database.DbContext.SaveChangesAsync();

        // ============================================
        // STEP 2: Validate Certificate (Expiring Soon)
        // ============================================
        var expiringSoonCertificate = _certificateFactory.ExpiringSoon(5); // 5 days

        expiringSoonCertificate.IsValid.Should().BeTrue("Certificate should still be valid");
        expiringSoonCertificate.ExpiresWithin(7).Should().BeTrue("Certificate expires within 7 days");

        // ============================================
        // STEP 3: Check Warning Condition
        // ============================================
        var shouldWarn = expiringSoonCertificate.ExpiresWithin(30);

        shouldWarn.Should().BeTrue("System should warn about certificate expiring soon");

        // In real implementation, this would:
        // 1. Log warning
        // 2. Send email to admin
        // 3. Display warning in UI
        // 4. Still allow signing (certificate is valid)
    }

    [Test]
    public async Task AllDocumentTypes_ShouldBeGenerated_Successfully()
    {
        // ============================================
        // Test all 4 document types
        // ============================================
        var student = _studentFactory.Generate();
        Database.DbContext.Set<Domain.Entities.Student>().Add(student);
        await Database.DbContext.SaveChangesAsync();

        var attestationScolarite = _documentFactory.AttestationScolarite();
        var releveNotes = _documentFactory.ReleveNotes();
        var attestationReussite = _documentFactory.AttestationReussite();
        var attestationInscription = _documentFactory.AttestationInscription();

        var allDocuments = new[] { attestationScolarite, releveNotes, attestationReussite, attestationInscription };

        Database.DbContext.Set<Domain.Entities.Document>().AddRange(allDocuments);
        await Database.DbContext.SaveChangesAsync();

        // Verify all types saved
        var savedCount = await Database.DbContext.Set<Domain.Entities.Document>().CountAsync();
        savedCount.Should().Be(4, "All 4 document types should be saved");

        // Verify each type
        var types = await Database.DbContext.Set<Domain.Entities.Document>()
            .Select(d => d.DocumentType)
            .Distinct()
            .ToListAsync();

        types.Should().Contain("ATTESTATION_SCOLARITE");
        types.Should().Contain("RELEVE_NOTES");
        types.Should().Contain("ATTESTATION_REUSSITE");
        types.Should().Contain("ATTESTATION_INSCRIPTION");
    }
}
