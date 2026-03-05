namespace AcadSign.Backend.Application.Common.Models;

public class Grade
{
    public string SubjectNameAr { get; set; } = string.Empty;
    public string SubjectNameFr { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public int Credits { get; set; }
}
