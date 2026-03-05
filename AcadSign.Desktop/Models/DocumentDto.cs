namespace AcadSign.Desktop.Models;

public class DocumentDto
{
    public Guid Id { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = string.Empty;
}
