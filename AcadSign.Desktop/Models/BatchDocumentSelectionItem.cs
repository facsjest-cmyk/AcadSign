using CommunityToolkit.Mvvm.ComponentModel;

namespace AcadSign.Desktop.Models;

public partial class BatchDocumentSelectionItem : ObservableObject
{
    public DocumentDto Document { get; }

    [ObservableProperty]
    private bool _isSelected;

    public BatchDocumentSelectionItem(DocumentDto document)
    {
        Document = document;
        IsSelected = true;
    }
}
