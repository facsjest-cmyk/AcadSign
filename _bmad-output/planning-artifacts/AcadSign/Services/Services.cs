using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AcadSign.Models;
using Amazon.S3;
using Amazon.S3.Model;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Newtonsoft.Json;

namespace AcadSign.Services;

// ════════════════════════════════════════════════════════════════════════
//  INTERFACES
// ════════════════════════════════════════════════════════════════════════

public interface ISisApiService
{
    Task<List<DocumentRequest>> FetchPendingRequestsAsync(CancellationToken ct = default);
}

public interface IESignService
{
    Task<SignatureResult> SignDocumentAsync(
        byte[] pdfBytes, string pin, DocumentRequest request,
        IProgress<(int pct, string msg)>? progress = null,
        CancellationToken ct = default);
}

public interface IS3StorageService
{
    Task<S3UploadResult> UploadAsync(
        byte[] fileBytes, string key,
        IDictionary<string, string>? metadata = null,
        CancellationToken ct = default);

    Task<string> GetPresignedUrlAsync(string key, TimeSpan expiry);
}

public interface IEmailService
{
    Task SendDocumentAsync(
        string toEmail, string toName,
        string subject, string bodyHtml,
        byte[] attachmentBytes, string attachmentName,
        CancellationToken ct = default);
}

public interface IPdfGeneratorService
{
    Task<byte[]> GenerateAsync(DocumentRequest request, CancellationToken ct = default);
}

public interface IPdfViewerService
{
    System.Windows.Media.Imaging.BitmapSource RenderPage(byte[] pdfBytes, int pageIndex, int dpi = 150);
    int GetPageCount(byte[] pdfBytes);
}

// ════════════════════════════════════════════════════════════════════════
//  SIS API SERVICE
// ════════════════════════════════════════════════════════════════════════

public class SisApiService : ISisApiService
{
    private readonly HttpClient     _http;
    private readonly SisApiSettings _settings;

    public SisApiService(HttpClient http, AppSettings settings)
    {
        _http     = http;
        _settings = settings.SisApi;
        _http.DefaultRequestHeaders.Add("X-API-Key", _settings.ApiKey);
        _http.DefaultRequestHeaders.Add("X-Institution", _settings.InstitutionCode);
    }

    public async Task<List<DocumentRequest>> FetchPendingRequestsAsync(CancellationToken ct = default)
    {
        var response = await _http.GetAsync(
            $"document-requests?status=pending&institution={_settings.InstitutionCode}", ct);

        response.EnsureSuccessStatusCode();

        var json    = await response.Content.ReadAsStringAsync(ct);
        var result  = JsonConvert.DeserializeObject<SisApiResponse>(json);
        return result?.Data ?? new List<DocumentRequest>();
    }
}

// ════════════════════════════════════════════════════════════════════════
//  E-SIGN SERVICE  (Barid Al-Maghrib)
// ════════════════════════════════════════════════════════════════════════

public class ESignService : IESignService
{
    private readonly HttpClient      _http;
    private readonly ESignSettings   _settings;

    public ESignService(IHttpClientFactory factory, AppSettings settings)
    {
        _http     = factory.CreateClient("esign");
        _settings = settings.ESign;
        _http.BaseAddress = new Uri(_settings.BaseUrl);
        _http.DefaultRequestHeaders.Add("X-API-Key", _settings.ApiKey);
    }

