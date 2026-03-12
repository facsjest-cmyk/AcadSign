using AcadSign.Backend.Domain.Entities;
using Bogus;

namespace AcadSign.Backend.Application.UnitTests.Common.Factories;

/// <summary>
/// Factory for generating test Document entities with realistic fake data.
/// Uses Bogus library for data generation.
/// </summary>
public class DocumentFactory
{
    private readonly Faker<Document> _faker;
    private static readonly string[] DocumentTypes = new[]
    {
        "ATTESTATION_SCOLARITE",
        "RELEVE_NOTES",
        "ATTESTATION_REUSSITE",
        "ATTESTATION_INSCRIPTION"
    };

    public DocumentFactory()
    {
        _faker = new Faker<Document>()
            .RuleFor(d => d.Id, f => f.Random.Int(1, 100000))
            .RuleFor(d => d.PublicId, f => Guid.NewGuid())
            .RuleFor(d => d.DocumentType, f => f.PickRandom(DocumentTypes))
            .RuleFor(d => d.StudentId, f => Guid.NewGuid())
            .RuleFor(d => d.Status, f => "UNSIGNED")
            .RuleFor(d => d.S3ObjectPath, f => $"documents/{f.Random.Guid()}/{f.System.FileName("pdf")}")
            .RuleFor(d => d.SignedAt, f => null)
            .RuleFor(d => d.SignerName, f => null)
            .RuleFor(d => d.SignatureData, f => null)
            .RuleFor(d => d.Created, f => f.Date.RecentOffset(30))
            .RuleFor(d => d.CreatedBy, f => f.Internet.Email())
            .RuleFor(d => d.LastModified, f => f.Date.RecentOffset(1))
            .RuleFor(d => d.LastModifiedBy, f => null);
    }

    /// <summary>
    /// Generate a single Document with random data
    /// </summary>
    public Document Generate() => _faker.Generate();

    /// <summary>
    /// Generate multiple Documents with random data
    /// </summary>
    public List<Document> Generate(int count) => _faker.Generate(count);

    /// <summary>
    /// Generate a Document with specific overrides
    /// </summary>
    public Document WithOverrides(Action<Document> configure)
    {
        var document = Generate();
        configure(document);
        return document;
    }

    /// <summary>
    /// Generate an unsigned Document (default)
    /// </summary>
    public Document Unsigned()
    {
        var document = Generate();
        document.Status = "UNSIGNED";
        document.SignedAt = null;
        document.SignerName = null;
        document.SignatureData = null;
        return document;
    }

    /// <summary>
    /// Generate a signed Document
    /// </summary>
    public Document Signed()
    {
        var faker = new Faker();
        var document = Generate();
        document.Status = "SIGNED";
        document.SignedAt = faker.Date.Recent(7);
        document.SignerName = faker.Name.FullName();
        document.SignatureData = Convert.ToBase64String(faker.Random.Bytes(256));
        return document;
    }

    /// <summary>
    /// Generate a Document with error status
    /// </summary>
    public Document WithError()
    {
        var document = Generate();
        document.Status = "ERROR";
        return document;
    }

    /// <summary>
    /// Generate a Document for a specific student
    /// </summary>
    public Document ForStudent(Guid studentId)
    {
        var document = Generate();
        document.StudentId = studentId;
        return document;
    }

    /// <summary>
    /// Generate a Document of a specific type
    /// </summary>
    public Document OfType(string documentType)
    {
        var document = Generate();
        document.DocumentType = documentType;
        return document;
    }

    /// <summary>
    /// Generate an Attestation de Scolarité
    /// </summary>
    public Document AttestationScolarite() => OfType("ATTESTATION_SCOLARITE");

    /// <summary>
    /// Generate a Relevé de Notes
    /// </summary>
    public Document ReleveNotes() => OfType("RELEVE_NOTES");

    /// <summary>
    /// Generate an Attestation de Réussite
    /// </summary>
    public Document AttestationReussite() => OfType("ATTESTATION_REUSSITE");

    /// <summary>
    /// Generate an Attestation d'Inscription
    /// </summary>
    public Document AttestationInscription() => OfType("ATTESTATION_INSCRIPTION");

    /// <summary>
    /// Generate a batch of documents for the same student
    /// </summary>
    public List<Document> BatchForStudent(Guid studentId, int count)
    {
        var documents = Generate(count);
        foreach (var doc in documents)
        {
            doc.StudentId = studentId;
        }
        return documents;
    }

    /// <summary>
    /// Generate a batch of unsigned documents
    /// </summary>
    public List<Document> UnsignedBatch(int count)
    {
        var documents = Generate(count);
        foreach (var doc in documents)
        {
            doc.Status = "UNSIGNED";
            doc.SignedAt = null;
            doc.SignerName = null;
            doc.SignatureData = null;
        }
        return documents;
    }
}
