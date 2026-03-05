# AcadSign Backend - Test Infrastructure

## 📋 Overview

This directory contains the complete test infrastructure for AcadSign Backend, implementing the test strategy defined in:
- **Test Design Architecture**: `/_bmad-output/test-artifacts/test-design-architecture.md`
- **Test Design QA**: `/_bmad-output/test-artifacts/test-design-qa.md`

**Total Test Coverage**: ~180 tests (P0-P3 priorities)
- **P0 (Critical)**: 60 tests - Core functionality, security, compliance
- **P1 (High)**: 70 tests - Important features, integration
- **P2 (Medium)**: 40 tests - Edge cases, regression
- **P3 (Low)**: 10 tests - Exploratory, benchmarks

---

## 🏗️ Test Projects Structure

```
tests/
├── Application.UnitTests/          # Unit tests for Application layer
│   ├── Common/
│   │   └── Factories/              # Test data factories (Bogus)
│   │       ├── StudentFactory.cs
│   │       ├── DocumentFactory.cs
│   │       ├── CertificateFactory.cs
│   │       └── README.md
│   └── Factories/                  # Factory validation tests
│
├── Domain.UnitTests/               # Unit tests for Domain layer
│
├── Infrastructure.IntegrationTests/ # Integration tests with Testcontainers
│   ├── Common/
│   │   ├── TestContainersFixture.cs    # PostgreSQL + MinIO containers
│   │   ├── DatabaseFixture.cs          # Database setup + Respawn cleanup
│   │   └── IntegrationTestBase.cs      # Base class for integration tests
│   ├── P0_Infrastructure/              # P0 infrastructure tests
│   │   ├── P0_001_BackendApiHealthTests.cs
│   │   └── P0_002_PostgreSqlConnectionTests.cs
│   └── E2E_Examples/                   # End-to-end workflow examples
│       └── E2E_DocumentSigningWorkflowTests.cs
│
└── Application.FunctionalTests/    # Functional tests (WebApplicationFactory)
```

---

## 🚀 Quick Start

### Prerequisites

- .NET 8.0 SDK
- Docker Desktop (for Testcontainers)
- Git

### Run All Tests

```bash
# From repository root
cd AcadSign.Backend

# Run all tests
dotnet test

# Run specific test project
dotnet test tests/Application.UnitTests
dotnet test tests/Infrastructure.IntegrationTests
dotnet test tests/Application.FunctionalTests

# Run tests by category
dotnet test --filter "Category=P0"
dotnet test --filter "Category=Infrastructure"
dotnet test --filter "Category=E2E"
```

### Run Tests with Coverage

```bash
dotnet test --collect:"XPlat Code Coverage"
```

---

## 🧪 Test Factories

