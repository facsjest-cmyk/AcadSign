using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using AcadSign.Models;
using AcadSign.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AcadSign.ViewModels;

public partial class MainViewModel : ObservableObject
{
    // ── Dependencies ──────────────────────────────────────────────────────
    private readonly ISisApiService      _sisApi;
    private readonly IESignService       _eSign;
    private readonly IS3StorageService   _s3;
    private readonly IEmailService       _email;
    private readonly IPdfGeneratorService _pdfGen;
    private readonly IPdfViewerService   _pdfViewer;

    // ── Observable Properties ─────────────────────────────────────────────

    [ObservableProperty] private ObservableCollection<DocumentRequest> _documents = new();
    [ObservableProperty] private ObservableCollection<DocumentRequest> _filteredDocuments = new();
    [ObservableProperty] private DocumentRequest?                       _selectedDocument;
    [ObservableProperty] private string                                 _searchText = string.Empty;
    [ObservableProperty] private string                                 _filterStatus = "all";

    // PDF Viewer
    [ObservableProperty] private System.Windows.Media.Imaging.BitmapSource? _pdfPageImage;
    [ObservableProperty] private bool                                        _isSignedView;
    [ObservableProperty] private int                                         _currentPage = 1;
    [ObservableProperty] private int                                         _totalPages = 1;
    [ObservableProperty] private double                                      _zoomLevel = 1.0;

    // Signing
    [ObservableProperty] private string  _pin = string.Empty;
    [ObservableProperty] private bool    _isSigning;
    [ObservableProperty] private int     _signProgress;
    [ObservableProperty] private string  _signProgressLabel = string.Empty;
    [ObservableProperty] private bool    _showSignProgress;
    [ObservableProperty] private FlowDocument _signLog = new();

    // Batch
    [ObservableProperty] private bool    _isBatchSigning;
    [ObservableProperty] private int     _batchProgress;
    [ObservableProperty] private string  _batchProgressLabel = string.Empty;
    [ObservableProperty] private FlowDocument _batchLog = new();
    [ObservableProperty] private bool    _showBatchDialog;

    // Email
    [ObservableProperty] private string  _emailTo = string.Empty;
    [ObservableProperty] private string  _emailSubject = string.Empty;
    [ObservableProperty] private bool    _isSendingEmail;
    [ObservableProperty] private bool    _emailSent;

    // API / Status
    [ObservableProperty] private bool    _isLoadingFromApi;
    [ObservableProperty] private string  _apiStatus = "Prêt";
    [ObservableProperty] private string  _apiStatusColor = "#F59E0B";
    [ObservableProperty] private string  _eSignStatus = "Connecté";
    [ObservableProperty] private string  _eSignStatusColor = "#10B981";
    [ObservableProperty] private string  _s3Status = "En ligne";
    [ObservableProperty] private string  _s3StatusColor = "#10B981";
    [ObservableProperty] private string  _statusBarMessage = string.Empty;
    [ObservableProperty] private string  _currentTime = DateTime.Now.ToString("HH:mm:ss");

    // Computed counters
    public int PendingCount  => Documents.Count(d => d.Status == DocumentStatus.Pending);
    public int SignedCount   => Documents.Count(d => d.Status == DocumentStatus.Signed
                                                  || d.Status == DocumentStatus.EmailSent);
    public int SelectedCount => Documents.Count(d => d.IsSelected);
    public int TotalCount    => Documents.Count;

    public bool CanSign   => SelectedDocument != null
                          && SelectedDocument.Status == DocumentStatus.Pending
                          && Pin.Length >= 4
                          && !IsSigning;

    public bool CanSendEmail => SelectedDocument?.Status == DocumentStatus.Signed
                             && !string.IsNullOrEmpty(EmailTo)
                             && !IsSendingEmail;

    public bool HasSelectedItems => SelectedCount > 0;

    // ── Constructor ───────────────────────────────────────────────────────

    public MainViewModel(
        ISisApiService sisApi,
        IESignService eSign,
        IS3StorageService s3,
        IEmailService email,
        IPdfGeneratorService pdfGen,
        IPdfViewerService pdfViewer)
    {
        _sisApi    = sisApi;
        _eSign     = eSign;
        _s3        = s3;
        _email     = email;
        _pdfGen    = pdfGen;
        _pdfViewer = pdfViewer;

        // Clock update
        var timer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        timer.Tick += (_, _) => CurrentTime = DateTime.Now.ToString("HH:mm:ss");
        timer.Start();

        // Load mock data
        LoadMockData();
        ApplyFilter();
    }

