using System.Windows;

namespace AcadSign.Desktop.Views;

public partial class PinDialog : Window
{
    public string Pin { get; private set; } = string.Empty;

    public PinDialog()
    {
        InitializeComponent();
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        Pin = PinBox.Password;
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
