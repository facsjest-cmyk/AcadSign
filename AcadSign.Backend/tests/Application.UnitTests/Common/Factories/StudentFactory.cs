using AcadSign.Backend.Domain.Entities;
using Bogus;

namespace AcadSign.Backend.Application.UnitTests.Common.Factories;

/// <summary>
/// Factory for generating test Student entities with realistic fake data.
/// Uses Bogus library for data generation.
/// </summary>
public class StudentFactory
{
    private readonly Faker<Student> _faker;

    public StudentFactory()
    {
        _faker = new Faker<Student>()
            .RuleFor(s => s.Id, f => f.Random.Int(1, 100000))
            .RuleFor(s => s.PublicId, f => Guid.NewGuid())
            .RuleFor(s => s.CIN, f => f.Random.AlphaNumeric(8).ToUpper())
            .RuleFor(s => s.CNE, f => $"E{f.Random.Number(10000000, 99999999)}")
            .RuleFor(s => s.FirstName, f => f.Name.FirstName())
            .RuleFor(s => s.LastName, f => f.Name.LastName())
            .RuleFor(s => s.Email, (f, s) => f.Internet.Email(s.FirstName, s.LastName, "uh2.ac.ma"))
            .RuleFor(s => s.PhoneNumber, f => f.Phone.PhoneNumber("+212 6## ## ## ##"))
            .RuleFor(s => s.DateOfBirth, f => f.Date.Past(25, DateTime.Now.AddYears(-18)))
            .RuleFor(s => s.InstitutionId, f => Guid.NewGuid());
    }

    /// <summary>
    /// Generate a single Student with random data
    /// </summary>
    public Student Generate() => _faker.Generate();

    /// <summary>
    /// Generate multiple Students with random data
    /// </summary>
    public List<Student> Generate(int count) => _faker.Generate(count);

    /// <summary>
    /// Generate a Student with specific overrides
    /// </summary>
    public Student WithOverrides(Action<Student> configure)
    {
        var student = Generate();
        configure(student);
        return student;
    }

    /// <summary>
    /// Generate a Student with a specific CIN
    /// </summary>
    public Student WithCin(string cin)
    {
        var student = Generate();
        student.CIN = cin;
        return student;
    }

    /// <summary>
    /// Generate a Student with a specific CNE
    /// </summary>
    public Student WithCne(string cne)
    {
        var student = Generate();
        student.CNE = cne;
        return student;
    }

    /// <summary>
    /// Generate a Student with a specific email
    /// </summary>
    public Student WithEmail(string email)
    {
        var student = Generate();
        student.Email = email;
        return student;
    }

    /// <summary>
    /// Generate a Student for a specific institution
    /// </summary>
    public Student ForInstitution(Guid institutionId)
    {
        var student = Generate();
        student.InstitutionId = institutionId;
        return student;
    }
}
