using System.Globalization;
using System.Text.Json;
using AcadSign.Backend.Application.Common.Interfaces;
using AcadSign.Backend.Application.Common.Models;
using AcadSign.Backend.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AcadSign.Backend.Infrastructure.Pdf;

public class AttestationTemplateRenderer : IAttestationTemplateRenderer
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly ILogger<AttestationTemplateRenderer> _logger;

    static AttestationTemplateRenderer()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public AttestationTemplateRenderer(
        IConfiguration configuration,
        IHostEnvironment hostEnvironment,
        ILogger<AttestationTemplateRenderer> logger)
    {
        _configuration = configuration;
        _hostEnvironment = hostEnvironment;
        _logger = logger;
    }

    public async Task<byte[]?> TryRenderAsync(
        DocumentType type,
        StudentData data,
        CancellationToken cancellationToken = default)
    {
        if (!IsAttestationType(type))
        {
            return null;
        }

        try
        {
            var templateRoot = ResolveTemplateRootPath();
            var mappingFilePath = ResolveMappingFilePath(templateRoot);

            if (!File.Exists(mappingFilePath))
            {
                _logger.LogWarning("Template mapping file not found at {MappingFilePath}", mappingFilePath);
                return null;
            }

            await using var mappingStream = File.OpenRead(mappingFilePath);
            var templateConfiguration = await JsonSerializer.DeserializeAsync<TemplateConfiguration>(
                mappingStream,
                JsonOptions,
                cancellationToken);

            if (templateConfiguration?.Templates == null || templateConfiguration.Templates.Count == 0)
            {
                _logger.LogWarning("Template mapping file {MappingFilePath} is empty or invalid", mappingFilePath);
                return null;
            }

            var selectedTemplate = SelectTemplate(templateConfiguration.Templates, type, data);
            if (selectedTemplate == null)
            {
                _logger.LogWarning("No template rule matched for document type {DocumentType}", type);
                return null;
            }

            var backgroundImagePath = ResolveBackgroundImagePath(templateRoot, selectedTemplate.BackgroundImage);
            if (!File.Exists(backgroundImagePath))
            {
                _logger.LogWarning("Template background image not found at {BackgroundImagePath}", backgroundImagePath);
                return null;
            }

            var backgroundImageBytes = await File.ReadAllBytesAsync(backgroundImagePath, cancellationToken);
            var renderedPdf = BuildTemplatePdf(backgroundImageBytes, selectedTemplate.Fields, data);

            _logger.LogInformation(
                "Template renderer selected {TemplateKey} for {DocumentType}",
                selectedTemplate.Key,
                type);

            return renderedPdf;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Template rendering failed for {DocumentType}; fallback will be used", type);
            return null;
        }
    }

    private static bool IsAttestationType(DocumentType type)
        => type is DocumentType.AttestationScolarite
            or DocumentType.AttestationReussite
            or DocumentType.AttestationInscription;

    private string ResolveTemplateRootPath()
    {
        var configuredRoot = _configuration["AttestationTemplateRenderer:TemplateRootPath"];
        if (!string.IsNullOrWhiteSpace(configuredRoot))
        {
            if (Path.IsPathRooted(configuredRoot))
            {
                return configuredRoot;
            }

            return Path.GetFullPath(Path.Combine(_hostEnvironment.ContentRootPath, configuredRoot));
        }

        var probes = new[]
        {
            Path.Combine(_hostEnvironment.ContentRootPath, "Attestations"),
            Path.Combine(_hostEnvironment.ContentRootPath, "..", "..", "..", "Attestations"),
            Path.Combine(AppContext.BaseDirectory, "Attestations"),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "Attestations")
        };

        foreach (var probe in probes)
        {
            var candidate = Path.GetFullPath(probe);
            if (Directory.Exists(candidate))
            {
                return candidate;
            }
        }

        return Path.GetFullPath(probes[1]);
    }

    private string ResolveMappingFilePath(string templateRootPath)
    {
        var configuredMappingPath = _configuration["AttestationTemplateRenderer:MappingFile"];
        if (string.IsNullOrWhiteSpace(configuredMappingPath))
        {
            return Path.Combine(templateRootPath, "template-layouts.json");
        }

        if (Path.IsPathRooted(configuredMappingPath))
        {
            return configuredMappingPath;
        }

        return Path.Combine(templateRootPath, configuredMappingPath);
    }

    private static string ResolveBackgroundImagePath(string templateRootPath, string? backgroundImage)
    {
        if (string.IsNullOrWhiteSpace(backgroundImage))
        {
            return string.Empty;
        }

        return Path.IsPathRooted(backgroundImage)
            ? backgroundImage
            : Path.Combine(templateRootPath, backgroundImage);
    }

    private static TemplateDefinition? SelectTemplate(
        IEnumerable<TemplateDefinition> templates,
        DocumentType type,
        StudentData data)
    {
        var candidates = templates
            .Where(t => string.Equals(t.DocumentType, type.ToString(), StringComparison.OrdinalIgnoreCase))
            .OrderBy(t => t.Priority)
            .ToList();

        if (candidates.Count == 0)
        {
            return null;
        }

        return candidates.FirstOrDefault(template => MatchesRules(template, data))
               ?? candidates.FirstOrDefault(template => IsDefaultRule(template))
               ?? candidates[0];
    }

    private static bool MatchesRules(TemplateDefinition template, StudentData data)
    {
        if (!MatchesContainsRule(template.ProgramContains, data.ProgramNameFr))
        {
            return false;
        }

        if (!MatchesContainsRule(template.FacultyContains, data.FacultyFr))
        {
            return false;
        }

        if (!MatchesContainsRule(template.InstitutionContains, data.FacultyFr))
        {
            return false;
        }

        return true;
    }

    private static bool IsDefaultRule(TemplateDefinition template)
    {
        return string.IsNullOrWhiteSpace(template.ProgramContains)
               && string.IsNullOrWhiteSpace(template.FacultyContains)
               && string.IsNullOrWhiteSpace(template.InstitutionContains);
    }

    private static bool MatchesContainsRule(string? configuredRule, string? source)
    {
        if (string.IsNullOrWhiteSpace(configuredRule))
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(source))
        {
            return false;
        }

        return source.Contains(configuredRule, StringComparison.OrdinalIgnoreCase);
    }

    private static byte[] BuildTemplatePdf(
        byte[] backgroundImageBytes,
        IReadOnlyCollection<TemplateFieldDefinition>? fields,
        StudentData data)
    {
        var orderedFields = (fields ?? Array.Empty<TemplateFieldDefinition>())
            .OrderBy(field => field.Y)
            .ThenBy(field => field.X)
            .ToList();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(0);

                page.Content().Layers(layers =>
                {
                    layers.Layer().Image(backgroundImageBytes).FitArea();

                    layers.PrimaryLayer().Column(column =>
                    {
                        var previousY = 0f;

                        foreach (var field in orderedFields)
                        {
                            var value = ResolveFieldValue(field.Field, data);
                            if (string.IsNullOrWhiteSpace(value))
                            {
                                continue;
                            }

                            var renderedText = string.IsNullOrWhiteSpace(field.Prefix)
                                ? value
                                : $"{field.Prefix}{value}";

                            var deltaY = Math.Max(0, field.Y - previousY);
                            previousY = field.Y;

                            var fieldContainer = column.Item()
                                .PaddingTop(deltaY)
                                .PaddingLeft(Math.Max(0, field.X));

                            if (field.Bold)
                            {
                                fieldContainer
                                    .Text(renderedText)
                                    .FontSize(field.FontSize > 0 ? field.FontSize : 11)
                                    .Bold();
                            }
                            else
                            {
                                fieldContainer
                                    .Text(renderedText)
                                    .FontSize(field.FontSize > 0 ? field.FontSize : 11);
                            }
                        }
                    });
                });
            });
        });

        return document.GeneratePdf();
    }

    private static string ResolveFieldValue(string fieldName, StudentData data)
    {
        return fieldName switch
        {
            "FullNameFr" => $"{data.FirstNameFr} {data.LastNameFr}".Trim(),
            "FullNameAr" => $"{data.FirstNameAr} {data.LastNameAr}".Trim(),
            "FirstNameFr" => data.FirstNameFr,
            "LastNameFr" => data.LastNameFr,
            "FirstNameAr" => data.FirstNameAr,
            "LastNameAr" => data.LastNameAr,
            "CNE" => data.CNE,
            "CIN" => data.CIN,
            "ProgramNameFr" => data.ProgramNameFr,
            "ProgramNameAr" => data.ProgramNameAr,
            "FacultyFr" => data.FacultyFr,
            "FacultyAr" => data.FacultyAr,
            "AcademicYear" => data.AcademicYear,
            "DegreeNameFr" => data.DegreeNameFr,
            "DegreeNameAr" => data.DegreeNameAr,
            "Mention" => data.Mention,
            "GraduationYear" => data.GraduationYear > 0 ? data.GraduationYear.ToString(CultureInfo.InvariantCulture) : string.Empty,
            "EnrollmentStatus" => data.EnrollmentStatus,
            "EnrollmentDate" => data.EnrollmentDate == default
                ? string.Empty
                : data.EnrollmentDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
            "DateOfBirth" => data.DateOfBirth == default
                ? string.Empty
                : data.DateOfBirth.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
            "DocumentId" => data.DocumentId.ToString(),
            _ => string.Empty
        };
    }

    private sealed class TemplateConfiguration
    {
        public List<TemplateDefinition> Templates { get; set; } = new();
    }

    private sealed class TemplateDefinition
    {
        public string Key { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public int Priority { get; set; }
        public string? ProgramContains { get; set; }
        public string? FacultyContains { get; set; }
        public string? InstitutionContains { get; set; }
        public string? BackgroundImage { get; set; }
        public List<TemplateFieldDefinition> Fields { get; set; } = new();
    }

    private sealed class TemplateFieldDefinition
    {
        public string Field { get; set; } = string.Empty;
        public float X { get; set; }
        public float Y { get; set; }
        public float FontSize { get; set; } = 11;
        public bool Bold { get; set; }
        public string? Prefix { get; set; }
    }
}