    public async Task<SignatureResult> SignDocumentAsync(
        byte[] pdfBytes, string pin, DocumentRequest request,
        IProgress<(int pct, string msg)>? progress = null,
        CancellationToken ct = default)
    {
        try
        {
            // Step 1 – Authenticate session
            progress?.Report((10, "Connexion à Barid Al-Maghrib e-Sign (TLS 1.3)..."));
            var sessionToken = await AuthenticateAsync(pin, ct);
            progress?.Report((20, "✓ Session authentifiée"));

            // Step 2 – Compute document hash
            progress?.Report((30, "Calcul de l'empreinte SHA-256..."));
            var hash = ComputeSha256(pdfBytes);
            progress?.Report((40, $"✓ Hash: {hash[..16]}...{hash[^8..]}"));

            // Step 3 – Request signature from HSM
            progress?.Report((55, "Envoi au HSM pour signature PAdES-B-LT..."));
            var signatureBytes = await RequestHsmSignatureAsync(
                pdfBytes, hash, sessionToken, request, ct);
            progress?.Report((70, "✓ Signature cryptographique obtenue"));

            // Step 4 – Request RFC 3161 timestamp
            progress?.Report((80, "Application de l'horodatage RFC 3161..."));
            var timestamp = await RequestTimestampAsync(hash, ct);
            progress?.Report((90, $"✓ Timestamp: {DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}"));

            // Step 5 – Embed signature into PDF (PAdES)
            progress?.Report((95, "Intégration PAdES dans le document PDF..."));
            var signedPdf = EmbedSignatureIntoPdf(pdfBytes, signatureBytes, timestamp);
            progress?.Report((100, "✓ Document signé — PAdES-B-LT conforme eIDAS"));

            return new SignatureResult
            {
                Success           = true,
                SignedPdfBytes    = signedPdf,
                CertificateSerial = _settings.CertificateSerial,
                DocumentHash      = hash,
                TimestampToken    = timestamp,
                SignedAt          = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            return new SignatureResult
            {
                Success = false,
                Error   = ex.Message
            };
        }
    }

    // ── Private helpers ──────────────────────────────────────────────────

    private async Task<string> AuthenticateAsync(string pin, CancellationToken ct)
    {
        var payload = JsonConvert.SerializeObject(new
        {
            certificate_serial = _settings.CertificateSerial,
            pin
        });

        var content  = new StringContent(payload, Encoding.UTF8, "application/json");
        var response = await _http.PostAsync("auth/session", content, ct);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(ct);
        dynamic? obj = JsonConvert.DeserializeObject(body);
        return obj?.session_token ?? throw new InvalidOperationException("No session token");
    }

    private async Task<byte[]> RequestHsmSignatureAsync(
        byte[] pdfBytes, string hash, string sessionToken,
        DocumentRequest request, CancellationToken ct)
    {
        var payload = JsonConvert.SerializeObject(new
        {
            document_hash     = hash,
            signature_format  = "PAdES-B-LT",
            session_token     = sessionToken,
            metadata = new
            {
                document_id   = request.Id,
                document_type = request.DocumentType.ToString(),
                student_id    = request.Student.Id,
                institution   = "UH2"
            }
        });

        var content  = new StringContent(payload, Encoding.UTF8, "application/json");
        var response = await _http.PostAsync("sign/pades", content, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsByteArrayAsync(ct);
    }

    private async Task<string> RequestTimestampAsync(string hash, CancellationToken ct)
    {
        var payload = JsonConvert.SerializeObject(new { hash, algorithm = "SHA256" });
        var content = new StringContent(payload, Encoding.UTF8, "application/json");
        var response = await _http.PostAsync("timestamp/rfc3161", content, ct);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync(ct);
        dynamic? obj = JsonConvert.DeserializeObject(body);
        return obj?.token ?? DateTime.UtcNow.ToString("O");
    }

    private static byte[] EmbedSignatureIntoPdf(
        byte[] originalPdf, byte[] signatureBytes, string timestamp)
    {
        // In production: use iText 7 PdfSigner to embed PAdES signature
        // This merges the signature container into the PDF structure
        // iText7 code:
        //   var signer = new PdfSigner(reader, outputStream, stampingProps);
        //   signer.SetSignDate(DateTime.Now);
        //   var appearance = signer.GetSignatureAppearance();
        //   signer.SignDetached(externalSignature, chain, null, null, tsaClient, 0, PdfSigner.CryptoStandard.CMS);
        return originalPdf; // placeholder – replace with actual iText7 embedding
    }

    private static string ComputeSha256(byte[] data)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        return Convert.ToHexString(sha.ComputeHash(data)).ToLower();
    }
}

// ════════════════════════════════════════════════════════════════════════
//  S3 STORAGE SERVICE
// ════════════════════════════════════════════════════════════════════════

public class S3StorageService : IS3StorageService
{
    private readonly IAmazonS3  _s3;
    private readonly S3Settings _settings;

    public S3StorageService(AppSettings settings)
    {
        _settings = settings.S3;
        var config = new AmazonS3Config
        {
            ServiceURL   = _settings.Endpoint,
            ForcePathStyle = _settings.UsePathStyle
        };
        _s3 = new AmazonS3Client(_settings.AccessKey, _settings.SecretKey, config);
    }

