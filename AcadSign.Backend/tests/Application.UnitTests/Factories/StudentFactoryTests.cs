using AcadSign.Backend.Application.UnitTests.Common.Factories;
using FluentAssertions;
using NUnit.Framework;

namespace AcadSign.Backend.Application.UnitTests.Factories;

/// <summary>
/// Tests for StudentFactory to ensure test data generation works correctly
/// </summary>
[TestFixture]
public class StudentFactoryTests
{
    private StudentFactory _factory = null!;

    [SetUp]
    public void Setup()
    {
        _factory = new StudentFactory();
    }

    [Test]
    public void Generate_ShouldCreateValidStudent()
    {
        // Act
        var student = _factory.Generate();

        // Assert
        student.Should().NotBeNull();
        student.Id.Should().BeGreaterThan(0);
        student.CIN.Should().NotBeNullOrEmpty();
        student.CIN.Length.Should().Be(8);
        student.CNE.Should().NotBeNullOrEmpty();
        student.CNE.Should().StartWith("E");
        student.FirstName.Should().NotBeNullOrEmpty();
        student.LastName.Should().NotBeNullOrEmpty();
        student.Email.Should().NotBeNullOrEmpty();
        student.Email.Should().Contain("@uh2.ac.ma");
        student.DateOfBirth.Should().BeBefore(DateTime.Now.AddYears(-18));
        student.InstitutionId.Should().NotBeEmpty();
    }

    [Test]
    public void Generate_WithCount_ShouldCreateMultipleStudents()
    {
        // Act
        var students = _factory.Generate(10);

        // Assert
        students.Should().HaveCount(10);
        students.Should().OnlyHaveUniqueItems(s => s.CIN);
        students.Should().OnlyHaveUniqueItems(s => s.CNE);
        students.Should().OnlyHaveUniqueItems(s => s.Email);
    }

    [Test]
    public void WithCin_ShouldSetSpecificCin()
    {
        // Arrange
        var expectedCin = "AB123456";

        // Act
        var student = _factory.WithCin(expectedCin);

        // Assert
        student.CIN.Should().Be(expectedCin);
    }

    [Test]
    public void WithCne_ShouldSetSpecificCne()
    {
        // Arrange
        var expectedCne = "E12345678";

        // Act
        var student = _factory.WithCne(expectedCne);

        // Assert
        student.CNE.Should().Be(expectedCne);
    }

    [Test]
    public void WithEmail_ShouldSetSpecificEmail()
    {
        // Arrange
        var expectedEmail = "test.student@uh2.ac.ma";

        // Act
        var student = _factory.WithEmail(expectedEmail);

        // Assert
        student.Email.Should().Be(expectedEmail);
    }

    [Test]
    public void ForInstitution_ShouldSetSpecificInstitutionId()
    {
        // Arrange
        var institutionId = Guid.NewGuid();

        // Act
        var student = _factory.ForInstitution(institutionId);

        // Assert
        student.InstitutionId.Should().Be(institutionId);
    }

    [Test]
    public void WithOverrides_ShouldApplyCustomConfiguration()
    {
        // Arrange
        var expectedFirstName = "Ahmed";
        var expectedLastName = "Benali";

        // Act
        var student = _factory.WithOverrides(s =>
        {
            s.FirstName = expectedFirstName;
            s.LastName = expectedLastName;
        });

        // Assert
        student.FirstName.Should().Be(expectedFirstName);
        student.LastName.Should().Be(expectedLastName);
    }

    [Test]
    public void Generate_ShouldCreateDifferentStudentsEachTime()
    {
        // Act
        var student1 = _factory.Generate();
        var student2 = _factory.Generate();

        // Assert
        student1.CIN.Should().NotBe(student2.CIN);
        student1.CNE.Should().NotBe(student2.CNE);
        student1.Email.Should().NotBe(student2.Email);
    }
}
