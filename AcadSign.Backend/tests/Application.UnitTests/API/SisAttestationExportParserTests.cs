using AcadSign.Backend.Application.Services;
using FluentAssertions;
using NUnit.Framework;

namespace AcadSign.Backend.Application.UnitTests.API;

[TestFixture]
[Category("P0")]
[Category("SIS")]
public class SisAttestationExportParserTests
{
    [Test]
    public void Parse_Nominal_ReturnsTypedItems()
    {
        var parser = new SisAttestationExportParser();

        var json = "[" +
                   "{\"nom\":\"DOE\",\"prenom\":\"JOHN\",\"apogee\":\"A1\",\"filiere\":\"INFO\"}," +
                   "{\"nom\":\"SMITH\",\"prenom\":\"JANE\",\"apogee\":\"A2\",\"filiere\":\"MATH\"}" +
                   "]";

        var result = parser.Parse(json);

        result.Errors.Should().BeEmpty();
        result.Items.Should().HaveCount(2);
        result.Items[0].Nom.Should().Be("DOE");
        result.Items[0].Prenom.Should().Be("JOHN");
        result.Items[0].Apogee.Should().Be("A1");
        result.Items[0].Filiere.Should().Be("INFO");
    }

    [Test]
    public void Parse_UnknownFields_AreIgnoredAndCapturedInAdditionalFields()
    {
        var parser = new SisAttestationExportParser();

        var json = "[" +
                   "{\"nom\":\"DOE\",\"prenom\":\"JOHN\",\"apogee\":\"A1\",\"filiere\":\"INFO\",\"extra\":\"x\"}" +
                   "]";

        var result = parser.Parse(json);

        result.Errors.Should().BeEmpty();
        result.Items.Should().HaveCount(1);
        result.Items[0].AdditionalFields.ContainsKey("extra").Should().BeTrue();
    }

    [Test]
    public void Parse_MissingRequiredFields_GeneratesErrorItemAndDoesNotCrash()
    {
        var parser = new SisAttestationExportParser();

        var json = "[" +
                   "{\"nom\":\"DOE\",\"prenom\":\"JOHN\",\"apogee\":\"A1\"}," +
                   "{\"nom\":\"OK\",\"prenom\":\"OK\",\"apogee\":\"A2\",\"filiere\":\"OK\"}" +
                   "]";

        var result = parser.Parse(json);

        result.Items.Should().HaveCount(1);
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Code.Should().Be("MISSING_REQUIRED_FIELDS");
        result.Errors[0].ItemIndex.Should().Be(0);
        result.Errors[0].Nom.Should().Be("DOE");
        result.Errors[0].Prenom.Should().Be("JOHN");
        result.Errors[0].Apogee.Should().Be("A1");
    }
}
