namespace AcadSign.Backend.Application.Common.Models;

public class StudentData
{
    public string FirstNameAr { get; set; } = string.Empty;
    public string LastNameAr { get; set; } = string.Empty;
    public string FirstNameFr { get; set; } = string.Empty;
    public string LastNameFr { get; set; } = string.Empty;
    public string CIN { get; set; } = string.Empty;
    public string CNE { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string ProgramNameAr { get; set; } = string.Empty;
    public string ProgramNameFr { get; set; } = string.Empty;
    public string FacultyAr { get; set; } = string.Empty;
    public string FacultyFr { get; set; } = string.Empty;
    public string AcademicYear { get; set; } = string.Empty;
    public Guid DocumentId { get; set; }

    // Pour Relevé de Notes
    public List<Grade> Grades { get; set; } = new();
    public decimal GPA { get; set; }
    public string Mention { get; set; } = string.Empty;

    // Pour Attestation de Réussite
    public int GraduationYear { get; set; }
    public string DegreeNameAr { get; set; } = string.Empty;
    public string DegreeNameFr { get; set; } = string.Empty;

    // Pour Attestation d'Inscription
    public DateTime EnrollmentDate { get; set; }
    public string EnrollmentStatus { get; set; } = string.Empty;
}
