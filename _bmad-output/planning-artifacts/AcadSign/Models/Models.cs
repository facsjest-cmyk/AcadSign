using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AcadSign.Models;

// ─── Enumerations ─────────────────────────────────────────────────────────────

public enum DocumentType
{
    AttestationScolarite,
    ReleveNotes,
    AttestationReussite,
    AttestationInscription,
    Other
}

public enum DocumentStatus
{
    Pending,
    Generating,
    Signing,
    Signed,
    EmailSent,
    Error
}

// ─── Student ──────────────────────────────────────────────────────────────────

public class Student
{
    public string Id           { get; set; } = string.Empty;   // N° Apogée
    public string FullName     { get; set; } = string.Empty;
    public string FullNameAr   { get; set; } = string.Empty;
    public string Cin          { get; set; } = string.Empty;
    public string Email        { get; set; } = string.Empty;
    public string Program      { get; set; } = string.Empty;
    public string Faculty      get; set; } = string.Empty;
    public string Level        { get; set; } = string.Empty;
    public string AcademicYear { get; set; } = "2024/2025";
    public DateTime BirthDate  { get; set; }
    public List<Grade> Grades  { get; set; } = new();
    public double Gpa          { get; set; }
    public string Mention      { get; set; } = string.Empty;
}

public class Grade
{
    public string Subject  { get; set; } = string.Empty;
    public double Score    { get; set; }
    public double Credits  { get; set; }
    public string Semester { get; set; } = string.Empty;
}

// ─── Document Request ─────────────────────────────────────────────────────────

public class DocumentRequest : ObservableObject
{
    private DocumentStatus _status = DocumentStatus.Pending;
    private string         _errorMessage = string.Empty;
    private int            _progress;
    private string?        _signedPdfPath;
    private string?        _unsignedPdfPath;
    private string?        _s3Url;
    private bool           _isSelected;

    public string          Id            { get; set; } = Guid.NewGuid().ToString("N")[..8].ToUpper();
    public string          Reference     { get; set; } = string.Empty;
    public Student         Student       { get; set; } = new();
    public DocumentType    DocumentType  { get; set; }
    public DateTime        RequestedAt   { get; set; } = DateTime.Now;
    public DateTime?       SignedAt      { get; set; }
    public string?         CertificateSerial { get; set; }
    public string?         SignatureHash     { get; set; }
    public string?         TimestampToken    { get; set; }

    public DocumentStatus Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public int Progress
    {
        get => _progress;
        set => SetProperty(ref _progress, value);
    }

    public string? SignedPdfPath
    {
        get => _signedPdfPath;
        set => SetProperty(ref _signedPdfPath, value);
    }

    public string? UnsignedPdfPath
    {
        get => _unsignedPdfPath;
        set => SetProperty(ref _unsignedPdfPath, value);
    }

    public string? S3Url
    {
        get => _s3Url;
        set => SetProperty(ref _s3Url, value);
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    // Computed
    public string DisplayName     => Student.FullName;
    public string DisplayType     => DocumentTypeDisplay(DocumentType);
    public string DisplayDate     => RequestedAt.ToString("dd/MM/yyyy");
    public string StatusIcon      => Status switch
    {
        DocumentStatus.Signed    => "✓",
        DocumentStatus.Error     => "⚠",
        DocumentStatus.Signing   => "⏳",
        DocumentStatus.EmailSent => "📧",
        _                        => "●"
    };
    public string StatusColor => Status switch
    {
        DocumentStatus.Signed    => "#10B981",
        DocumentStatus.Error     => "#EF4444",
        DocumentStatus.Signing   => "#F59E0B",
        DocumentStatus.EmailSent => "#6366F1",
        _                        => "#F59E0B"
    };

    private static string DocumentTypeDisplay(DocumentType t) => t switch
    {
        DocumentType.AttestationScolarite  => "Attestation de Scolarité",
        DocumentType.ReleveNotes           => "Relevé de Notes",
        DocumentType.AttestationReussite   => "Attestation de Réussite",
        DocumentType.AttestationInscription=> "Attestation d'Inscription",
        _                                  => "Document"
    };
}

// ─── API Response DTOs ────────────────────────────────────────────────────────

public class SisApiResponse
{
    public bool                    Success  { get; set; }
    public string?                 Message  { get; set; }
    public List<DocumentRequest>   Data     { get; set; } = new();
    public int                     Total    { get; set; }
}

public class SignatureResult
{
    public bool    Success           { get; set; }
    public string? Error             { get; set; }
    public string? CertificateSerial { get; set; }
    public string? DocumentHash      { get; set; }
    public string? TimestampToken    { get; set; }
    public byte[]? SignedPdfBytes    { get; set; }
    public DateTime SignedAt         { get; set; }
}

public class S3UploadResult
{
    public bool    Success  { get; set; }
    public string? Url      { get; set; }
    public string? Key      { get; set; }
    public string? Error    { get; set; }
}

// ─── Settings ─────────────────────────────────────────────────────────────────

public class AppSettings
{
    public ESignSettings  ESign   { get; set; } = new();
    public S3Settings     S3      { get; set; } = new();
    public EmailSettings  Email   { get; set; } = new();
    public SisApiSettings SisApi  { get; set; } = new();
}

public class ESignSettings
{
    public string BaseUrl           { get; set; } = "https://esign.barid.ma/api/v1";
    public string CertificatePath   { get; set; } = string.Empty;
    public string CertificateSerial { get; set; } = string.Empty;
    public string ApiKey            { get; set; } = string.Empty;
    public int    TimeoutSeconds    { get; set; } = 30;
}

public class S3Settings
{
    public string Endpoint        { get; set; } = "https://s3.uh2.ac.ma";
    public string AccessKey       { get; set; } = string.Empty;
    public string SecretKey       { get; set; } = string.Empty;
    public string BucketName      { get; set; } = "uh2-docs-signed";
    public string Region          { get; set; } = "us-east-1";
    public bool   UsePathStyle    { get; set; } = true;
}

public class EmailSettings
{
    public string SmtpHost        { get; set; } = "smtp.uh2.ac.ma";
    public int    SmtpPort        { get; set; } = 587;
    public string Username        { get; set; } = string.Empty;
    public string Password        { get; set; } = string.Empty;
    public string FromAddress     { get; set; } = "scolarite@uh2.ac.ma";
    public string FromName        { get; set; } = "Service de Scolarité UH2";
    public bool   UseSsl          { get; set; } = true;
}

public class SisApiSettings
{
    public string BaseUrl         { get; set; } = "https://sis.uh2.ac.ma/api/";
    public string ApiKey          { get; set; } = string.Empty;
    public string InstitutionCode { get; set; } = "UH2";
}