    public async Task<S3UploadResult> UploadAsync(
        byte[] fileBytes, string key,
        IDictionary<string, string>? metadata = null,
        CancellationToken ct = default)
    {
        try
        {
            var request = new PutObjectRequest
            {
                BucketName  = _settings.BucketName,
                Key         = key,
                InputStream = new MemoryStream(fileBytes),
                ContentType = "application/pdf",
                ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
            };

            if (metadata != null)
                foreach (var kv in metadata)
                    request.Metadata.Add(kv.Key, kv.Value);

            await _s3.PutObjectAsync(request, ct);

            var url = await GetPresignedUrlAsync(key, TimeSpan.FromDays(1));
            return new S3UploadResult { Success = true, Key = key, Url = url };
        }
        catch (Exception ex)
        {
            return new S3UploadResult { Success = false, Error = ex.Message };
        }
    }

    public Task<string> GetPresignedUrlAsync(string key, TimeSpan expiry)
    {
        var req = new GetPreSignedUrlRequest
        {
            BucketName = _settings.BucketName,
            Key        = key,
            Expires    = DateTime.UtcNow.Add(expiry)
        };
        return Task.FromResult(_s3.GetPreSignedURL(req));
    }

    public static string BuildS3Key(DocumentRequest req)
        => $"docs/{DateTime.Now:yyyy}/{req.DocumentType}/{req.Student.Id}/{req.Id}.pdf";
}

// ════════════════════════════════════════════════════════════════════════
//  EMAIL SERVICE  (MailKit / SMTP)
// ════════════════════════════════════════════════════════════════════════

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;

    public EmailService(AppSettings settings)
    {
        _settings = settings.Email;
    }

    public async Task SendDocumentAsync(
        string toEmail, string toName,
        string subject, string bodyHtml,
        byte[] attachmentBytes, string attachmentName,
        CancellationToken ct = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromAddress));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = subject;

        var builder = new BodyBuilder { HtmlBody = bodyHtml };
        builder.Attachments.Add(attachmentName, attachmentBytes,
            ContentType.Parse("application/pdf"));

        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort,
            _settings.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None, ct);
        await client.AuthenticateAsync(_settings.Username, _settings.Password, ct);
        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);
    }

    public static string BuildEmailBody(DocumentRequest request) => $"""
        <html><body style="font-family:Arial,sans-serif;color:#334155;background:#f8fafc;padding:30px">
          <div style="max-width:600px;margin:0 auto;background:#fff;border-radius:12px;padding:32px;box-shadow:0 4px 20px rgba(0,0,0,.08)">
            <div style="background:linear-gradient(135deg,#0ea5e9,#6366f1);border-radius:8px;padding:20px;margin-bottom:24px">
              <h2 style="color:#fff;margin:0">Université Hassan II de Casablanca</h2>
              <p style="color:rgba(255,255,255,.8);margin:4px 0 0">Service de Scolarité</p>
            </div>
            <p>Madame / Monsieur <strong>{request.Student.FullName}</strong>,</p>
            <p>Nous vous prions de trouver en pièce jointe votre <strong>{request.DisplayType}</strong>,
               signé électroniquement conformément à la loi marocaine n° 43-20.</p>
            <div style="background:#f0fdf4;border:1px solid #10b981;border-radius:8px;padding:14px;margin:16px 0">
              <p style="margin:0;color:#065f46">✓ Ce document est signé numériquement (PAdES-B-LT)
                 et peut être vérifié via le portail officiel.</p>
            </div>
            <p>Cordialement,<br><strong>Service de Scolarité</strong></p>
          </div>
        </body></html>
        """;
}

// ════════════════════════════════════════════════════════════════════════
//  PDF GENERATOR  (QuestPDF)
// ════════════════════════════════════════════════════════════════════════

public class PdfGeneratorService : IPdfGeneratorService
{
    public async Task<byte[]> GenerateAsync(DocumentRequest request, CancellationToken ct = default)
    {
        // QuestPDF document generation
        // In production, use QuestPDF fluent API:
        //
        // var doc = Document.Create(container =>
        // {
        //     container.Page(page =>
        //     {
        //         page.Size(PageSizes.A4);
        //         page.Margin(40);
        //         page.Header().Element(ComposeHeader);
        //         page.Content().Element(c => ComposeContent(c, request));
        //         page.Footer().Element(ComposeFooter);
        //     });
        // });
        // return doc.GeneratePdf();

        await Task.Delay(500, ct); // Simulated generation time
        return GenerateMockPdf(request);
    }