    // ══════════════════════════════════════════════════════════════════════
    //  COMMANDS
    // ══════════════════════════════════════════════════════════════════════

    // ── Load from API ─────────────────────────────────────────────────────

    [RelayCommand]
    private async Task FetchFromApiAsync()
    {
        IsLoadingFromApi = true;
        ApiStatus       = "Chargement...";
        ApiStatusColor  = "#F59E0B";
        StatusBarMessage = "Récupération des demandes depuis l'API SIS...";

        try
        {
            var requests = await _sisApi.FetchPendingRequestsAsync();
            foreach (var req in requests)
            {
                if (!Documents.Any(d => d.Id == req.Id))
                    Documents.Add(req);
            }
            ApplyFilter();
            NotifyCounters();

            ApiStatus       = "Synchronisé";
            ApiStatusColor  = "#10B981";
            StatusBarMessage = $"{requests.Count} nouvelles demandes importées";
        }
        catch (Exception ex)
        {
            ApiStatus       = "Erreur";
            ApiStatusColor  = "#EF4444";
            StatusBarMessage = $"Erreur API: {ex.Message}";
        }
        finally
        {
            IsLoadingFromApi = false;
        }
    }

    // ── Select Document ───────────────────────────────────────────────────

    partial void OnSelectedDocumentChanged(DocumentRequest? value)
    {
        if (value == null) return;
        EmailTo      = value.Student.Email;
        EmailSubject = $"Votre {value.DisplayType} — Université Hassan II";
        EmailSent    = false;
        IsSignedView = value.Status is DocumentStatus.Signed or DocumentStatus.EmailSent;
        _ = LoadPdfPreviewAsync(value);
        OnPropertyChanged(nameof(CanSign));
        OnPropertyChanged(nameof(CanSendEmail));
    }

    partial void OnPinChanged(string value)
    {
        OnPropertyChanged(nameof(CanSign));
    }

    // ── PDF Preview ───────────────────────────────────────────────────────

    [RelayCommand]
    private async Task ShowBeforeSignatureAsync()
    {
        IsSignedView = false;
        if (SelectedDocument == null) return;
        await LoadPdfPreviewAsync(SelectedDocument, signed: false);
    }

    [RelayCommand]
    private async Task ShowAfterSignatureAsync()
    {
        if (SelectedDocument?.Status is not (DocumentStatus.Signed or DocumentStatus.EmailSent))
            return;
        IsSignedView = true;
        await LoadPdfPreviewAsync(SelectedDocument, signed: true);
    }

    [RelayCommand]
    private void ZoomIn()  { ZoomLevel = Math.Min(ZoomLevel + 0.25, 3.0); }

    [RelayCommand]
    private void ZoomOut() { ZoomLevel = Math.Max(ZoomLevel - 0.25, 0.5); }

    private async Task LoadPdfPreviewAsync(DocumentRequest req, bool? signed = null)
    {
        var useSigned = signed ?? IsSignedView;
        var path = useSigned ? req.SignedPdfPath : req.UnsignedPdfPath;

        byte[] pdfBytes;
        if (!string.IsNullOrEmpty(path) && File.Exists(path))
            pdfBytes = await File.ReadAllBytesAsync(path);
        else
        {
            pdfBytes = await _pdfGen.GenerateAsync(req);
            if (!useSigned)
            {
                var tmpPath = Path.Combine(Path.GetTempPath(), $"acadSign_{req.Id}_unsigned.pdf");
                await File.WriteAllBytesAsync(tmpPath, pdfBytes);
                req.UnsignedPdfPath = tmpPath;
            }
        }

        TotalPages  = _pdfViewer.GetPageCount(pdfBytes);
        CurrentPage = 1;
        PdfPageImage = _pdfViewer.RenderPage(pdfBytes, 0);
    }

    // ── Sign Single ───────────────────────────────────────────────────────

