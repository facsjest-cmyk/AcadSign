using AcadSign.Backend.Domain.Entities;

namespace AcadSign.Backend.Application.Common.Models;

public sealed class PendingDocumentDto
{
    public Guid Id { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public string DocumentType { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? StudentId { get; init; }
    public string? Cin { get; init; }
    public string? Program { get; init; }
    public string? Level { get; init; }
    public string? Reference { get; init; }
}

public static class PendingDocumentDtoMapper
{
    public static PendingDocumentDto Map(Document document, Student? student)
    {
        ArgumentNullException.ThrowIfNull(document);

        return new PendingDocumentDto
        {
            Id = document.PublicId,
            StudentName = BuildStudentName(student),
            DocumentType = document.DocumentType,
            CreatedAt = document.Created.UtcDateTime,
            Status = document.Status,
            StudentId = student?.CNE ?? document.StudentId.ToString(),
            Cin = student?.CIN,
            Program = null,
            Level = null,
            Reference = null
        };
    }

    private static string BuildStudentName(Student? student)
    {
        if (student is null)
        {
            return string.Empty;
        }

        return $"{student.FirstName} {student.LastName}".Trim();
    }
}
