using AcadSign.Desktop.Models;
using AcadSign.Desktop.Services.Batch;
using AcadSign.Desktop.Services.Navigation;
using AcadSign.Desktop.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace AcadSign.Desktop.ViewModels;

public partial class BatchSigningViewModel : ObservableObject, INavigationAware
{
    private readonly IBatchSigningService _batchSigningService;
    private readonly INavigationService _navigationService;

    private List<DocumentDto> _documents = new();
    private readonly Stopwatch _stopwatch = new();

    public ObservableCollection<BatchDocumentSelectionItem> Documents { get; } = new();

    private List<DocumentDto> SelectedDocuments => Documents.Where(d => d.IsSelected).Select(d => d.Document).ToList();

    [ObservableProperty]
    private int _totalDocuments;

    [ObservableProperty]
    private int _processedDocuments;

    [ObservableProperty]
    private int _failedDocuments;

    [ObservableProperty]
    private string _currentDocumentName = string.Empty;

    [ObservableProperty]
    private double _progressPercentage;

    [ObservableProperty]
    private string _estimatedTimeRemaining = string.Empty;

    [ObservableProperty]
    private bool _isSigning;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public ObservableCollection<DocumentSignResult> FailedDocumentResults { get; } = new();

    public BatchSigningViewModel(IBatchSigningService batchSigningService, INavigationService navigationService)
    {
        _batchSigningService = batchSigningService;
        _navigationService = navigationService;
    }

    public void OnNavigatedTo(object parameter)
    {
        if (parameter is List<DocumentDto> docs)
        {
            _documents = docs;
            Documents.Clear();
            foreach (var d in docs)
            {
                Documents.Add(new BatchDocumentSelectionItem(d));
            }

            TotalDocuments = docs.Count;
            ProcessedDocuments = 0;
            FailedDocuments = 0;
            ProgressPercentage = 0;
            CurrentDocumentName = string.Empty;
            EstimatedTimeRemaining = string.Empty;
            StatusMessage = $"Prêt: {TotalDocuments} document(s)";
            FailedDocumentResults.Clear();
        }
    }

    [RelayCommand]
    private async Task StartAsync()
    {
        if (IsSigning)
        {
            return;
        }

        var docs = SelectedDocuments;
        if (docs.Count == 0)
        {
            StatusMessage = "Aucun document à signer";
            return;
        }

        try
        {
            IsSigning = true;
            StatusMessage = "Saisie du PIN...";

            var pin = await GetPinFromUserAsync();
            if (string.IsNullOrWhiteSpace(pin))
            {
                StatusMessage = "Batch annulé (PIN requis)";
                return;
            }

            _stopwatch.Restart();

            var progress = new Progress<BatchProgress>(p =>
            {
                ProcessedDocuments = p.CurrentDocument;
                CurrentDocumentName = p.CurrentDocumentName;
                ProgressPercentage = p.PercentComplete;
                StatusMessage = p.Status;

                if (p.CurrentDocument > 0)
                {
                    var elapsed = _stopwatch.Elapsed;
                    var remainingDocs = Math.Max(0, p.TotalDocuments - p.CurrentDocument);
                    var avg = TimeSpan.FromMilliseconds(elapsed.TotalMilliseconds / p.CurrentDocument);
                    var remaining = TimeSpan.FromMilliseconds(avg.TotalMilliseconds * remainingDocs);
                    EstimatedTimeRemaining = $"~{(int)remaining.TotalMinutes} min";
                }
            });

            TotalDocuments = docs.Count;
            var result = await _batchSigningService.SignBatchAsync(docs, pin, progress);

            FailedDocumentResults.Clear();
            foreach (var r in result.Results.Where(x => !x.Success))
            {
                FailedDocumentResults.Add(r);
            }

            FailedDocuments = result.FailedSigns;
            ProcessedDocuments = Math.Min(result.TotalDocuments, result.SuccessfulSigns + result.FailedSigns);
            ProgressPercentage = result.TotalDocuments > 0 ? (double)ProcessedDocuments / result.TotalDocuments * 100 : 0;

            StatusMessage = $"Batch terminé: {result.SuccessfulSigns} OK, {result.FailedSigns} échec(s) - Durée: {result.Duration:mm\\:ss}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur batch: {ex.Message}";
        }
        finally
        {
            _stopwatch.Stop();
            IsSigning = false;
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        try
        {
            await _batchSigningService.CancelBatchAsync();
            StatusMessage = "Annulation demandée";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur annulation: {ex.Message}";
        }
    }

    [RelayCommand]
    private void Close()
    {
        _navigationService.NavigateTo<MainViewModel>();
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
}
