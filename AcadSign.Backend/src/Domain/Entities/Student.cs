using AcadSign.Backend.Domain.Attributes;

namespace AcadSign.Backend.Domain.Entities;

public class Student : BaseEntity
{
    public Guid PublicId { get; set; }

    // Données chiffrées (PII - Personally Identifiable Information)
    [EncryptedProperty]
    public string CIN { get; set; } = string.Empty; // Carte d'Identité Nationale

    [EncryptedProperty]
    public string CNE { get; set; } = string.Empty; // Code National Étudiant

    [EncryptedProperty]
    public string Email { get; set; } = string.Empty;

    [EncryptedProperty]
    public string? PhoneNumber { get; set; }

    // Données non chiffrées
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public Guid InstitutionId { get; set; }
}
