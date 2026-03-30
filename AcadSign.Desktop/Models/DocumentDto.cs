using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AcadSign.Desktop.Models;

public class DocumentDto : ObservableObject
{
    private bool _isSelected;
    private string _status = string.Empty;
    private string? _unsignedPreviewPath;
    private string? _signedPreviewPath;

    public Guid Id { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public string? StudentId { get; set; }
    public string? Cin { get; set; }
    public string? Program { get; set; }
    public string? Level { get; set; }
    public string? Reference { get; set; }

    // URL source du PDF côté backend (FSJEST), utilisée pour télécharger l'aperçu à la demande
    public string? SourcePdfUrl { get; set; }

    public string? UnsignedPreviewPath
    {
        get => _unsignedPreviewPath;
        set => SetProperty(ref _unsignedPreviewPath, value);
    }

    public string? SignedPreviewPath
    {
        get => _signedPreviewPath;
        set => SetProperty(ref _signedPreviewPath, value);
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}