    [RelayCommand(CanExecute = nameof(CanSign))]
    private async Task SignDocumentAsync()
    {
        if (SelectedDocument == null || Pin.Length < 4) return;

        IsSigning        = true;
        ShowSignProgress = true;
        SignProgress     = 0;
        SignLog          = new FlowDocument();
        SelectedDocument.Status = DocumentStatus.Signing;
        StatusBarMessage = $"Signature en cours: {SelectedDocument.DisplayName}";

        var progress = new Progress<(int pct, string msg)>(update =>
        {
            SignProgress      = update.pct;
            SignProgressLabel = update.msg;
            AppendLog(SignLog, update.msg, update.pct == 100 ? "#10B981" : "#94A3B8");
        });

        try
        {
            // Generate PDF if needed
            byte[] pdfBytes;
            if (!string.IsNullOrEmpty(SelectedDocument.UnsignedPdfPath)
                && File.Exists(SelectedDocument.UnsignedPdfPath))
                pdfBytes = await File.ReadAllBytesAsync(SelectedDocument.UnsignedPdfPath);
            else
                pdfBytes = await _pdfGen.GenerateAsync(SelectedDocument);

            // Sign
            var result = await _eSign.SignDocumentAsync(pdfBytes, Pin, SelectedDocument, progress);

            if (!result.Success)
                throw new InvalidOperationException(result.Error ?? "Erreur inconnue");

            // Save signed PDF
            var signedPath = Path.Combine(
                Path.GetTempPath(), $"acadSign_{SelectedDocument.Id}_signed.pdf");
            await File.WriteAllBytesAsync(signedPath, result.SignedPdfBytes!);
            SelectedDocument.SignedPdfPath    = signedPath;
            SelectedDocument.SignedAt         = result.SignedAt;
            SelectedDocument.CertificateSerial = result.CertificateSerial;
            SelectedDocument.SignatureHash    = result.DocumentHash;
            SelectedDocument.TimestampToken   = result.TimestampToken;

            // Upload to S3
            AppendLog(SignLog, "Téléversement vers le serveur S3...", "#94A3B8");
            var s3Key = S3StorageService.BuildS3Key(SelectedDocument);
            var s3Result = await _s3.UploadAsync(
                result.SignedPdfBytes!,
                s3Key,
                new Dictionary<string, string>
                {
                    ["student-id"]   = SelectedDocument.Student.Id,
                    ["doc-type"]     = SelectedDocument.DocumentType.ToString(),
                    ["signed-at"]    = result.SignedAt.ToString("O"),
                    ["cert-serial"]  = result.CertificateSerial ?? ""
                });

            SelectedDocument.S3Url = s3Result.Url;
            AppendLog(SignLog, s3Result.Success
                ? $"✓ Stocké: {s3Key}"
                : $"⚠ S3: {s3Result.Error}", s3Result.Success ? "#10B981" : "#EF4444");

            // Mark as signed
            SelectedDocument.Status = DocumentStatus.Signed;
            IsSignedView = true;
            await LoadPdfPreviewAsync(SelectedDocument, signed: true);

            NotifyCounters();
            StatusBarMessage = $"✓ {SelectedDocument.DisplayName} — Document signé et sauvegardé";
        }
        catch (Exception ex)
        {
            SelectedDocument.Status       = DocumentStatus.Error;
            SelectedDocument.ErrorMessage = ex.Message;
            AppendLog(SignLog, $"✗ Erreur: {ex.Message}", "#EF4444");
            StatusBarMessage = $"Erreur: {ex.Message}";
        }
        finally
        {
            IsSigning = false;
            OnPropertyChanged(nameof(CanSign));
        }
    }

    // ── Batch Sign ────────────────────────────────────────────────────────

    [RelayCommand]
    private void OpenBatchSignDialog()
    {
        if (SelectedCount == 0) return;
        ShowBatchDialog = true;
        BatchLog        = new FlowDocument();
        BatchProgress   = 0;
    }

    [RelayCommand]
    private void CloseBatchDialog() => ShowBatchDialog = false;

