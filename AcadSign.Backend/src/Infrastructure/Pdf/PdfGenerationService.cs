using AcadSign.Backend.Application.Common.Interfaces;
using AcadSign.Backend.Application.Common.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AcadSign.Backend.Infrastructure.Pdf;

public class PdfGenerationService : IPdfGenerationService
{
    private readonly ILogger<PdfGenerationService> _logger;
    private readonly IQrCodeService _qrCodeService;
    private readonly string _verificationBaseUrl;

    static PdfGenerationService()
    {
        // Configurer la licence QuestPDF (Community License pour usage académique)
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public PdfGenerationService(
        ILogger<PdfGenerationService> logger,
        IQrCodeService qrCodeService,
        IConfiguration configuration)
    {
        _logger = logger;
        _qrCodeService = qrCodeService;
        _verificationBaseUrl = configuration["VerificationPortal:BaseUrl"] 
            ?? "https://verify.acadsign.ma";
    }

    public async Task<byte[]> GenerateDocumentAsync(DocumentType type, StudentData data)
    {
        return await Task.Run(() =>
        {
            _logger.LogInformation("Generating PDF document of type {DocumentType} for student {StudentId}", 
                type, data.DocumentId);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Element(c => ComposeHeader(c, type, data));
                    page.Content().Element(c => ComposeContent(c, type, data));
                    page.Footer().Element(c => ComposeFooter(c, data));
                });
            });

            var pdfBytes = document.GeneratePdf();
            
            _logger.LogInformation("PDF document generated successfully. Size: {Size} bytes", pdfBytes.Length);
            
