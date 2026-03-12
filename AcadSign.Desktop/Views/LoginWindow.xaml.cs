using System;
using System.Windows;
using AcadSign.Desktop.Controls;
using AcadSign.Desktop.Services.Authentication;

namespace AcadSign.Desktop.Views;

public partial class LoginWindow : Window
{
    public event EventHandler<LoginSuccessEventArgs>? LoginSuccess;
    private readonly LoginControl _loginControl;

    public LoginWindow(IAuthenticationService authService)
    {
        InitializeComponent();
        
        // Créer le LoginControl avec le service d'authentification
        _loginControl = new LoginControl(authService);
        
        // S'abonner à l'événement de succès de connexion
        _loginControl.LoginSuccess += OnLoginSuccess;
        
        // Remplacer le LoginControl dans le Grid
        LoginControl.Content = _loginControl;
    }

    private void OnLoginSuccess(object? sender, LoginSuccessEventArgs e)
    {
        // Propager l'événement
        LoginSuccess?.Invoke(this, e);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        // Permet de déplacer la fenêtre en cliquant et glissant
        if (e.ButtonState == System.Windows.Input.MouseButtonState.Pressed)
        {
            this.DragMove();
        }
    }
}
