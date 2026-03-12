using System.Text.Json;
using AcadSign.Backend.Application.Models;

namespace AcadSign.Backend.Application.Services;

public class SisAttestationExportParser
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public SisAttestationExportResult Parse(string json)
    {
        var result = new SisAttestationExportResult();

        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.ValueKind != JsonValueKind.Array)
        {
            throw new FormatException("SIS export payload must be a JSON array");
        }

        var index = 0;
        foreach (var element in doc.RootElement.EnumerateArray())
        {
            if (element.ValueKind != JsonValueKind.Object)
            {
                result.Errors.Add(new SisAttestationExportItemError
                {
                    ItemIndex = index,
                    Code = "INVALID_ITEM",
                    Message = "Item JSON invalide (doit être un objet)"
                });
                index++;
                continue;
            }

            SisAttestationStudentDto? dto;
            try
            {
                dto = element.Deserialize<SisAttestationStudentDto>(JsonOptions);
            }
            catch (Exception)
            {
                result.Errors.Add(new SisAttestationExportItemError
                {
                    ItemIndex = index,
                    Code = "INVALID_ITEM_JSON",
                    Message = "Impossible de désérialiser l'item"
                });
                index++;
                continue;
            }

            if (dto == null)
            {
                result.Errors.Add(new SisAttestationExportItemError
                {
                    ItemIndex = index,
                    Code = "INVALID_ITEM_JSON",
                    Message = "Item vide ou invalide"
                });
                index++;
                continue;
            }

            var missing = new List<string>();
            if (string.IsNullOrWhiteSpace(dto.Nom)) missing.Add("nom");
            if (string.IsNullOrWhiteSpace(dto.Prenom)) missing.Add("prenom");
            if (string.IsNullOrWhiteSpace(dto.Apogee)) missing.Add("apogee");
            if (string.IsNullOrWhiteSpace(dto.Filiere)) missing.Add("filiere");

            if (missing.Count > 0)
            {
                result.Errors.Add(new SisAttestationExportItemError
                {
                    ItemIndex = index,
                    Code = "MISSING_REQUIRED_FIELDS",
                    Message = $"Champs requis manquants: {string.Join(", ", missing)}",
                    Nom = dto.Nom,
                    Prenom = dto.Prenom,
                    Apogee = dto.Apogee,
                    Filiere = dto.Filiere
                });
                index++;
                continue;
            }

            result.Items.Add(dto);
            index++;
        }

        return result;
    }
}
