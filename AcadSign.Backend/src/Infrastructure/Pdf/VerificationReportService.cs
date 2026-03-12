using AcadSign.Backend.Application.Common.Interfaces;
using AcadSign.Backend.Application.Common.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AcadSign.Backend.Infrastructure.Pdf;

public class VerificationReportService : IVerificationReportService
{
    public Task<byte[]> GenerateVerificationReportAsync(DocumentVerificationResult verification, CancellationToken cancellationToken = default)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Element(c =>
                {
                    c.Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("AcadSign").FontSize(24).Bold();
                            col.Item().Text("Rapport de Vérification de Document").FontSize(14);
                        });

                        row.ConstantItem(160).AlignRight().Text($"Date: {DateTime.UtcNow:dd/MM/yyyy}");
                    });
                });

                page.Content().Element(c =>
                {
                    c.Column(col =>
                    {
                        col.Spacing(10);

                        col.Item().Background(verification.IsValid ? "#d4edda" : "#f8d7da")
                            .Padding(12)
                            .Text(verification.IsValid ? "Document Authentique" : "Document Non Authentique")
                            .FontSize(16)
                            .Bold();

                        col.Item().Text($"DocumentId: {verification.DocumentId}");

                        if (verification.IsValid)
                        {
                            col.Item().Text($"Type: {verification.DocumentType}");
                            col.Item().Text($"Émis par: {verification.IssuedBy}");
                            col.Item().Text($"Étudiant: {verification.StudentName}");
                            col.Item().Text($"Signé le: {(verification.SignedAt.HasValue ? verification.SignedAt.Value.ToString("u") : "N/A")}");
                            col.Item().Text($"Certificat: {verification.CertificateSerial}");
                            col.Item().Text($"Statut certificat: {verification.CertificateStatus}");
                            col.Item().Text($"Valide jusqu'au: {(verification.CertificateValidUntil.HasValue ? verification.CertificateValidUntil.Value.ToString("u") : "N/A")}");
                            col.Item().Text($"Émetteur certificat: {verification.CertificateIssuer}");
                            col.Item().Text($"Algorithme: {verification.SignatureAlgorithm}");
                            col.Item().Text($"Autorité d'horodatage: {verification.TimestampAuthority}");
                        }
                        else
                        {
                            col.Item().Text($"Erreur: {verification.Error}");
                            if (!string.IsNullOrWhiteSpace(verification.Reason))
                            {
                                col.Item().Text($"Raison: {verification.Reason}");
                            }
                            if (verification.RevokedAt.HasValue)
                            {
                                col.Item().Text($"Révoqué le: {verification.RevokedAt.Value:u}");
                            }
                        }
                    });
                });

                page.Footer().AlignCenter().Text("Généré par AcadSign");
            });
        });

        var bytes = doc.GeneratePdf();
        return Task.FromResult(bytes);
    }
}
