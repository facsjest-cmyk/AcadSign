using AcadSign.Backend.Application.Common.Interfaces;
using AcadSign.Backend.Application.Common.Models;
using AcadSign.Backend.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcadSign.Backend.Web.Endpoints;

public class Templates : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder group)
    {
        group.RequireAuthorization();
        group.MapPost(UploadTemplate, "");
        group.MapGet(ListTemplates, "");
        group.MapGet(GetTemplate, "{templateId}");
        group.MapDelete(DeleteTemplate, "{templateId}");
    }

    [Authorize(Roles = "Admin")]
    public async Task<IResult> UploadTemplate(
        [FromForm] IFormFile templateFile,
        [FromForm] string documentType,
        [FromForm] string institutionId,
        [FromForm] string? description,
        [FromServices] ITemplateRepository templateRepo,
        [FromServices] ILogger<Templates> logger)
    {
        logger.LogInformation("Uploading template for {DocumentType} at {InstitutionId}", 
            documentType, institutionId);

        // Valider le fichier
        if (templateFile == null || templateFile.Length == 0)
        {
            return Results.BadRequest(new { error = "Template file is required" });
        }

        if (!templateFile.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
        {
            return Results.BadRequest(new { error = "Only PDF files are supported" });
        }

        // Valider le type de document
        if (!Enum.TryParse<DocumentType>(documentType, out var docType))
        {
            return Results.BadRequest(new { error = "Invalid document type" });
        }

        // Lire le fichier
        byte[] templateData;
        using (var memoryStream = new MemoryStream())
        {
            await templateFile.CopyToAsync(memoryStream);
            templateData = memoryStream.ToArray();
        }

        // Calculer la nouvelle version
        var existingTemplates = await templateRepo.GetByInstitutionAndTypeAsync(institutionId, docType);
        var newVersion = CalculateNextVersion(existingTemplates);

        // Désactiver les anciens templates
        foreach (var oldTemplate in existingTemplates.Where(t => t.IsActive))
        {
            oldTemplate.IsActive = false;
            await templateRepo.UpdateAsync(oldTemplate);
        }

        // Créer le nouveau template
        var template = new DocumentTemplate
        {
            Id = Guid.NewGuid(),
            Type = docType,
            InstitutionId = institutionId,
            Version = newVersion,
            TemplateData = templateData,
            FileName = templateFile.FileName,
            FileSize = templateData.Length,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system", // TODO: Get from User claims
            IsActive = true,
            Description = description
        };

        await templateRepo.AddAsync(template);
        await templateRepo.SaveChangesAsync();

        logger.LogInformation(
            "Template uploaded: {TemplateId}, Type: {Type}, Institution: {Institution}, Version: {Version}",
            template.Id, template.Type, template.InstitutionId, template.Version);

        return Results.Ok(new UploadTemplateResponse
        {
            TemplateId = template.Id,
            DocumentType = template.Type.ToString(),
            Version = template.Version,
            CreatedAt = template.CreatedAt
        });
    }

    [Authorize(Roles = "Admin,Registrar")]
    public async Task<IResult> ListTemplates(
        [FromQuery] string? institutionId,
        [FromServices] ITemplateRepository templateRepo,
        [FromServices] ILogger<Templates> logger)
    {
        if (string.IsNullOrEmpty(institutionId))
        {
            return Results.BadRequest(new { error = "institutionId is required" });
        }

        logger.LogInformation("Listing templates for institution {InstitutionId}", institutionId);

        var templates = await templateRepo.GetByInstitutionAsync(institutionId);

        var response = new ListTemplatesResponse
        {
            Templates = templates.Select(t => new TemplateDto
            {
                TemplateId = t.Id,
                DocumentType = t.Type.ToString(),
                InstitutionId = t.InstitutionId,
                Version = t.Version,
                IsActive = t.IsActive,
                CreatedAt = t.CreatedAt,
                FileName = t.FileName,
                FileSize = t.FileSize,
                Description = t.Description
            }).ToList()
        };

        return Results.Ok(response);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IResult> GetTemplate(
        Guid templateId,
        [FromServices] ITemplateRepository templateRepo,
        [FromServices] ILogger<Templates> logger)
    {
        logger.LogInformation("Getting template {TemplateId}", templateId);

        var template = await templateRepo.GetByIdAsync(templateId);

        if (template == null)
        {
            return Results.NotFound(new { error = "Template not found" });
        }

        return Results.File(template.TemplateData, "application/pdf", template.FileName);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IResult> DeleteTemplate(
        Guid templateId,
        [FromServices] ITemplateRepository templateRepo,
        [FromServices] ILogger<Templates> logger)
    {
        logger.LogInformation("Deleting template {TemplateId}", templateId);

        var template = await templateRepo.GetByIdAsync(templateId);

        if (template == null)
        {
            return Results.NotFound(new { error = "Template not found" });
        }

        // Ne pas supprimer physiquement, juste désactiver
        template.IsActive = false;
        await templateRepo.UpdateAsync(template);
        await templateRepo.SaveChangesAsync();

        logger.LogInformation("Template {TemplateId} deactivated", templateId);

        return Results.NoContent();
    }

    private string CalculateNextVersion(IEnumerable<DocumentTemplate> existingTemplates)
    {
        if (!existingTemplates.Any())
        {
            return "1.0";
        }

        var latestVersion = existingTemplates
            .Select(t => Version.Parse(t.Version))
            .OrderByDescending(v => v)
            .First();

        // Incrémenter la version mineure
        return $"{latestVersion.Major}.{latestVersion.Minor + 1}";
    }
}

public record UploadTemplateResponse
{
    public Guid TemplateId { get; init; }
    public string DocumentType { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

public record ListTemplatesResponse
{
    public List<TemplateDto> Templates { get; init; } = new();
}

public record TemplateDto
{
    public Guid TemplateId { get; init; }
    public string DocumentType { get; init; } = string.Empty;
    public string InstitutionId { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public string FileName { get; init; } = string.Empty;
    public long FileSize { get; init; }
    public string? Description { get; init; }
}
