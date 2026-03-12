using System.Text.Json;
using System.Text.Json.Serialization;

namespace AcadSign.Backend.Application.Models;

public class SisAttestationStudentDto
{
    [JsonPropertyName("nom")]
    public string? Nom { get; set; }

    [JsonPropertyName("prenom")]
    public string? Prenom { get; set; }

    [JsonPropertyName("apogee")]
    public string? Apogee { get; set; }

    [JsonPropertyName("filiere")]
    public string? Filiere { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement> AdditionalFields { get; set; } = new();
}

public class SisAttestationExportItemError
{
    public int ItemIndex { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    public string? Nom { get; set; }
    public string? Prenom { get; set; }
    public string? Apogee { get; set; }
    public string? Filiere { get; set; }
}

public class SisAttestationExportResult
{
    public List<SisAttestationStudentDto> Items { get; set; } = new();
    public List<SisAttestationExportItemError> Errors { get; set; } = new();
}
