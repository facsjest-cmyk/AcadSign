using AcadSign.Backend.Domain.Enums;

namespace AcadSign.Backend.Application.Common.Models;

public class SisAttestationBatchFailure
{
    public int? ItemIndex { get; set; }
    public string? Apogee { get; set; }
    public string? Nom { get; set; }
    public string? Prenom { get; set; }
    public string? Filiere { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class SisAttestationBatchGenerationResult
{
    public int Total { get; set; }
    public int Generated { get; set; }
    public int Failed { get; set; }

    public DocumentType DocumentType { get; set; }

    public List<SisAttestationBatchFailure> Failures { get; set; } = new();
    public List<Guid> CreatedDocumentIds { get; set; } = new();
}
