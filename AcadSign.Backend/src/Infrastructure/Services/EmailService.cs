using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using AcadSign.Backend.Application.Services;
using AcadSign.Backend.Domain.Entities;

namespace AcadSign.Backend.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly IAuditLogService _auditService;
    private readonly ILogger<EmailService> _logger;
    
    public EmailService(
        IConfiguration configuration,
        IAuditLogService auditService,
        ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _auditService = auditService;
        _logger = logger;
    }
    
    public async Task SendDocumentReadyEmailAsync(
        string toEmail,
        string studentName,
        DocumentMetadata document,
        string downloadUrl,
        string language = "fr")
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                "Service de Scolarité - Université Hassan II",
                _configuration["Email:FromAddress"]));
            message.To.Add(new MailboxAddress(studentName, toEmail));
            
            var (subject, body) = language == "ar" 
                ? GetArabicTemplate(studentName, document, downloadUrl)
                : GetFrenchTemplate(studentName, document, downloadUrl);
            
            message.Subject = subject;
            
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = body,
                TextBody = StripHtml(body)
            };
            message.Body = bodyBuilder.ToMessageBody();
            
            using var client = new SmtpClient();
            await client.ConnectAsync(
                _configuration["Email:SmtpHost"],
                int.Parse(_configuration["Email:SmtpPort"] ?? "587"),
                SecureSocketOptions.StartTls);
            
            await client.AuthenticateAsync(
                _configuration["Email:Username"],
                _configuration["Email:Password"]);
            
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            
            await _auditService.LogEventAsync(AuditEventType.EMAIL_SENT, document.DocumentId, new
            {
                toEmail = toEmail,
                documentType = document.DocumentType,
                language = language
            });
            
            _logger.LogInformation("Email sent to {Email} for document {DocumentId}", 
                toEmail, document.DocumentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            throw;
        }
    }
    
    private (string subject, string body) GetFrenchTemplate(
        string studentName,
        DocumentMetadata document,
        string downloadUrl)
    {
        var subject = "Votre document académique est prêt - Université Hassan II";
        
        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; }}
        .button {{ display: inline-block; background: #667eea; color: white; padding: 15px 30px; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ background: #333; color: white; padding: 20px; text-align: center; border-radius: 0 0 10px 10px; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎓 AcadSign</h1>
            <p>Université Hassan II Casablanca</p>
        </div>
        <div class='content'>
            <h2>Bonjour {studentName},</h2>
            <p>Votre document académique est maintenant disponible :</p>
            
            <table style='width: 100%; margin: 20px 0;'>
                <tr>
                    <td><strong>Type de document :</strong></td>
                    <td>{document.DocumentType}</td>
                </tr>
                <tr>
                    <td><strong>Date d'émission :</strong></td>
                    <td>{document.IssuedDate:dd/MM/yyyy}</td>
                </tr>
            </table>
            
            <p style='text-align: center;'>
                <a href='{downloadUrl}' class='button'>📄 Télécharger votre document</a>
            </p>
            
            <p style='background: #fff3cd; padding: 15px; border-left: 4px solid #ffc107;'>
                ⚠️ <strong>Important :</strong> Ce lien expirera le {document.ExpiryDate:dd/MM/yyyy à HH:mm}.
            </p>
            
            <p>Pour vérifier l'authenticité de votre document, scannez le QR code présent sur le document ou visitez :</p>
            <p style='text-align: center;'>
                <a href='https://verify.acadsign.ma'>https://verify.acadsign.ma</a>
            </p>
            
            <p>Cordialement,<br>
            Service de Scolarité<br>
            Université Hassan II Casablanca</p>
        </div>
        <div class='footer'>
            <p>Cet email a été envoyé automatiquement. Merci de ne pas y répondre.</p>
            <p>© 2026 Université Hassan II Casablanca - Tous droits réservés</p>
        </div>
    </div>
</body>
</html>";
        
        return (subject, body);
    }
    
    private (string subject, string body) GetArabicTemplate(
        string studentName,
        DocumentMetadata document,
        string downloadUrl)
    {
        var subject = "وثيقتك الأكاديمية جاهزة - جامعة الحسن الثاني";
        
        var body = $@"
<!DOCTYPE html>
<html dir='rtl' lang='ar'>
<head>
    <style>
        body {{ font-family: 'Amiri', Arial, sans-serif; line-height: 1.8; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; }}
        .button {{ display: inline-block; background: #667eea; color: white; padding: 15px 30px; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ background: #333; color: white; padding: 20px; text-align: center; border-radius: 0 0 10px 10px; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎓 AcadSign</h1>
            <p>جامعة الحسن الثاني الدار البيضاء</p>
        </div>
        <div class='content'>
            <h2>مرحبا {studentName}،</h2>
            <p>وثيقتك الأكاديمية متاحة الآن:</p>
            
            <table style='width: 100%; margin: 20px 0;'>
                <tr>
                    <td><strong>نوع الوثيقة:</strong></td>
                    <td>{document.DocumentType}</td>
                </tr>
                <tr>
                    <td><strong>تاريخ الإصدار:</strong></td>
                    <td>{document.IssuedDate:dd/MM/yyyy}</td>
                </tr>
            </table>
            
            <p style='text-align: center;'>
                <a href='{downloadUrl}' class='button'>📄 تحميل وثيقتك</a>
            </p>
            
            <p style='background: #fff3cd; padding: 15px; border-right: 4px solid #ffc107;'>
                ⚠️ <strong>مهم:</strong> سينتهي هذا الرابط في {document.ExpiryDate:dd/MM/yyyy} الساعة {document.ExpiryDate:HH:mm}.
            </p>
            
            <p>للتحقق من صحة وثيقتك، امسح رمز QR الموجود على الوثيقة أو قم بزيارة:</p>
            <p style='text-align: center;'>
                <a href='https://verify.acadsign.ma'>https://verify.acadsign.ma</a>
            </p>
            
            <p>مع أطيب التحيات،<br>
            مصلحة الشؤون الدراسية<br>
            جامعة الحسن الثاني الدار البيضاء</p>
        </div>
        <div class='footer'>
            <p>تم إرسال هذا البريد الإلكتروني تلقائيًا. يرجى عدم الرد عليه.</p>
            <p>© 2026 جامعة الحسن الثاني الدار البيضاء - جميع الحقوق محفوظة</p>
        </div>
    </div>
</body>
</html>";
        
        return (subject, body);
    }
    
    private string StripHtml(string html)
    {
        return System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty);
    }
}