    [RelayCommand]
    private async Task StartBatchSignAsync()
    {
        var pendingSelected = Documents
            .Where(d => d.IsSelected && d.Status == DocumentStatus.Pending)
            .ToList();

        if (!pendingSelected.Any()) return;

        IsBatchSigning = true;
        BatchProgress  = 0;
        BatchLog       = new FlowDocument();

        int done = 0;
        foreach (var doc in pendingSelected)
        {
            BatchProgressLabel = $"Signature: {doc.DisplayName}";
            AppendLog(BatchLog, $"→ Traitement: {doc.DisplayName} ({doc.DocumentType})...", "#94A3B8");

            doc.Status = DocumentStatus.Signing;

            var innerProgress = new Progress<(int pct, string msg)>(u =>
                AppendLog(BatchLog, $"  {u.msg}", "#64748B"));

            try
            {
                var pdfBytes = await _pdfGen.GenerateAsync(doc);
                var result   = await _eSign.SignDocumentAsync(pdfBytes, Pin, doc, innerProgress);

                if (result.Success)
                {
                    var path = Path.Combine(Path.GetTempPath(), $"acadSign_{doc.Id}_signed.pdf");
                    await File.WriteAllBytesAsync(path, result.SignedPdfBytes!);
                    doc.SignedPdfPath    = path;
                    doc.SignedAt        = result.SignedAt;
                    doc.CertificateSerial = result.CertificateSerial;

                    var s3Key = S3StorageService.BuildS3Key(doc);
                    var s3Res = await _s3.UploadAsync(result.SignedPdfBytes!, s3Key);
                    doc.S3Url  = s3Res.Url;
                    doc.Status = DocumentStatus.Signed;

                    AppendLog(BatchLog, $"✓ {doc.DisplayName} — Signé & uploadé S3", "#10B981");
                }
                else
                {
                    doc.Status       = DocumentStatus.Error;
                    doc.ErrorMessage = result.Error ?? "Erreur inconnue";
                    AppendLog(BatchLog, $"✗ {doc.DisplayName}: {doc.ErrorMessage}", "#EF4444");
                }
            }
            catch (Exception ex)
            {
                doc.Status       = DocumentStatus.Error;
                doc.ErrorMessage = ex.Message;
                AppendLog(BatchLog, $"✗ {doc.DisplayName}: {ex.Message}", "#EF4444");
            }

            done++;
            BatchProgress = (int)((double)done / pendingSelected.Count * 100);
            await Task.Delay(100); // Small delay to let UI update
        }

        IsBatchSigning     = false;
        BatchProgressLabel = $"✓ {done} document(s) traité(s)";
        NotifyCounters();
        StatusBarMessage = $"Lot terminé: {done} documents signés";
    }

    // ── Select All ────────────────────────────────────────────────────────

    [RelayCommand]
    private void SelectAllPending()
    {
        foreach (var d in Documents.Where(x => x.Status == DocumentStatus.Pending))
            d.IsSelected = true;
        OnPropertyChanged(nameof(SelectedCount));
        OnPropertyChanged(nameof(HasSelectedItems));
    }

    [RelayCommand]
    private void ClearSelection()
    {
        foreach (var d in Documents) d.IsSelected = false;
        OnPropertyChanged(nameof(SelectedCount));
        OnPropertyChanged(nameof(HasSelectedItems));
    }

    // ── Email ─────────────────────────────────────────────────────────────

    [RelayCommand(CanExecute = nameof(CanSendEmail))]
    private async Task SendEmailAsync()
    {
        if (SelectedDocument?.SignedPdfPath == null) return;

        IsSendingEmail = true;
        StatusBarMessage = $"Envoi email à {EmailTo}...";

        try
        {
            var pdfBytes = await File.ReadAllBytesAsync(SelectedDocument.SignedPdfPath);
            var body     = EmailService.BuildEmailBody(SelectedDocument);
            var filename = $"{SelectedDocument.DisplayType}_{SelectedDocument.Student.Id}.pdf";

            await _email.SendDocumentAsync(
                EmailTo, SelectedDocument.Student.FullName,
                EmailSubject, body, pdfBytes, filename);

            SelectedDocument.Status = DocumentStatus.EmailSent;
            EmailSent = true;
            StatusBarMessage = $"✓ Email envoyé à {EmailTo}";
        }
        catch (Exception ex)
        {
            StatusBarMessage = $"Erreur email: {ex.Message}";
        }
        finally
        {
            IsSendingEmail = false;
            OnPropertyChanged(nameof(CanSendEmail));
        }
    }

    // ── Filter / Search ───────────────────────────────────────────────────

