using System;
using System.Collections.Generic;

namespace AcadSign.Desktop.Models;

public class AttestationsExportResponse
{
    public List<AttestationItem> Data { get; set; } = new();
    public AttestationsExportMeta Meta { get; set; } = new();
}

public class AttestationItem
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string PdfUrl { get; set; } = string.Empty;
    public DateTime? ProcessedAt { get; set; }
}

public class AttestationsExportMeta
{
    public int TotalPages { get; set; }
}

public class DownloadedAttestationPdf
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string LocalPath { get; set; } = string.Empty;
    public DateTime? ProcessedAt { get; set; }
}
