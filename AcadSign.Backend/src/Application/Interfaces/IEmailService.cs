namespace AcadSign.Backend.Application.Services;

public interface IEmailService
{
    Task SendDocumentReadyEmailAsync(
        string toEmail, 
        string studentName, 
        DocumentMetadata document, 
        string downloadUrl,
        string language = "fr");
}

public class DocumentMetadata
{
    public Guid DocumentId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public DateTime IssuedDate { get; set; }
    public DateTime ExpiryDate { get; set; }
}
