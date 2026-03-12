using AcadSign.Backend.Domain.Enums;

namespace AcadSign.Backend.Application.Models;

public class CreateBatchRequest
{
    public List<DocumentGenerationRequest> Documents { get; set; } = new();
}

public class DocumentGenerationRequest
{
    public string StudentId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string CIN { get; set; } = string.Empty;
    public string CNE { get; set; } = string.Empty;
    public DocumentType DocumentType { get; set; }
    public string AcademicYear { get; set; } = string.Empty;
}