            return pdfBytes;
        });
    }

    private void ComposeHeader(IContainer container, DocumentType type, StudentData data)
    {
        container.Row(row =>
        {
            // Logo université (placeholder)
            row.ConstantItem(100).Height(50).Border(1).AlignCenter().AlignMiddle()
                .Text("LOGO").FontSize(10);

            // Titre bilingue (centre)
            row.RelativeItem().Column(column =>
            {
                column.Item().AlignCenter().Text(GetTitleArabic(type))
                    .FontSize(18)
                    .Bold();

                column.Item().AlignCenter().Text(GetTitleFrench(type))
                    .FontSize(16)
                    .Bold();
            });

            // Espace (à droite)
            row.ConstantItem(100);
        });
    }

    private void ComposeContent(IContainer container, DocumentType type, StudentData data)
    {
        container.Column(column =>
        {
            column.Spacing(10);

            // Section données étudiant
            column.Item().Element(c => ComposeStudentInfo(c, data));

            // Contenu spécifique au type de document
            column.Item().Element(c => ComposeDocumentSpecificContent(c, type, data));
        });
    }

    private void ComposeStudentInfo(IContainer container, StudentData data)
    {
        container.Column(column =>
        {
            column.Spacing(5);

            // Nom en arabe
            column.Item().Text($"{data.FirstNameAr} {data.LastNameAr}")
                .FontSize(14)
                .Bold();

            // Nom en français
            column.Item().Text($"{data.FirstNameFr} {data.LastNameFr}")
                .FontSize(12)
                .Bold();

            // CIN et CNE
            column.Item().Row(row =>
            {
                row.RelativeItem().Text($"CIN: {data.CIN}");
                row.RelativeItem().Text($"CNE: {data.CNE}");
            });

            // Date de naissance
            column.Item().Text($"Date de naissance: {data.DateOfBirth:dd/MM/yyyy}");

            // Programme et faculté
            column.Item().Text($"Programme: {data.ProgramNameFr}");
            column.Item().Text($"Faculté: {data.FacultyFr}");
            column.Item().Text($"Année académique: {data.AcademicYear}");
        });
    }

    private void ComposeDocumentSpecificContent(IContainer container, DocumentType type, StudentData data)
    {
        switch (type)
        {
            case DocumentType.AttestationScolarite:
                ComposeAttestationScolarite(container, data);
                break;
            case DocumentType.ReleveNotes:
                ComposeReleveNotes(container, data);
                break;
            case DocumentType.AttestationReussite:
                ComposeAttestationReussite(container, data);
                break;
            case DocumentType.AttestationInscription:
                ComposeAttestationInscription(container, data);
                break;
            default:
                throw new ArgumentException("Type de document inconnu", nameof(type));
        }
    }

    private void ComposeAttestationScolarite(IContainer container, StudentData data)
    {
        container.Column(column =>
        {
            column.Spacing(15);

            // Déclaration bilingue
            column.Item().Column(col =>
            {
                col.Item().Text("نشهد بأن الطالب(ة)")
                    .FontSize(12);

                col.Item().Text("Nous certifions que l'étudiant(e)")
                    .FontSize(12);
            });

            // Nom étudiant (mis en évidence)
            column.Item().PaddingVertical(10).Column(col =>
            {
                col.Item().AlignCenter().Text($"{data.FirstNameAr} {data.LastNameAr}")
                    .FontSize(16)
                    .Bold();

                col.Item().AlignCenter().Text($"{data.FirstNameFr} {data.LastNameFr}")
                    .FontSize(14)
                    .Bold();
            });

            // Informations d'identification
            column.Item().Row(row =>
            {
                row.RelativeItem().Text($"CIN: {data.CIN}").FontSize(11);
                row.RelativeItem().Text($"CNE: {data.CNE}").FontSize(11);
                row.RelativeItem().Text($"Né(e) le: {data.DateOfBirth:dd/MM/yyyy}").FontSize(11);
            });

            // Programme d'études
            column.Item().PaddingTop(15).Column(col =>
            {
                col.Item().Text("مسجل(ة) بصفة قانونية في");

                col.Item().Text("Est régulièrement inscrit(e) en");

                col.Item().PaddingLeft(20).Text(data.ProgramNameAr)
                    .Bold();

                col.Item().PaddingLeft(20).Text(data.ProgramNameFr)
                    .Bold();
            });

            // Faculté
            column.Item().PaddingTop(10).Column(col =>
            {
                col.Item().Text(data.FacultyAr);
                col.Item().Text(data.FacultyFr);
            });

            // Année académique
            column.Item().PaddingTop(10).Text($"Année académique: {data.AcademicYear}")
                .FontSize(11)
                .Bold();

            // Déclaration finale
            column.Item().PaddingTop(20).Column(col =>
            {
                col.Item().Text("هذه الشهادة صالحة لجميع الأغراض القانونية")
                    .FontSize(10)
                    .Italic();

                col.Item().Text("Cette attestation est valable pour toutes fins légales")
                    .FontSize(10)
                    .Italic();
            });
        });
    }

    private void ComposeReleveNotes(IContainer container, StudentData data)
    {
        container.Column(column =>
        {
            column.Spacing(10);

            // Tableau des notes
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3); // Matière AR
                    columns.RelativeColumn(3); // Matière FR
                    columns.RelativeColumn(1); // Note
                    columns.RelativeColumn(1); // Crédits
                });

                // En-tête
                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("المادة")
                        .Bold();
                    header.Cell().Element(CellStyle).Text("Matière").Bold();
                    header.Cell().Element(CellStyle).Text("Note/20").Bold();
                    header.Cell().Element(CellStyle).Text("ECTS").Bold();
                });

                // Lignes de notes
                foreach (var grade in data.Grades)
                {
                    table.Cell().Element(CellStyle).Text(grade.SubjectNameAr);
                    table.Cell().Element(CellStyle).Text(grade.SubjectNameFr);
                    table.Cell().Element(CellStyle).Text(grade.Score.ToString("F2"));
                    table.Cell().Element(CellStyle).Text(grade.Credits.ToString());
                }
            });

            // Résultats globaux
            column.Item().PaddingTop(20).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text($"Moyenne Générale (GPA): {data.GPA:F2}/20")
                        .FontSize(12)
                        .Bold();

                    col.Item().Text($"Mention: {data.Mention}")
                        .FontSize(12)
                        .Bold();

                    col.Item().Text($"Total Crédits: {data.Grades.Sum(g => g.Credits)} ECTS")
                        .FontSize(11);
                });
            });
        });
    }

    private void ComposeAttestationReussite(IContainer container, StudentData data)
    {
        container.Column(column =>
        {
            column.Spacing(15);

            // Déclaration
            column.Item().AlignCenter().Column(col =>
            {
                col.Item().Text("نشهد بأن")
                    .FontSize(12);

                col.Item().Text("Nous certifions que")
                    .FontSize(12);
            });

            // Nom étudiant
            column.Item().AlignCenter().PaddingVertical(10).Column(col =>
            {
                col.Item().Text($"{data.FirstNameAr} {data.LastNameAr}")
                    .FontSize(18)
                    .Bold();

                col.Item().Text($"{data.FirstNameFr} {data.LastNameFr}")
                    .FontSize(16)
                    .Bold();
            });

            // Diplôme obtenu
            column.Item().AlignCenter().Column(col =>
            {
                col.Item().Text("حصل(ت) على");

                col.Item().Text("A obtenu le diplôme de");

                col.Item().PaddingTop(10).Text(data.DegreeNameAr)
                    .FontSize(14)
                    .Bold();

                col.Item().Text(data.DegreeNameFr)
                    .FontSize(14)
                    .Bold();
            });

            // Année et mention
            column.Item().PaddingTop(15).AlignCenter().Column(col =>
            {
                col.Item().Text($"Année: {data.GraduationYear}")
                    .FontSize(12)
                    .Bold();

                col.Item().Text($"Mention: {data.Mention}")
                    .FontSize(12)
                    .Bold();
            });
        });
    }

    private void ComposeAttestationInscription(IContainer container, StudentData data)
    {
        container.Column(column =>
        {
            column.Spacing(15);

            // Déclaration
            column.Item().Text("نشهد بأن الطالب(ة) المذكور(ة) أدناه مسجل(ة) لدينا");

            column.Item().Text("Nous certifions que l'étudiant(e) ci-dessous est inscrit(e) auprès de notre établissement");

            // Nom et informations
            column.Item().PaddingTop(10).Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.ConstantItem(150).Text("الاسم الكامل:");
                    row.RelativeItem().Text($"{data.FirstNameAr} {data.LastNameAr}")
                        .Bold();
                });

                col.Item().Row(row =>
                {
                    row.ConstantItem(150).Text("Nom complet:");
                    row.RelativeItem().Text($"{data.FirstNameFr} {data.LastNameFr}").Bold();
                });
            });

            // Programme
            column.Item().PaddingTop(10).Column(col =>
            {
                col.Item().Text($"Programme: {data.ProgramNameFr}").Bold();
                col.Item().Text($"Année académique: {data.AcademicYear}");
                col.Item().Text($"Date d'inscription: {data.EnrollmentDate:dd/MM/yyyy}");
                col.Item().Text($"Statut: {data.EnrollmentStatus}");
            });
        });
    }

    private static IContainer CellStyle(IContainer container)
    {
        return container
            .Border(1)
            .BorderColor(Colors.Grey.Lighten2)
            .Padding(5);
    }

    private void ComposeFooter(IContainer container, StudentData data)
    {
        container.Row(row =>
        {
            // Date d'émission (gauche)
            row.RelativeItem().Column(col =>
            {
                col.Item().Text($"Émis le: {DateTime.Now:dd/MM/yyyy}")
                    .FontSize(10);
                col.Item().Text($"Document ID: {data.DocumentId}")
                    .FontSize(8);
            });

            // QR Code (droite)
            row.ConstantItem(80).Column(col =>
            {
                // Générer le QR code
                var verificationUrl = $"{_verificationBaseUrl}/documents/{data.DocumentId}";
                var qrCodeBytes = _qrCodeService.GenerateQrCode(verificationUrl);

                // Embedder le QR code
                col.Item().Height(80).Width(80).Image(qrCodeBytes);

                // Légende bilingue
                col.Item().AlignCenter().Text("رمز التحقق")
                    .FontSize(8);

                col.Item().AlignCenter().Text("Code de Vérification")
                    .FontSize(8);
            });
        });
    }

    private string GetTitleArabic(DocumentType type)
    {
        return type switch
        {
            DocumentType.AttestationScolarite => "شهادة مدرسية",
            DocumentType.ReleveNotes => "كشف النقاط",
            DocumentType.AttestationReussite => "شهادة نجاح",
            DocumentType.AttestationInscription => "شهادة تسجيل",
            _ => throw new ArgumentException("Type de document inconnu", nameof(type))
        };
    }

    private string GetTitleFrench(DocumentType type)
    {
        return type switch
        {
            DocumentType.AttestationScolarite => "Attestation de Scolarité",
            DocumentType.ReleveNotes => "Relevé de Notes",
            DocumentType.AttestationReussite => "Attestation de Réussite",
            DocumentType.AttestationInscription => "Attestation d'Inscription",
            _ => throw new ArgumentException("Type de document inconnu", nameof(type))
        };
    }

}
