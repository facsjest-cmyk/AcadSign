using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AcadSign.Desktop.Services.Api;
using AcadSign.Desktop.Services.Authentication;
using AcadSign.Desktop.Services.Batch;
using AcadSign.Desktop.Services.Navigation;
using AcadSign.Desktop.Services.Storage;
using AcadSign.Desktop.Models;
using AcadSign.Desktop.Views;
using System.Collections.ObjectModel;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Collections.Specialized;
using System.ComponentModel;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Net.Http.Headers;

namespace AcadSign.Desktop.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IApiClientService _apiClient;
    private readonly IAuthenticationService _authenticationService;
    private readonly ITokenStorageService _tokenStorageService;
    private readonly INavigationService _navigationService;
    private readonly IBatchSigningService _batchSigningService;

    private bool _suppressDocumentNotifications;

    public DongleStatusViewModel DongleStatus { get; }

    public IAsyncRelayCommand LoadDocumentsUiCommand { get; }
    
    [ObservableProperty]
    private ObservableCollection<DocumentDto> _documents = new();

    [ObservableProperty]
    private ObservableCollection<DocumentDto> _filteredDocuments = new();
    
    [ObservableProperty]
    private DocumentDto? _selectedDocument;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _filterStatus = "all";
    
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private string _statusText = "Prêt";
    
    [ObservableProperty]
    private bool _isBatchSigning;
    
    [ObservableProperty]
    private double _batchProgressPercent;
    
    [ObservableProperty]
    private string _batchProgressStatus = string.Empty;

    [ObservableProperty]
    private Uri? _previewUri;

    [ObservableProperty]
    private bool _isPreviewLoading;

    [ObservableProperty]
    private string _previewStatusText = string.Empty;

    [ObservableProperty]
    private bool _isSignedPreview;

    // Signing (UI PIN box)
    [ObservableProperty]
    private string _pin = string.Empty;

    // Clock
    [ObservableProperty]
    private string _currentTime = DateTime.Now.ToString("HH:mm:ss");

    [ObservableProperty]
    private string _apiStatusText = "Connecté";

    [ObservableProperty]
    private string _s3StatusText = "En ligne";

    [ObservableProperty]
    private string _sisStatusText = "Prêt";

    [ObservableProperty]
    private DateTime? _fromDateFilter;

    [ObservableProperty]
    private DateTime? _toDateFilter;

    [ObservableProperty]
    private int _currentPreviewPage = 1;

    [ObservableProperty]
    private int _previewPageCount = 1;

    [ObservableProperty]
    private int _previewZoomPercent = 100;

    private CancellationTokenSource? _previewCts;

    public int PendingCount => Documents.Count(d => IsPendingStatus(d.Status));
    public int SignedCount => Documents.Count(d => IsSignedStatus(d.Status));
    public int SelectedCount => Documents.Count(d => d.IsSelected);
    public int TotalCount => Documents.Count;
    public bool HasSelectedItems => SelectedCount > 0;
    public string BatchSigningButtonText => $"⚡ Signature{Environment.NewLine}par lot ({SelectedCount})";
    public string PreviewPageDisplay => $"{CurrentPreviewPage} / {PreviewPageCount}";
    public string PreviewZoomDisplay => $"{PreviewZoomPercent}%";

    public bool CanSign => SelectedDocument != null
                          && IsPendingStatus(SelectedDocument.Status)
                          && Pin.Length >= 4
                          && !IsLoading;
    
    public MainViewModel(
        IApiClientService apiClient,
        IAuthenticationService authenticationService,
        ITokenStorageService tokenStorageService,
        IBatchSigningService batchSigningService,
        INavigationService navigationService,
        DongleStatusViewModel dongleStatus)
    {
        _apiClient = apiClient;
        _authenticationService = authenticationService;
        _tokenStorageService = tokenStorageService;
        _batchSigningService = batchSigningService;
        _navigationService = navigationService;
        DongleStatus = dongleStatus;

        LoadDocumentsUiCommand = new AsyncRelayCommand(LoadDocumentsAsync);

        Documents.CollectionChanged += DocumentsOnCollectionChanged;

        var timer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        timer.Tick += (_, _) => CurrentTime = DateTime.Now.ToString("HH:mm:ss");
        timer.Start();
        
        _ = LoadDocumentsAsync();
    }

    partial void OnPinChanged(string value)
    {
        OnPropertyChanged(nameof(CanSign));
    }

    partial void OnCurrentPreviewPageChanged(int value)
    {
        OnPropertyChanged(nameof(PreviewPageDisplay));
    }

    partial void OnPreviewPageCountChanged(int value)
    {
        OnPropertyChanged(nameof(PreviewPageDisplay));
    }

    partial void OnPreviewZoomPercentChanged(int value)
    {
        OnPropertyChanged(nameof(PreviewZoomDisplay));
    }
    
    partial void OnFromDateFilterChanged(DateTime? value)
    {
        ApplyFilter();
    }

    partial void OnToDateFilterChanged(DateTime? value)
    {
        ApplyFilter();
    }
    
    [RelayCommand]
    private async Task LoadDocumentsAsync()
    {
        try
        {
            IsLoading = true;
            StatusText = "Chargement des documents...";
            ApiStatusText = "Chargement";
            
            var docs = await _apiClient.GetPendingDocumentsAsync();
            Documents.Clear();
            foreach (var doc in docs)
            {
                doc.IsSelected = true;
                Documents.Add(doc);
            }

            AttachDocumentHandlers();
            NotifyCounters();

            ApplyFilter();

            if (SelectedDocument == null && Documents.Count > 0)
            {
                SelectedDocument = Documents[0];
            }
            
            ApiStatusText = "Connecté";
            StatusText = $"{Documents.Count} document(s) en attente";
        }
        catch (Exception ex)
        {
            ApiStatusText = "Erreur";
            StatusText = $"Erreur: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            OnPropertyChanged(nameof(CanSign));
        }
    }

    [RelayCommand]
    private async Task GenerateAttestationsFromSisAsync()
    {
        if (IsLoading)
        {
            return;
        }

        string? generationSummary = null;
        string? failureSummary = null;

        try
        {
            IsLoading = true;
            StatusText = "Génération des attestations en cours...";
            SisStatusText = "Actif";

            var result = await _apiClient.GenerateAttestationsFromSisAsync();

            generationSummary = $"Génération terminée: total={result.Total}, générées={result.Generated}, échecs={result.Failed}";

            if (result.Failures.Count > 0)
            {
                failureSummary = string.Join(" | ", result.Failures
                    .Take(3)
                    .Select(f => $"{(string.IsNullOrWhiteSpace(f.Apogee) ? "N/A" : f.Apogee)}:{f.Code}"));

                if (result.Failures.Count > 3)
                {
                    failureSummary += $" (+{result.Failures.Count - 3} autres)";
                }

                StatusText = $"{generationSummary} — {failureSummary}";
            }
            else
            {
                StatusText = generationSummary;
            }
        }
        catch (Exception ex)
        {
            SisStatusText = "Erreur";
            StatusText = $"Erreur génération: {ex.Message}";
            return;
        }
        finally
        {
            IsLoading = false;
            OnPropertyChanged(nameof(CanSign));
        }

        SisStatusText = "Prêt";

        await LoadDocumentsUiCommand.ExecuteAsync(null);

        if (!string.IsNullOrWhiteSpace(generationSummary)
            && !StatusText.StartsWith("Erreur", StringComparison.OrdinalIgnoreCase))
        {
            StatusText = string.IsNullOrWhiteSpace(failureSummary)
                ? $"{generationSummary} — liste rafraîchie"
                : $"{generationSummary} — {failureSummary} — liste rafraîchie";
        }
    }

    private void DocumentsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        AttachDocumentHandlers(e);
        NotifyCounters();
    }

    private void AttachDocumentHandlers(NotifyCollectionChangedEventArgs? e = null)
    {
        if (e?.OldItems != null)
        {
            foreach (var item in e.OldItems)
            {
                if (item is DocumentDto d)
                    d.PropertyChanged -= DocumentOnPropertyChanged;
            }
        }

        if (e?.NewItems != null)
        {
            foreach (var item in e.NewItems)
            {
                if (item is DocumentDto d)
                    d.PropertyChanged += DocumentOnPropertyChanged;
            }
            return;
        }

        foreach (var d in Documents)
        {
            d.PropertyChanged -= DocumentOnPropertyChanged;
            d.PropertyChanged += DocumentOnPropertyChanged;
        }
    }

    private void DocumentOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(DocumentDto.IsSelected) or nameof(DocumentDto.Status))
        {
            if (_suppressDocumentNotifications)
                return;

            NotifyCounters();
            ApplyFilter();
            OnPropertyChanged(nameof(CanSign));
        }
    }

    private void NotifyCounters()
    {
        OnPropertyChanged(nameof(PendingCount));
        OnPropertyChanged(nameof(SignedCount));
        OnPropertyChanged(nameof(SelectedCount));
        OnPropertyChanged(nameof(TotalCount));
        OnPropertyChanged(nameof(HasSelectedItems));
        OnPropertyChanged(nameof(BatchSigningButtonText));
    }

    private static bool IsPendingStatus(string? status)
        => string.Equals(status, "UNSIGNED", StringComparison.OrdinalIgnoreCase)
           || string.Equals(status, "PENDING", StringComparison.OrdinalIgnoreCase);

    private static bool IsSignedStatus(string? status)
        => string.Equals(status, "SIGNED", StringComparison.OrdinalIgnoreCase)
           || string.Equals(status, "EMAIL_SENT", StringComparison.OrdinalIgnoreCase);
    
    [RelayCommand]
    private void SignDocument(DocumentDto? document)
    {
        if (document == null)
        {
            StatusText = "Sélectionnez un document";
            return;
        }

        _navigationService.NavigateTo<SigningViewModel>(document);
    }
    
    [RelayCommand]
    private void NavigateToSettings()
    {
        _navigationService.NavigateTo<SettingsViewModel>();
    }
    
    [RelayCommand]
    private async Task LogoutAsync()
    {
        try
        {
            await _authenticationService.LogoutAsync();
        }
        catch
        {
        }

        try
        {
            await _tokenStorageService.DeleteTokensAsync();
        }
        catch
        {
        }

        Application.Current.Dispatcher.Invoke(() =>
        {
            Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            Application.Current.MainWindow?.Close();
            var app = (App)Application.Current;
            app.ShowLoginWindow();
        });
    }
    
    [RelayCommand]
    private async Task StartBatchSigningAsync()
    {
        var docs = Documents.Where(d => d.IsSelected).ToList();
        if (docs.Count == 0)
        {
            StatusText = "Aucun document sélectionné";
            return;
        }

        _navigationService.NavigateTo<BatchSigningViewModel>(docs);
        await Task.CompletedTask;
    }

    private Task<string?> GetPinFromUserAsync()
    {
        var tcs = new TaskCompletionSource<string?>();

        Application.Current.Dispatcher.Invoke(() =>
        {
            var dialog = new PinDialog
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            var ok = dialog.ShowDialog();
            if (ok == true)
            {
                tcs.SetResult(dialog.Pin);
            }
            else
            {
                tcs.SetResult(null);
            }
        });

        return tcs.Task;
    }

    partial void OnSelectedDocumentChanged(DocumentDto? value)
    {
        IsSignedPreview = false;
        ResetPreviewNavigation();
        _ = LoadPreviewAsync(value, signed: false);
    }

    private async Task LoadPreviewAsync(DocumentDto? document, bool signed, bool forceReload = false)
    {
        try
        {
            _previewCts?.Cancel();
            _previewCts = new CancellationTokenSource();
            var token = _previewCts.Token;

            if (document == null)
            {
                ResetPreviewNavigation();
                PreviewUri = null;
                PreviewStatusText = string.Empty;
                return;
            }

            IsPreviewLoading = true;
            PreviewStatusText = "Téléchargement du PDF...";

            var existingPath = GetPreviewPath(document, signed);
            if (!forceReload
                && !string.IsNullOrWhiteSpace(existingPath)
                && File.Exists(existingPath))
            {
                UpdatePreviewMetadata(File.ReadAllBytes(existingPath));
                ApplyPreviewUri(existingPath);
                PreviewStatusText = string.Empty;
                S3StatusText = "En ligne";
                return;
            }

            byte[] bytes;

            // Pour les documents FSJES, on dispose d'une URL directe vers le PDF (SourcePdfUrl)
            if (!string.IsNullOrWhiteSpace(document.SourcePdfUrl))
            {
                using var httpClient = new HttpClient();

                try
                {
                    // Récupérer le token JWT stocké et l'envoyer en Authorization: Bearer
                    var (accessToken, _) = await _tokenStorageService.GetTokensAsync();
                    if (!string.IsNullOrWhiteSpace(accessToken))
                    {
                        httpClient.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", accessToken);
                    }
                }
                catch
                {
                    // En cas de problème de récupération du token, on tente quand même sans header
                }

                bytes = await httpClient.GetByteArrayAsync(document.SourcePdfUrl);
            }
            else
            {
                // Fallback pour les anciens documents qui utilisent encore l'API historique
                bytes = await _apiClient.DownloadDocumentAsync(document.Id);
            }
            token.ThrowIfCancellationRequested();

            var folder = Path.Combine(Path.GetTempPath(), "AcadSign", "previews");
            Directory.CreateDirectory(folder);
            var suffix = signed ? "signed" : "unsigned";
            var filePath = Path.Combine(folder, $"{document.Id}_{suffix}.pdf");
            await File.WriteAllBytesAsync(filePath, bytes, token);

            if (signed)
                document.SignedPreviewPath = filePath;
            else
                document.UnsignedPreviewPath = filePath;

            UpdatePreviewMetadata(bytes);
            ApplyPreviewUri(filePath);
            PreviewStatusText = string.Empty;
            S3StatusText = "En ligne";
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            PreviewUri = null;
            PreviewStatusText = $"Erreur prévisualisation: {ex.Message}";
            S3StatusText = "Erreur";
        }
        finally
        {
            IsPreviewLoading = false;
        }
    }

    [RelayCommand]
    private async Task CancelBatchAsync()
    {
        try
        {
            await _batchSigningService.CancelBatchAsync();
            StatusText = "Annulation du batch demandée";
        }
        catch (Exception ex)
        {
            StatusText = $"Erreur annulation batch: {ex.Message}";
        }
    }

    [RelayCommand]
    private void OpenBeforeSignature()
    {
        if (SelectedDocument == null)
        {
            StatusText = "Sélectionnez un document";
            return;
        }

        IsSignedPreview = false;
        StatusText = "Aperçu: avant signature";
        if (!string.IsNullOrWhiteSpace(SelectedDocument.UnsignedPreviewPath)
            && File.Exists(SelectedDocument.UnsignedPreviewPath))
        {
            UpdatePreviewMetadata(File.ReadAllBytes(SelectedDocument.UnsignedPreviewPath));
            ApplyPreviewUri(SelectedDocument.UnsignedPreviewPath);
            return;
        }

        _ = LoadPreviewAsync(SelectedDocument, signed: false);
    }

    [RelayCommand]
    private void OpenAfterSignature()
    {
        if (SelectedDocument == null)
        {
            StatusText = "Sélectionnez un document";
            return;
        }

        IsSignedPreview = true;
        StatusText = "Aperçu: après signature";
        if (!string.IsNullOrWhiteSpace(SelectedDocument.SignedPreviewPath)
            && File.Exists(SelectedDocument.SignedPreviewPath))
        {
            UpdatePreviewMetadata(File.ReadAllBytes(SelectedDocument.SignedPreviewPath));
            ApplyPreviewUri(SelectedDocument.SignedPreviewPath);
            return;
        }

        _ = LoadPreviewAsync(SelectedDocument, signed: true);
    }

    [RelayCommand]
    private void PreviousPreviewPage()
    {
        if (SelectedDocument == null)
        {
            StatusText = "Sélectionnez un document";
            return;
        }

        if (CurrentPreviewPage <= 1)
        {
            return;
        }

        CurrentPreviewPage -= 1;
        var previewPath = GetPreviewPath(SelectedDocument, IsSignedPreview);
        if (!string.IsNullOrWhiteSpace(previewPath) && File.Exists(previewPath))
        {
            ApplyPreviewUri(previewPath);
        }
    }

    [RelayCommand]
    private void NextPreviewPage()
    {
        if (SelectedDocument == null)
        {
            StatusText = "Sélectionnez un document";
            return;
        }

        if (CurrentPreviewPage >= PreviewPageCount)
        {
            return;
        }

        CurrentPreviewPage += 1;
        var previewPath = GetPreviewPath(SelectedDocument, IsSignedPreview);
        if (!string.IsNullOrWhiteSpace(previewPath) && File.Exists(previewPath))
        {
            ApplyPreviewUri(previewPath);
        }
    }

    [RelayCommand]
    private void ZoomOutPreview()
    {
        if (SelectedDocument == null)
        {
            StatusText = "Sélectionnez un document";
            return;
        }

        PreviewZoomPercent = Math.Max(50, PreviewZoomPercent - 10);
        var previewPath = GetPreviewPath(SelectedDocument, IsSignedPreview);
        if (!string.IsNullOrWhiteSpace(previewPath) && File.Exists(previewPath))
        {
            ApplyPreviewUri(previewPath);
        }
    }

    [RelayCommand]
    private void ZoomInPreview()
    {
        if (SelectedDocument == null)
        {
            StatusText = "Sélectionnez un document";
            return;
        }

        PreviewZoomPercent = Math.Min(250, PreviewZoomPercent + 10);
        var previewPath = GetPreviewPath(SelectedDocument, IsSignedPreview);
        if (!string.IsNullOrWhiteSpace(previewPath) && File.Exists(previewPath))
        {
            ApplyPreviewUri(previewPath);
        }
    }

    [RelayCommand]
    private async Task RefreshPreviewAsync()
    {
        if (SelectedDocument == null)
        {
            StatusText = "Sélectionnez un document";
            return;
        }

        StatusText = "Actualisation de l'aperçu...";
        await LoadPreviewAsync(SelectedDocument, IsSignedPreview, forceReload: true);
        if (PreviewUri != null)
        {
            StatusText = "Aperçu actualisé";
        }
    }

    [RelayCommand]
    private async Task DownloadPreviewAsync()
    {
        if (SelectedDocument == null)
        {
            StatusText = "Sélectionnez un document";
            return;
        }

        var previewPath = GetPreviewPath(SelectedDocument, IsSignedPreview);
        if (string.IsNullOrWhiteSpace(previewPath) || !File.Exists(previewPath))
        {
            await LoadPreviewAsync(SelectedDocument, IsSignedPreview);
            previewPath = GetPreviewPath(SelectedDocument, IsSignedPreview);
        }

        if (string.IsNullOrWhiteSpace(previewPath) || !File.Exists(previewPath))
        {
            StatusText = "Aucun fichier disponible à télécharger";
            return;
        }

        var dialog = new SaveFileDialog
        {
            Filter = "PDF (*.pdf)|*.pdf",
            FileName = BuildSafeAttestationFileName(SelectedDocument, new HashSet<string>(StringComparer.OrdinalIgnoreCase)),
            AddExtension = true,
            DefaultExt = ".pdf"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        File.Copy(previewPath, dialog.FileName, overwrite: true);
        StatusText = $"Fichier enregistré: {dialog.FileName}";
    }

    [RelayCommand]
    private async Task PrintPreviewAsync()
    {
        if (SelectedDocument == null)
        {
            StatusText = "Sélectionnez un document";
            return;
        }

        var previewPath = GetPreviewPath(SelectedDocument, IsSignedPreview);
        if (string.IsNullOrWhiteSpace(previewPath) || !File.Exists(previewPath))
        {
            await LoadPreviewAsync(SelectedDocument, IsSignedPreview);
            previewPath = GetPreviewPath(SelectedDocument, IsSignedPreview);
        }

        if (string.IsNullOrWhiteSpace(previewPath) || !File.Exists(previewPath))
        {
            StatusText = "Aucun fichier disponible à imprimer";
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = previewPath,
            UseShellExecute = true,
            Verb = "print"
        });

        StatusText = "Impression lancée";
    }

    [RelayCommand]
    private async Task SendByEmailAsync()
    {
        if (SelectedDocument == null)
        {
            StatusText = "Sélectionnez un document";
            return;
        }

        try
        {
            StatusText = "Mise en file d'attente de l'email...";
            await _apiClient.ResendEmailAsync(SelectedDocument.Id);
            StatusText = "Email mis en file d'attente";
        }
        catch (Exception ex)
        {
            StatusText = $"Erreur email: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ExportGeneratedAttestationsAsync()
    {
        if (IsLoading)
        {
            return;
        }

        var selectedDocuments = Documents
            .Where(d => d.IsSelected)
            .ToList();

        var sourceDocuments = selectedDocuments.Count > 0
            ? selectedDocuments
            : FilteredDocuments.ToList();

        var exportCandidates = sourceDocuments
            .Where(d => IsPendingStatus(d.Status) || IsSignedStatus(d.Status))
            .ToList();

        if (exportCandidates.Count == 0)
        {
            StatusText = "Aucun document éligible à exporter";
            return;
        }

        var exportFolder = Path.Combine(
            AppContext.BaseDirectory,
            "Generated_Attestations",
            DateTime.Now.ToString("yyyy-MM-dd"));

        Directory.CreateDirectory(exportFolder);

        var exportedCount = 0;
        var failedCount = 0;
        var failures = new List<string>();
        var usedFileNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        try
        {
            IsLoading = true;
            StatusText = $"Export en cours ({exportCandidates.Count} document(s))...";

            foreach (var document in exportCandidates)
            {
                try
                {
                    var pdfBytes = await _apiClient.DownloadDocumentAsync(document.Id);
                    var fileName = BuildSafeAttestationFileName(document, usedFileNames);
                    var filePath = Path.Combine(exportFolder, fileName);
                    await File.WriteAllBytesAsync(filePath, pdfBytes);
                    exportedCount += 1;
                }
                catch (Exception ex)
                {
                    failedCount += 1;
                    var apogee = string.IsNullOrWhiteSpace(document.StudentId) ? "N/A" : document.StudentId;
                    failures.Add($"{apogee}:{ex.Message}");
                }
            }
        }
        finally
        {
            IsLoading = false;
            OnPropertyChanged(nameof(CanSign));
        }

        var summary = $"Export terminé: {exportedCount} exporté(s), {failedCount} échec(s)";

        if (failures.Count > 0)
        {
            var failureSummary = string.Join(" | ", failures.Take(2));
            if (failures.Count > 2)
            {
                failureSummary += $" (+{failures.Count - 2} autres)";
            }

            StatusText = $"{summary} — {failureSummary}";
        }
        else
        {
            StatusText = $"{summary} — {exportFolder}";
        }

        if (exportedCount > 0)
        {
            TryOpenFolder(exportFolder);
        }
    }

    [RelayCommand]
    private void PurgeSigned()
    {
        var toRemove = Documents
            .Where(d => string.Equals(d.Status, "SIGNED", StringComparison.OrdinalIgnoreCase)
                     || string.Equals(d.Status, "EmailSent", StringComparison.OrdinalIgnoreCase)
                     || string.Equals(d.Status, "EMAIL_SENT", StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var d in toRemove)
        {
            Documents.Remove(d);
        }

        ApplyFilter();
        StatusText = toRemove.Count == 0
            ? "Aucun document signé à purger"
            : $"{toRemove.Count} document(s) signé(s) retiré(s) de la liste";
    }

    [RelayCommand]
    private void SelectAll()
    {
        if (Documents.Count == 0)
            return;

        _suppressDocumentNotifications = true;
        try
        {
            foreach (var d in Documents)
            {
                d.IsSelected = true;
            }
        }
        finally
        {
            _suppressDocumentNotifications = false;
        }

        NotifyCounters();
        ApplyFilter();
        OnPropertyChanged(nameof(CanSign));
    }

    [RelayCommand]
    private void SetFilter(string filter)
    {
        FilterStatus = filter;
        ApplyFilter();
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var q = (SearchText ?? string.Empty).Trim();
        FilteredDocuments.Clear();

        foreach (var d in Documents)
        {
            if (!MatchesFilter(d))
                continue;

            if (!MatchesSearch(d, q))
                continue;

            if (!MatchesDateRange(d))
                continue;

            FilteredDocuments.Add(d);
        }
    }

    private bool MatchesFilter(DocumentDto d)
    {
        if (FilterStatus == "all")
            return true;

        if (FilterStatus == "pending")
            return string.Equals(d.Status, "UNSIGNED", StringComparison.OrdinalIgnoreCase)
                || string.Equals(d.Status, "PENDING", StringComparison.OrdinalIgnoreCase);

        if (FilterStatus == "signed")
            return string.Equals(d.Status, "SIGNED", StringComparison.OrdinalIgnoreCase)
                || string.Equals(d.Status, "EMAIL_SENT", StringComparison.OrdinalIgnoreCase);

        if (FilterStatus == "error")
            return string.Equals(d.Status, "ERROR", StringComparison.OrdinalIgnoreCase)
                || string.Equals(d.Status, "FAILED", StringComparison.OrdinalIgnoreCase);

        return true;
    }

    private static bool MatchesSearch(DocumentDto d, string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return true;

        var nq = NormalizeForSearch(q);

        var name = NormalizeForSearch(d.StudentName ?? string.Empty);
        var reference = NormalizeForSearch(d.Reference ?? string.Empty);
        var dateText = d.CreatedAt.ToString("yyyy-MM-dd");

        if (name.Contains(nq, StringComparison.Ordinal))
            return true;

        if (!string.IsNullOrEmpty(reference) && reference.Contains(nq, StringComparison.Ordinal))
            return true;

        if (!string.IsNullOrWhiteSpace(dateText) && dateText.Contains(q, StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }

    private bool MatchesDateRange(DocumentDto d)
    {
        if (FromDateFilter == null && ToDateFilter == null)
            return true;

        var date = d.CreatedAt.Date;

        if (FromDateFilter != null && date < FromDateFilter.Value.Date)
            return false;

        if (ToDateFilter != null && date > ToDateFilter.Value.Date)
            return false;

        return true;
    }

    private static string NormalizeForSearch(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);

        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    private void ResetPreviewNavigation()
    {
        CurrentPreviewPage = 1;
        PreviewPageCount = 1;
        PreviewZoomPercent = 100;
        OnPropertyChanged(nameof(PreviewPageDisplay));
        OnPropertyChanged(nameof(PreviewZoomDisplay));
    }

    private void UpdatePreviewMetadata(byte[] pdfBytes)
    {
        PreviewPageCount = CountPdfPages(pdfBytes);
        CurrentPreviewPage = Math.Max(1, Math.Min(CurrentPreviewPage, PreviewPageCount));
    }

    private void ApplyPreviewUri(string filePath)
    {
        var absoluteUri = new Uri(filePath).AbsoluteUri;
        PreviewUri = new Uri($"{absoluteUri}#page={CurrentPreviewPage}&zoom={PreviewZoomPercent}");
    }

    private static int CountPdfPages(byte[] pdfBytes)
    {
        var content = Encoding.ASCII.GetString(pdfBytes);
        var count = Regex.Matches(content, @"/Type\s*/Page\b", RegexOptions.IgnoreCase).Count;
        return Math.Max(1, count);
    }

    private static string? GetPreviewPath(DocumentDto document, bool signed)
        => signed ? document.SignedPreviewPath : document.UnsignedPreviewPath;

    private static string BuildSafeAttestationFileName(DocumentDto document, HashSet<string> usedFileNames)
    {
        var studentName = NormalizeFileNameToken(document.StudentName, "etudiant");
        var apogee = NormalizeFileNameToken(document.StudentId, "inconnu");
        var baseName = $"attestation_{studentName}_{apogee}";

        var uniqueName = baseName;
        var suffix = 1;

        while (!usedFileNames.Add(uniqueName))
        {
            suffix += 1;
            uniqueName = $"{baseName}_{suffix}";
        }

        return $"{uniqueName}.pdf";
    }

    private static string NormalizeFileNameToken(string? value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);
        var previousUnderscore = false;

        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsLetterOrDigit(c))
            {
                sb.Append(c);
                previousUnderscore = false;
                continue;
            }

            if ((char.IsWhiteSpace(c) || c == '-' || c == '_')
                && !previousUnderscore
                && sb.Length > 0)
            {
                sb.Append('_');
                previousUnderscore = true;
            }
        }

        var token = sb.ToString().Trim('_');
        return string.IsNullOrWhiteSpace(token) ? fallback : token;
    }

    private static void TryOpenFolder(string folderPath)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = folderPath,
                UseShellExecute = true
            });
        }
        catch
        {
        }
    }
}
