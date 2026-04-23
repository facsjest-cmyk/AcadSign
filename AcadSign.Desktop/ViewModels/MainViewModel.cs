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
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using AcadSign.Desktop.Properties;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Colors;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using System.Text.Json;

namespace AcadSign.Desktop.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private const string UpdateRepoOwner = "facsjest-cmyk";
    private const string UpdateRepoName = "AcadSign";
    private const string UpdateAssetName = "AcadSign-Setup.exe";

    private static int _updateCheckStarted;

    private readonly IApiClientService _apiClient;
    private readonly IAuthenticationService _authenticationService;
    private readonly ITokenStorageService _tokenStorageService;
    private readonly INavigationService _navigationService;
    private readonly IBatchSigningService _batchSigningService;
    private readonly IServiceProvider _serviceProvider;

    private bool _suppressDocumentNotifications;

    private CancellationTokenSource? _usbAlertCts;

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
    private bool _isUsbAlertVisible;

    [ObservableProperty]
    private string _usbAlertText = string.Empty;

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
        DongleStatusViewModel dongleStatus,
        IServiceProvider serviceProvider)
    {
        _apiClient = apiClient;
        _authenticationService = authenticationService;
        _tokenStorageService = tokenStorageService;
        _batchSigningService = batchSigningService;
        _navigationService = navigationService;
        DongleStatus = dongleStatus;
        _serviceProvider = serviceProvider;

        LoadDocumentsUiCommand = new AsyncRelayCommand(LoadDocumentsAsync);

        Documents.CollectionChanged += DocumentsOnCollectionChanged;

        var timer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        timer.Tick += (_, _) => CurrentTime = DateTime.Now.ToString("HH:mm:ss");
        timer.Start();
        
        _ = CheckForUpdatesAsync();
        _ = LoadDocumentsAsync();
    }

    private static Version? GetCurrentAppVersion()
    {
        try
        {
            return Assembly.GetExecutingAssembly().GetName().Version;
        }
        catch
        {
            return null;
        }
    }

    private static Version? TryParseVersion(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        var v = input.Trim();
        if (v.StartsWith("v", StringComparison.OrdinalIgnoreCase))
        {
            v = v.Substring(1);
        }

        if (Version.TryParse(v, out var version))
        {
            return version;
        }

        return null;
    }

    private async Task CheckForUpdatesAsync()
    {
        if (Interlocked.Exchange(ref _updateCheckStarted, 1) == 1)
        {
            return;
        }

        try
        {
            var currentVersion = GetCurrentAppVersion();
            if (currentVersion == null)
            {
                return;
            }

            using var http = new HttpClient();
            http.Timeout = TimeSpan.FromSeconds(10);
            http.DefaultRequestHeaders.UserAgent.Clear();
            http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("AcadSign", currentVersion.ToString()));
            http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

            var url = $"https://api.github.com/repos/{UpdateRepoOwner}/{UpdateRepoName}/releases/latest";
            var json = await http.GetStringAsync(url);

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var tagName = root.TryGetProperty("tag_name", out var tagProp) ? tagProp.GetString() : null;
            var htmlUrl = root.TryGetProperty("html_url", out var htmlProp) ? htmlProp.GetString() : null;

            var latestVersion = TryParseVersion(tagName);
            if (latestVersion == null)
            {
                return;
            }

            if (latestVersion <= currentVersion)
            {
                return;
            }

            string? downloadUrl = null;
            if (root.TryGetProperty("assets", out var assetsProp) && assetsProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var asset in assetsProp.EnumerateArray())
                {
                    var name = asset.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : null;
                    if (!string.IsNullOrWhiteSpace(name) && string.Equals(name, UpdateAssetName, StringComparison.OrdinalIgnoreCase))
                    {
                        downloadUrl = asset.TryGetProperty("browser_download_url", out var dlProp) ? dlProp.GetString() : null;
                        break;
                    }
                }

                if (string.IsNullOrWhiteSpace(downloadUrl))
                {
                    foreach (var asset in assetsProp.EnumerateArray())
                    {
                        var name = asset.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : null;
                        if (!string.IsNullOrWhiteSpace(name) && name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                        {
                            downloadUrl = asset.TryGetProperty("browser_download_url", out var dlProp) ? dlProp.GetString() : null;
                            if (!string.IsNullOrWhiteSpace(downloadUrl))
                            {
                                break;
                            }
                        }
                    }
                }
            }

            downloadUrl ??= htmlUrl;
            if (string.IsNullOrWhiteSpace(downloadUrl))
            {
                return;
            }

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var result = MessageBox.Show(
                    $"Une nouvelle version d'AcadSign est disponible.\n\nVersion installée: {currentVersion}\nNouvelle version: {latestVersion}\n\nVoulez-vous télécharger la mise à jour ?",
                    "Mise à jour disponible",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo(downloadUrl) { UseShellExecute = true });
                    }
                    catch
                    {
                    }
                }
            });
        }
        catch
        {
        }
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

    private static string? TryGetBackendBaseUrl(string? apiEndpoint)
    {
        if (string.IsNullOrWhiteSpace(apiEndpoint))
        {
            return null;
        }

        if (!Uri.TryCreate(apiEndpoint.Trim(), UriKind.Absolute, out var uri))
        {
            return null;
        }

        var builder = new UriBuilder(uri)
        {
            Port = 18080,
            Path = string.Empty,
            Query = string.Empty,
            Fragment = string.Empty
        };

        return builder.Uri.GetLeftPart(UriPartial.Authority).TrimEnd('/');
    }

    private static string? TryResolveFsjestUrl(string? apiEndpoint, string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        var trimmed = url.Trim();
        if (Uri.TryCreate(trimmed, UriKind.Absolute, out var absolute))
        {
            return absolute.ToString();
        }

        if (string.IsNullOrWhiteSpace(apiEndpoint))
        {
            return null;
        }

        var baseUrl = apiEndpoint.Trim().TrimEnd('/');
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri))
        {
            return null;
        }

        if (trimmed.StartsWith("/", StringComparison.Ordinal))
        {
            return $"{baseUri.GetLeftPart(UriPartial.Authority)}{trimmed}";
        }

        return $"{baseUrl}/{trimmed}";
    }

    private static byte[] StampTestSignature(byte[] inputPdf)
    {
        using var readerStream = new MemoryStream(inputPdf);
        using var outputStream = new MemoryStream();
        using var reader = new PdfReader(readerStream);
        using var writer = new PdfWriter(outputStream);
        using var pdfDoc = new PdfDocument(reader, writer);

        var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
        var pageCount = pdfDoc.GetNumberOfPages();
        for (var i = 1; i <= pageCount; i++)
        {
            var page = pdfDoc.GetPage(i);
            var pageSize = page.GetPageSize();
            var canvas = new PdfCanvas(page.NewContentStreamAfter(), page.GetResources(), pdfDoc);
            using var layout = new Canvas(canvas, pageSize);

            var text = new Paragraph("TEST SIGNATURE")
                .SetFont(font)
                .SetFontSize(54)
                .SetFontColor(new DeviceRgb(220, 38, 38))
                .SetOpacity(0.18f);

            layout.ShowTextAligned(
                text,
                pageSize.GetWidth() / 2,
                pageSize.GetHeight() / 2,
                i,
                iText.Layout.Properties.TextAlignment.CENTER,
                iText.Layout.Properties.VerticalAlignment.MIDDLE,
                (float)(Math.PI / 4));
        }

        pdfDoc.Close();
        return outputStream.ToArray();
    }

    private async Task ShowUsbAlertAsync(string message)
    {
        _usbAlertCts?.Cancel();
        _usbAlertCts = new CancellationTokenSource();
        var token = _usbAlertCts.Token;

        Application.Current.Dispatcher.Invoke(() =>
        {
            UsbAlertText = message;
            IsUsbAlertVisible = true;
        });

        try
        {
            await Task.Delay(TimeSpan.FromSeconds(4), token);
        }
        catch (TaskCanceledException)
        {
            return;
        }

        Application.Current.Dispatcher.Invoke(() =>
        {
            IsUsbAlertVisible = false;
        });
    }

    [RelayCommand]
    private async Task TestMinioUploadAsync()
    {
        if (SelectedDocument == null)
        {
            StatusText = "Sélectionnez un document";
            return;
        }

        try
        {
            IsLoading = true;
            StatusText = "Test MinIO: sélection du PDF local...";
            S3StatusText = "Test...";

            var dialog = new OpenFileDialog
            {
                Filter = "PDF (*.pdf)|*.pdf",
                Title = "Sélectionner un PDF local (test MinIO)"
            };

            if (dialog.ShowDialog() != true)
            {
                StatusText = "Test MinIO: annulé";
                S3StatusText = "Prêt";
                return;
            }

            var localPdf = await File.ReadAllBytesAsync(dialog.FileName);

            StatusText = "Test MinIO: génération du PDF de test...";
            var stampedPdf = StampTestSignature(localPdf);

            StatusText = "Test MinIO: connexion au backend...";

            var backendBaseUrl = TryGetBackendBaseUrl(Settings.Default.ApiEndpoint);
            backendBaseUrl ??= "http://10.2.22.210:18080";

            using var backendClient = new HttpClient { BaseAddress = new Uri(backendBaseUrl) };
            try
            {
                var (accessToken, _) = await _tokenStorageService.GetTokensAsync();
                if (!string.IsNullOrWhiteSpace(accessToken))
                {
                    backendClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", accessToken);
                }
            }
            catch
            {
            }

            StatusText = "Test MinIO: upload vers backend (MinIO)...";

            using var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(stampedPdf);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            content.Add(fileContent, "file", Path.GetFileName(dialog.FileName));

            using var response = await backendClient.PostAsync("/api/v1/test/minio/upload", content);
            var responseText = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                StatusText = $"Test MinIO: endpoint test introuvable (404) sur {backendBaseUrl}. Fallback via /documents/{{id}}/signed...";

                var pendingJson = await backendClient.GetStringAsync("/api/v1/documents/pending");
                var pending = JsonSerializer.Deserialize<List<BackendPendingDocumentDto>>(pendingJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<BackendPendingDocumentDto>();

                if (pending.Count == 0)
                {
                    S3StatusText = "Erreur";
                    StatusText =
                        $"Test MinIO: aucun document UNSIGNED/PENDING dans le backend ({backendBaseUrl}). " +
                        "Le serveur ne semble pas avoir l'endpoint /api/v1/test/minio/upload déployé (404), " +
                        "et /api/v1/documents/{id}/signed exige un document existant en DB. " +
                        "Solution: déployer le backend avec l'endpoint de test, ou créer au moins un document en attente côté backend.";
                    return;
                }

                var target = pending[0];
                using var content2 = new MultipartFormDataContent();
                var fileContent2 = new ByteArrayContent(stampedPdf);
                fileContent2.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                content2.Add(fileContent2, "file", Path.GetFileName(dialog.FileName));

                using var response2 = await backendClient.PostAsync($"/api/v1/documents/{target.Id}/signed", content2);
                var responseText2 = await response2.Content.ReadAsStringAsync();
                if (!response2.IsSuccessStatusCode)
                {
                    S3StatusText = "Erreur";
                    StatusText = $"Test MinIO: upload échoué ({(int)response2.StatusCode} {response2.StatusCode}). {responseText2}";
                    return;
                }

                S3StatusText = "Test OK";
                StatusText = $"Test MinIO: upload OK (backend={backendBaseUrl}, documentId={target.Id}).";
                return;
            }

            if (!response.IsSuccessStatusCode)
            {
                S3StatusText = "Erreur";
                StatusText = $"Test MinIO: upload échoué ({(int)response.StatusCode} {response.StatusCode}). {responseText}";
                return;
            }

            S3StatusText = "Test OK";
            try
            {
                using var doc = JsonDocument.Parse(responseText);
                if (doc.RootElement.TryGetProperty("objectPath", out var objectPathEl))
                {
                    StatusText = $"Test MinIO: upload OK. Object: {objectPathEl.GetString()}";
                }
                else
                {
                    StatusText = $"Test MinIO: upload OK (backend={backendBaseUrl})";
                }
            }
            catch
            {
                StatusText = $"Test MinIO: upload OK (backend={backendBaseUrl})";
            }
        }
        catch (Exception ex)
        {
            S3StatusText = "Erreur";
            StatusText = $"Test MinIO: erreur ({ex.Message})";
        }
        finally
        {
            IsLoading = false;
            OnPropertyChanged(nameof(CanSign));
        }
    }
    
    [RelayCommand]
    private async Task SignDocument(DocumentDto? document)
    {
        if (document == null)
        {
            StatusText = "Sélectionnez un document";
            return;
        }

        if (!DongleStatus.IsDongleConnected)
        {
            await ShowUsbAlertAsync("USB hors ligne — veuillez connecter votre USB de signature");
            return;
        }

        var viewModel = _serviceProvider.GetRequiredService<SigningViewModel>();
        if (viewModel is INavigationAware aware)
        {
            aware.OnNavigatedTo(document);
        }

        var view = new SigningView
        {
            DataContext = viewModel,
            Owner = Application.Current.MainWindow,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        view.ShowDialog();

        if (string.Equals(document.Status, "SIGNED", StringComparison.OrdinalIgnoreCase))
        {
            SelectedDocument = document;
            OpenAfterSignature();
        }
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

        if (!DongleStatus.IsDongleConnected)
        {
            await ShowUsbAlertAsync("USB hors ligne — veuillez connecter votre USB de signature");
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

            using var httpClient = new HttpClient();
            try
            {
                var (accessToken, _) = await _tokenStorageService.GetTokensAsync();
                if (!string.IsNullOrWhiteSpace(accessToken))
                {
                    httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", accessToken);
                }
            }
            catch
            {
            }

            if (signed && !string.IsNullOrWhiteSpace(document.Reference))
            {
                var baseUrl = AcadSign.Desktop.Properties.Settings.Default.ApiEndpoint;
                var url = TryResolveFsjestUrl(baseUrl, $"/api/v1/admin/documents/{document.Reference}/download");
                if (string.IsNullOrWhiteSpace(url))
                {
                    throw new InvalidOperationException("ApiEndpoint is invalid; cannot build signed download URL.");
                }

                using var response = await httpClient.GetAsync(url, token);
                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync(token);
                    throw new InvalidOperationException($"Signed PDF download failed: HTTP {(int)response.StatusCode} {response.StatusCode}. Url={url}. Body={body}");
                }

                bytes = await response.Content.ReadAsByteArrayAsync(token);
            }
            else if (!signed && !string.IsNullOrWhiteSpace(document.SourcePdfUrl))
            {
                var baseUrl = AcadSign.Desktop.Properties.Settings.Default.ApiEndpoint;
                var url = TryResolveFsjestUrl(baseUrl, document.SourcePdfUrl);
                if (string.IsNullOrWhiteSpace(url))
                {
                    throw new InvalidOperationException("SourcePdfUrl is relative but ApiEndpoint is invalid; cannot resolve PDF URL.");
                }

                using var response = await httpClient.GetAsync(url, token);
                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync(token);
                    throw new InvalidOperationException($"PDF download failed: HTTP {(int)response.StatusCode} {response.StatusCode}. Url={url}. Body={body}");
                }

                bytes = await response.Content.ReadAsByteArrayAsync(token);
            }
            else
            {
                bytes = await _apiClient.DownloadDocumentAsync(document.Id);
            }
            token.ThrowIfCancellationRequested();

            var folder = Path.Combine(Path.GetTempPath(), "AcadSign", "previews");
            Directory.CreateDirectory(folder);
            var suffix = signed ? "signed" : "unsigned";
            var filePath = Path.Combine(folder, $"{document.Id}_{suffix}_{DateTime.UtcNow.Ticks}.pdf");
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