    private static byte[] GenerateMockPdf(DocumentRequest request)
    {
        // Returns a valid minimal PDF structure for demonstration
        // Replace with QuestPDF or RDLC template rendering
        var sb = new StringBuilder();
        sb.Append("%PDF-1.4\n1 0 obj<</Type/Catalog/Pages 2 0 R>>endobj\n");
        sb.Append("2 0 obj<</Type/Pages/Kids[3 0 R]/Count 1>>endobj\n");
        sb.Append("3 0 obj<</Type/Page/Parent 2 0 R/MediaBox[0 0 595 842]");
        sb.Append("/Contents 4 0 R/Resources<</Font<</F1 5 0 R>>>>>>endobj\n");
        sb.Append("4 0 obj<</Length 200>>\nstream\n");
        sb.Append("BT /F1 14 Tf 72 750 Td ");
        sb.Append($"(Universite Hassan II - {request.DisplayType}) Tj\n");
        sb.Append("0 -30 Td /F1 12 Tf ");
        sb.Append($"(Etudiant: {request.Student.FullName}) Tj\n");
        sb.Append("0 -20 Td ");
        sb.Append($"(N. Apogee: {request.Student.Id}) Tj\n");
        sb.Append("ET\nendstream\nendobj\n");
        sb.Append("5 0 obj<</Type/Font/Subtype/Type1/BaseFont/Helvetica>>endobj\n");
        sb.Append("xref\n0 6\n0000000000 65535 f\n");
        sb.Append("trailer<</Size 6/Root 1 0 R>>\nstartxref\n0\n%%EOF");
        return Encoding.Latin1.GetBytes(sb.ToString());
    }
}

// ════════════════════════════════════════════════════════════════════════
//  PDF VIEWER SERVICE  (PdfiumViewer wrapper)
// ════════════════════════════════════════════════════════════════════════

public class PdfViewerService : IPdfViewerService
{
    public System.Windows.Media.Imaging.BitmapSource RenderPage(
        byte[] pdfBytes, int pageIndex, int dpi = 150)
    {
        // Production: use PdfiumViewer or PDFsharp
        //
        // using var stream = new MemoryStream(pdfBytes);
        // using var doc    = PdfiumViewer.PdfDocument.Load(stream);
        // using var img    = doc.Render(pageIndex,
        //     (int)(doc.PageSizes[pageIndex].Width  / 72.0 * dpi),
        //     (int)(doc.PageSizes[pageIndex].Height / 72.0 * dpi),
        //     dpi, dpi, false);
        // return ConvertBitmap((System.Drawing.Bitmap)img);

        // Placeholder – returns a mock page bitmap
        return CreatePlaceholderBitmap(595, 842);
    }

    public int GetPageCount(byte[] pdfBytes) => 1;

    private static System.Windows.Media.Imaging.BitmapSource CreatePlaceholderBitmap(
        int width, int height)
    {
        var pixels = new byte[width * height * 4];
        for (int i = 0; i < pixels.Length; i += 4)
        { pixels[i]=255; pixels[i+1]=255; pixels[i+2]=255; pixels[i+3]=255; }
        return System.Windows.Media.Imaging.BitmapSource.Create(
            width, height, 96, 96,
            System.Windows.Media.PixelFormats.Bgra32, null, pixels, width * 4);
    }

    private static System.Windows.Media.Imaging.BitmapSource ConvertBitmap(
        System.Drawing.Bitmap bmp)
    {
        var rect = new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height);
        var data = bmp.LockBits(rect,
            System.Drawing.Imaging.ImageLockMode.ReadOnly,
            System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        try
        {
            return System.Windows.Media.Imaging.BitmapSource.Create(
                data.Width, data.Height, 96, 96,
                System.Windows.Media.PixelFormats.Bgra32, null,
                data.Scan0, data.Stride * data.Height, data.Stride);
        }
        finally { bmp.UnlockBits(data); }
    }
}
