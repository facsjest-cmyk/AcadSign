using AcadSign.Backend.Domain.Enums;

namespace AcadSign.Backend.Application.Common.Models;

public class BatchDocumentRequest
{
    public string StudentId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string CIN { get; set; } = string.Empty;
    public string CNE { get; set; } = string.Empty;
    public DocumentType DocumentType { get; set; }
    public string ProgramName { get; set; } = string.Empty;
    public string AcademicYear { get; set; } = string.Empty;
}