    partial void OnSearchTextChanged(string value)  => ApplyFilter();
    partial void OnFilterStatusChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        var q = SearchText.Trim().ToLower();
        FilteredDocuments.Clear();
        foreach (var d in Documents)
        {
            bool matchStatus = FilterStatus == "all"
                || (FilterStatus == "pending" && d.Status == DocumentStatus.Pending)
                || (FilterStatus == "signed"  && d.Status is DocumentStatus.Signed or DocumentStatus.EmailSent)
                || (FilterStatus == "error"   && d.Status == DocumentStatus.Error);

            bool matchSearch = string.IsNullOrEmpty(q)
                || d.Student.FullName.ToLower().Contains(q)
                || d.Student.Id.ToLower().Contains(q)
                || d.DisplayType.ToLower().Contains(q)
                || d.Reference.ToLower().Contains(q);

            if (matchStatus && matchSearch)
                FilteredDocuments.Add(d);
        }
        NotifyCounters();
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private void NotifyCounters()
    {
        OnPropertyChanged(nameof(PendingCount));
        OnPropertyChanged(nameof(SignedCount));
        OnPropertyChanged(nameof(SelectedCount));
        OnPropertyChanged(nameof(TotalCount));
        OnPropertyChanged(nameof(HasSelectedItems));
    }

    private static void AppendLog(FlowDocument doc, string msg, string color)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var time = DateTime.Now.ToString("HH:mm:ss");
            var para = new Paragraph { Margin = new Thickness(0, 1, 0, 1) };

            para.Inlines.Add(new Run($"[{time}] ")
            {
                Foreground = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#334155"))
            });
            para.Inlines.Add(new Run(msg)
            {
                Foreground = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString(color))
            });

            doc.Blocks.Add(para);
        });
    }

    // ── Mock Data ─────────────────────────────────────────────────────────

    private void LoadMockData()
    {
        var mockStudents = new[]
        {
            new Student { Id="E-2024-001", FullName="Yassine Benali",     FullNameAr="ياسين بنعلي",       Cin="BE123456", Email="y.benali@etu.uh2.ac.ma",    Program="Génie Informatique",  Level="3ème année" },
            new Student { Id="E-2024-002", FullName="Fatima Zahra Moutii",FullNameAr="فاطمة الزهراء الموتي",Cin="HH789012", Email="fz.moutii@etu.uh2.ac.ma",  Program="Sciences Économiques",Level="2ème année" },
            new Student { Id="E-2024-003", FullName="Omar Cherkaoui",     FullNameAr="عمر الشرقاوي",       Cin="AA345678", Email="o.cherkaoui@etu.uh2.ac.ma", Program="Droit Privé",         Level="4ème année" },
            new Student { Id="E-2024-004", FullName="Amina Tazi",         FullNameAr="أمينة التازي",       Cin="CD567890", Email="a.tazi@etu.uh2.ac.ma",      Program="Médecine",            Level="1ère année" },
            new Student { Id="E-2024-005", FullName="Karim Alaoui",       FullNameAr="كريم العلوي",        Cin="EF234567", Email="k.alaoui@etu.uh2.ac.ma",    Program="Architecture",        Level="5ème année" },
            new Student { Id="E-2024-006", FullName="Salma Berrada",      FullNameAr="سلمى البراده",       Cin="GH901234", Email="s.berrada@etu.uh2.ac.ma",   Program="Pharmacie",           Level="3ème année" },
            new Student { Id="E-2024-007", FullName="Hamza Lahlou",       FullNameAr="حمزة لحلو",          Cin="IJ678901", Email="h.lahlou@etu.uh2.ac.ma",    Program="Génie Civil",         Level="2ème année" },
            new Student { Id="E-2024-008", FullName="Nadia Ouazzani",     FullNameAr="نادية الوزاني",      Cin="KL345012", Email="n.ouazzani@etu.uh2.ac.ma",  Program="Informatique",        Level="Master 1"   },
        };

        var types = new[] {
            DocumentType.AttestationScolarite, DocumentType.ReleveNotes,
            DocumentType.AttestationReussite,  DocumentType.AttestationInscription
        };
        var statuses = new[] {
            DocumentStatus.Pending, DocumentStatus.Signed, DocumentStatus.Pending,
            DocumentStatus.Signed,  DocumentStatus.Pending,DocumentStatus.Error,
            DocumentStatus.Pending, DocumentStatus.Signed
        };

        for (int i = 0; i < mockStudents.Length; i++)
        {
            Documents.Add(new DocumentRequest
            {
                Student      = mockStudents[i],
                DocumentType = types[i % types.Length],
                Status       = statuses[i],
                Reference    = $"REF-2024-{i+1:000}",
                RequestedAt  = DateTime.Now.AddHours(-i * 3)
            });
        }
    }
}
