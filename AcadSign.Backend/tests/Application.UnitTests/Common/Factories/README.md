# Test Factories - AcadSign

## Overview

This directory contains test data factories for generating realistic fake data in unit and integration tests. Factories use the [Bogus](https://github.com/bchavez/Bogus) library for data generation.

## Available Factories

### StudentFactory

Generates test `Student` entities with realistic fake data.

**Basic Usage:**

```csharp
var factory = new StudentFactory();

// Generate a single student
var student = factory.Generate();

// Generate multiple students
var students = factory.Generate(10);

// Generate with specific CIN
var student = factory.WithCin("AB123456");

// Generate with specific CNE
var student = factory.WithCne("E12345678");

// Generate for specific institution
var institutionId = Guid.NewGuid();
var student = factory.ForInstitution(institutionId);

// Generate with custom overrides
var student = factory.WithOverrides(s =>
{
    s.FirstName = "Ahmed";
    s.LastName = "Benali";
});
```

**Generated Fields:**
- `Id`: Random integer (1-100000)
- `CIN`: 8-character alphanumeric (e.g., "AB123456")
- `CNE`: Student code starting with "E" (e.g., "E12345678")
- `FirstName`: Realistic first name
- `LastName`: Realistic last name
- `Email`: Generated from name @uh2.ac.ma
- `PhoneNumber`: Moroccan phone format (+212 6## ## ## ##)
- `DateOfBirth`: Between 18-43 years old
- `InstitutionId`: Random GUID

---

### DocumentFactory

Generates test `Document` entities with realistic fake data.

**Basic Usage:**

```csharp
var factory = new DocumentFactory();

// Generate a single document
var document = factory.Generate();

// Generate unsigned document (default)
var document = factory.Unsigned();

// Generate signed document
var document = factory.Signed();

// Generate document with error status
var document = factory.WithError();

// Generate for specific student
var studentId = Guid.NewGuid();
var document = factory.ForStudent(studentId);

// Generate specific document types
var attestation = factory.AttestationScolarite();
var releve = factory.ReleveNotes();
var reussite = factory.AttestationReussite();
var inscription = factory.AttestationInscription();

// Generate batch for same student
var documents = factory.BatchForStudent(studentId, 5);

// Generate unsigned batch
var documents = factory.UnsignedBatch(10);
```

**Document Types:**
- `ATTESTATION_SCOLARITE` - Enrollment certificate
- `RELEVE_NOTES` - Transcript
- `ATTESTATION_REUSSITE` - Success certificate
- `ATTESTATION_INSCRIPTION` - Registration certificate

**Document Statuses:**
- `UNSIGNED` - Not yet signed (default)
- `SIGNED` - Successfully signed
- `ERROR` - Signature failed
- `PENDING` - Signature in progress

---

### CertificateFactory

Generates test certificate data for Barid Al-Maghrib e-Sign certificates.

**Basic Usage:**

```csharp
var factory = new CertificateFactory();

// Generate valid certificate (expires in 1 year)
var cert = factory.Generate();

// Generate certificate expiring soon
var cert = factory.ExpiringSoon(15); // 15 days until expiry

// Generate expired certificate
var cert = factory.Expired();

// Generate certificate not yet valid
var cert = factory.NotYetValid();

// Generate with specific expiry date
var expiryDate = DateTime.UtcNow.AddMonths(6);
var cert = factory.WithExpiry(expiryDate);

// Generate for specific institution
var cert = factory.ForInstitution("Université Mohammed V");

// Generate revoked certificate
var cert = factory.Revoked();
```

**Certificate Properties:**
- `SerialNumber`: 16-character alphanumeric
- `Subject`: Certificate subject (CN, O, C)
- `Issuer`: Barid Al-Maghrib Root CA
- `NotBefore`: Certificate valid from date
- `NotAfter`: Certificate expiry date
- `Thumbprint`: 40-character hash
- `PublicKey`: Base64-encoded public key
- `IsValid`: Validity status
- `RevocationReason`: Reason if revoked
- `RevokedAt`: Revocation timestamp

**Helper Methods:**
- `IsExpired`: Check if certificate is expired
- `DaysUntilExpiry`: Days remaining until expiry
- `ExpiresWithin(days)`: Check if expires within N days

---

## Best Practices

### 1. Use Factories in Tests

```csharp
[Test]
public void SignDocument_ShouldSucceed_WhenCertificateValid()
{
    // Arrange
    var studentFactory = new StudentFactory();
    var documentFactory = new DocumentFactory();
    var certificateFactory = new CertificateFactory();
    
    var student = studentFactory.Generate();
    var document = documentFactory.ForStudent(student.Id).Unsigned();
    var certificate = certificateFactory.Generate();
    
    // Act
    var result = _signatureService.Sign(document, certificate);
    
    // Assert
    result.Should().BeSuccessful();
    document.Status.Should().Be("SIGNED");
}
```

### 2. Test Edge Cases

```csharp
[Test]
public void SignDocument_ShouldFail_WhenCertificateExpired()
{
    // Arrange
    var documentFactory = new DocumentFactory();
    var certificateFactory = new CertificateFactory();
    
    var document = documentFactory.Unsigned();
    var expiredCert = certificateFactory.Expired();
    
    // Act
    var result = _signatureService.Sign(document, expiredCert);
    
    // Assert
    result.Should().BeFailed();
    result.Error.Should().Contain("Certificate expired");
}
```

### 3. Batch Testing

```csharp
[Test]
public void BatchSign_ShouldSignAllDocuments()
{
    // Arrange
    var documentFactory = new DocumentFactory();
    var certificateFactory = new CertificateFactory();
    
    var documents = documentFactory.UnsignedBatch(500);
    var certificate = certificateFactory.Generate();
    
    // Act
    var result = _batchService.SignBatch(documents, certificate);
    
    // Assert
    result.SuccessCount.Should().Be(500);
    documents.Should().AllSatisfy(d => d.Status.Should().Be("SIGNED"));
}
```

### 4. Isolation and Cleanup

Factories generate unique data each time, ensuring test isolation:

```csharp
[Test]
public void TwoTests_ShouldNotInterfere()
{
    var factory = new StudentFactory();
    
    var student1 = factory.Generate();
    var student2 = factory.Generate();
    
    // Each student has unique CIN, CNE, Email
    student1.CIN.Should().NotBe(student2.CIN);
}
```

---

## Integration with Test Plan

These factories support the test scenarios defined in:
- `test-design-architecture.md` - Testability concerns
- `test-design-qa.md` - P0-P3 test coverage

**Relevant Test IDs:**
- **P0-006 to P0-014**: Authentication & Security tests
- **P0-015 to P0-022**: PDF Generation & Storage tests
- **P0-023 to P0-037**: Electronic Signature tests
- **P0-052 to P0-056**: CNDP Compliance tests

---

## Adding New Factories

To add a new factory:

1. Create a new file in this directory (e.g., `TemplateFactory.cs`)
2. Use Bogus `Faker<T>` for data generation
3. Provide fluent methods for common scenarios
4. Add tests in `Factories/` directory
5. Update this README

**Example:**

```csharp
public class TemplateFactory
{
    private readonly Faker<DocumentTemplate> _faker;
    
    public TemplateFactory()
    {
        _faker = new Faker<DocumentTemplate>()
            .RuleFor(t => t.Id, f => Guid.NewGuid())
            .RuleFor(t => t.Name, f => f.Commerce.ProductName())
            .RuleFor(t => t.Version, f => f.System.Version().ToString());
    }
    
    public DocumentTemplate Generate() => _faker.Generate();
}
```

---

## Dependencies

- **Bogus** (v35.6.1): Fake data generation
- **FluentAssertions** (v6.12.1): Fluent test assertions
- **NUnit**: Test framework

---

## References

- [Bogus Documentation](https://github.com/bchavez/Bogus)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [Test Design QA Document](../../../../_bmad-output/test-artifacts/test-design-qa.md)