Test factories generate realistic fake data using [Bogus](https://github.com/bchavez/Bogus).

### StudentFactory

```csharp
var factory = new StudentFactory();

// Generate single student
var student = factory.Generate();

// Generate with specific CIN
var student = factory.WithCin("AB123456");

// Generate for institution
var student = factory.ForInstitution(institutionId);
```

### DocumentFactory

```csharp
var factory = new DocumentFactory();

// Generate unsigned document
var document = factory.Unsigned();

// Generate signed document
var document = factory.Signed();

// Generate specific type
var attestation = factory.AttestationScolarite();

// Generate batch
var documents = factory.UnsignedBatch(500);
```

### CertificateFactory

```csharp
var factory = new CertificateFactory();

// Valid certificate
var cert = factory.Generate();

// Expiring soon
var cert = factory.ExpiringSoon(15); // 15 days

// Expired certificate
var cert = factory.Expired();
```

**Full documentation**: `Application.UnitTests/Common/Factories/README.md`

---

## 🐳 Testcontainers Integration

Integration tests use [Testcontainers](https://dotnet.testcontainers.org/) for complete isolation.

### How It Works

1. **OneTimeSetUp**: Start PostgreSQL + MinIO containers (once per test class)
2. **SetUp**: Reset database with Respawn (before each test)
3. **Test Execution**: Run test with isolated data
4. **OneTimeTearDown**: Stop and cleanup containers

### Example Integration Test

```csharp
[TestFixture]
public class MyIntegrationTests : IntegrationTestBase
{
    [Test]
    public async Task MyTest()
    {
        // Database is clean and ready
        var student = new StudentFactory().Generate();
        
        Database.DbContext.Set<Student>().Add(student);
        await Database.DbContext.SaveChangesAsync();
        
        // Assert...
    }
}
```

### Benefits

- ✅ **Complete Isolation**: Each test runs in clean environment
- ✅ **No Mocking**: Real PostgreSQL and MinIO instances
- ✅ **Parallel Safe**: Tests can run in parallel
- ✅ **CI/CD Ready**: Works in GitHub Actions

---

## 📊 Test Categories

Tests are organized by priority and type using NUnit categories:

### By Priority

- `[Category("P0")]` - Critical tests (must pass)
- `[Category("P1")]` - High priority tests
- `[Category("P2")]` - Medium priority tests
- `[Category("P3")]` - Low priority tests

### By Type

- `[Category("Infrastructure")]` - Infrastructure tests
- `[Category("Database")]` - Database tests
- `[Category("Security")]` - Security tests
- `[Category("Performance")]` - Performance tests
- `[Category("E2E")]` - End-to-end tests
- `[Category("Example")]` - Example/documentation tests

### Run by Category

```bash
# Run only P0 tests
dotnet test --filter "Category=P0"

# Run infrastructure tests
dotnet test --filter "Category=Infrastructure"

# Run P0 OR P1 tests
dotnet test --filter "Category=P0|Category=P1"

# Run P0 AND Infrastructure tests
dotnet test --filter "Category=P0&Category=Infrastructure"
```

---

## 🔄 CI/CD Pipeline

Tests run automatically in GitHub Actions on every push and pull request.

### Workflow: `.github/workflows/dotnet-tests.yml`

**Jobs:**

1. **unit-tests** (~2 min)
   - Application.UnitTests
   - Domain.UnitTests
   - No external dependencies

2. **integration-tests** (~10 min)
   - Infrastructure.IntegrationTests
   - Uses Testcontainers (PostgreSQL + MinIO)
   - Filters: `Category=P0|Category=Infrastructure`

3. **functional-tests** (~5 min)
   - Application.FunctionalTests
   - WebApplicationFactory tests

4. **test-summary**
   - Aggregates results
   - Posts PR comment with status

### Viewing Results

- **GitHub Actions**: Check "Actions" tab in repository
- **PR Comments**: Automated comment with test summary
- **Code Coverage**: Uploaded to Codecov

---

## 📝 Writing New Tests

### 1. Unit Test Example

```csharp
[TestFixture]
[Category("P1")]
public class MyServiceTests
{
    private MyService _service;
    private Mock<IDependency> _mockDependency;

    [SetUp]
    public void SetUp()
    {
        _mockDependency = new Mock<IDependency>();
        _service = new MyService(_mockDependency.Object);
    }

    [Test]
    public async Task MyMethod_ShouldSucceed_WhenValid()
    {
        // Arrange
        var input = new StudentFactory().Generate();
        _mockDependency.Setup(x => x.DoSomething()).ReturnsAsync(true);

        // Act
        var result = await _service.MyMethod(input);

        // Assert
        result.Should().BeTrue();
        _mockDependency.Verify(x => x.DoSomething(), Times.Once);
    }
}
```

### 2. Integration Test Example

```csharp
[TestFixture]
[Category("P0")]
[Category("Database")]
public class MyRepositoryTests : IntegrationTestBase
{
    [Test]
    public async Task SaveStudent_ShouldPersist_ToDatabase()
    {
        // Arrange
        var student = new StudentFactory().Generate();

        // Act
        Database.DbContext.Set<Student>().Add(student);
        await Database.DbContext.SaveChangesAsync();

        // Assert
        var saved = await Database.DbContext.Set<Student>()
            .FirstOrDefaultAsync(s => s.Id == student.Id);
        
        saved.Should().NotBeNull();
        saved!.CIN.Should().Be(student.CIN);
    }
}
```

### 3. E2E Test Example

```csharp
[TestFixture]
[Category("E2E")]
public class CompleteWorkflowTests : IntegrationTestBase
{
    [Test]
    public async Task SignDocument_EndToEnd_ShouldSucceed()
    {
        // Arrange
        var student = new StudentFactory().Generate();
        var document = new DocumentFactory().ForStudent(student.Id).Unsigned();
        var certificate = new CertificateFactory().Generate();

        // Act
        // ... complete workflow ...

        // Assert
        document.Status.Should().Be("SIGNED");
    }
}
```

---

## 🎯 Test Coverage Goals

**Target Coverage**: 80% overall

- **Domain Layer**: 90%+ (pure business logic)
- **Application Layer**: 85%+ (use cases, commands, queries)
- **Infrastructure Layer**: 70%+ (integration points)
- **Web Layer**: 60%+ (controllers, middleware)

### Check Coverage

```bash
# Generate coverage report
dotnet test --collect:"XPlat Code Coverage"

# Install ReportGenerator
dotnet tool install -g dotnet-reportgenerator-globaltool

# Generate HTML report
reportgenerator \
  -reports:"**/coverage.cobertura.xml" \
  -targetdir:"coveragereport" \
  -reporttypes:Html

# Open report
open coveragereport/index.html
```

---

## 🐛 Debugging Tests

### Visual Studio

1. Right-click test → Debug Test(s)
2. Set breakpoints in test code
3. Step through execution

### VS Code

1. Install C# extension
2. Set breakpoints
3. Run → Start Debugging (F5)

### Command Line

```bash
# Run single test
dotnet test --filter "FullyQualifiedName~MyTestName"

# Run with verbose output
dotnet test --logger "console;verbosity=detailed"

# Run with diagnostic output
dotnet test --diag:log.txt
```

---

## 📚 Dependencies

### Test Frameworks

- **NUnit** (3.x) - Test framework
- **FluentAssertions** (6.12.1) - Fluent assertions
- **Moq** (4.x) - Mocking framework

### Test Infrastructure

- **Testcontainers** (3.9.0) - Container orchestration
- **Testcontainers.PostgreSql** (3.9.0) - PostgreSQL container
- **Testcontainers.Minio** (3.9.0) - MinIO container
- **Respawn** (6.2.1) - Database cleanup

### Test Data

- **Bogus** (35.6.1) - Fake data generation

---

## 🔗 References

- **Test Design Architecture**: `/_bmad-output/test-artifacts/test-design-architecture.md`
- **Test Design QA**: `/_bmad-output/test-artifacts/test-design-qa.md`
- **Factory Documentation**: `Application.UnitTests/Common/Factories/README.md`
- **NUnit Documentation**: https://docs.nunit.org/
- **Testcontainers Documentation**: https://dotnet.testcontainers.org/
- **Bogus Documentation**: https://github.com/bchavez/Bogus

---

## 🚨 Troubleshooting

### Docker Not Running

**Error**: `Cannot connect to Docker daemon`

**Solution**: Start Docker Desktop

### Port Already in Use

**Error**: `Port 5432 already in use`

**Solution**: Stop local PostgreSQL or change Testcontainers port

### Tests Timeout

**Error**: `Test exceeded timeout`

**Solution**: Increase timeout in test or check Docker resources

### Database Not Clean

**Error**: `Expected 0 records, found X`

**Solution**: Ensure `IntegrationTestBase.SetUp()` is called

---

## 📞 Support

For questions or issues:

1. Check test documentation in `/_bmad-output/test-artifacts/`
2. Review factory examples in `Common/Factories/README.md`
3. Check E2E examples in `E2E_Examples/`
4. Contact QA team

---

**Last Updated**: 2026-03-05
**Test Infrastructure Version**: 1.0.0
