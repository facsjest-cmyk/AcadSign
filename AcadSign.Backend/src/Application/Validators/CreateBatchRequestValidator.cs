using FluentValidation;
using AcadSign.Backend.Application.Models;

namespace AcadSign.Backend.Application.Validators;

public class CreateBatchRequestValidator : AbstractValidator<CreateBatchRequest>
{
    public CreateBatchRequestValidator()
    {
        RuleFor(x => x.Documents)
            .NotEmpty().WithMessage("La liste de documents est requise")
            .Must(x => x != null && x.Count > 0).WithMessage("La liste de documents ne peut pas être vide")
            .Must(x => x == null || x.Count <= 500).WithMessage("Maximum 500 documents par batch");
        
        RuleForEach(x => x.Documents).SetValidator(new DocumentGenerationRequestValidator());
    }
}

public class DocumentGenerationRequestValidator : AbstractValidator<DocumentGenerationRequest>
{
    public DocumentGenerationRequestValidator()
    {
        RuleFor(x => x.StudentId)
            .NotEmpty().WithMessage("StudentId est requis")
            .MaximumLength(50).WithMessage("StudentId ne peut pas dépasser 50 caractères");
        
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Prénom est requis")
            .MaximumLength(100).WithMessage("Prénom ne peut pas dépasser 100 caractères");
        
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Nom est requis")
            .MaximumLength(100).WithMessage("Nom ne peut pas dépasser 100 caractères");
        
        RuleFor(x => x.CIN)
            .NotEmpty().WithMessage("CIN est requis")
            .Matches(@"^[A-Z]{1,2}[0-9]{6}$")
            .WithMessage("CIN format invalide. Format attendu: A123456 ou AB123456");
        
        RuleFor(x => x.CNE)
            .NotEmpty().WithMessage("CNE est requis")
            .Matches(@"^[A-Z0-9]{10}$")
            .WithMessage("CNE format invalide. Format attendu: 10 caractères alphanumériques");
        
        RuleFor(x => x.DocumentType)
            .IsInEnum().WithMessage("Type de document invalide");
        
        RuleFor(x => x.AcademicYear)
            .NotEmpty().WithMessage("Année académique est requise")
            .Matches(@"^[0-9]{4}-[0-9]{4}$")
            .WithMessage("Année académique invalide. Format attendu: 2025-2026")
            .Must(BeValidAcademicYear)
            .WithMessage("Année académique invalide. L'année de fin doit être l'année de début + 1");
    }
    
    private bool BeValidAcademicYear(string academicYear)
    {
        if (string.IsNullOrEmpty(academicYear)) return false;
        
        var parts = academicYear.Split('-');
        if (parts.Length != 2) return false;
        
        if (!int.TryParse(parts[0], out var startYear)) return false;
        if (!int.TryParse(parts[1], out var endYear)) return false;
        
        return endYear == startYear + 1;
    }
}
