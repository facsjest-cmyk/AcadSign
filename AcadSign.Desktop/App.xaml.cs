using System;
using System.IO;
using System.Windows;
using AcadSign.Desktop.ViewModels;
using AcadSign.Desktop.Views;
using AcadSign.Desktop.Services.Navigation;
using AcadSign.Desktop.Services.Authentication;
using AcadSign.Desktop.Services.Api;
using AcadSign.Desktop.Services.Signature;
using AcadSign.Desktop.Services.Dongle;
using AcadSign.Desktop.Services.Batch;
using AcadSign.Desktop.Services.Storage;
using AcadSign.Desktop.Services.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Refit;

namespace AcadSign.Desktop;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private ServiceProvider? _serviceProvider;
    private static readonly string StartupLogPath = Path.Combine(Path.GetTempPath(), "AcadSign.Desktop.startup.log");

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        LogStartup("OnStartup begin");

        // Capturer les exceptions non gérées
        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            var ex = args.ExceptionObject as Exception;
            LogStartup($"UnhandledException: {ex}");
            MessageBox.Show($"Exception non gérée:\n{ex?.Message}\n\nStack:\n{ex?.StackTrace}", 
                "Erreur Critique", MessageBoxButton.OK, MessageBoxImage.Error);
        };

        DispatcherUnhandledException += (sender, args) =>
        {
            LogStartup($"DispatcherUnhandledException: {args.Exception}");
            MessageBox.Show($"Exception Dispatcher:\n{args.Exception.Message}\n\nStack:\n{args.Exception.StackTrace}", 
                "Erreur Dispatcher", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        };

        try
        {
            var services = new ServiceCollection();
            LogStartup("ServiceCollection created");
            
            // Ajouter les services un par un avec logging
            try
            {
                ConfigureServices(services);
                _serviceProvider = services.BuildServiceProvider();
                LogStartup("ServiceProvider built");
            }
            catch (Exception ex)
            {
                LogStartup($"ConfigureServices failed: {ex}");
                MessageBox.Show($"Erreur lors de la configuration des services:\n{ex.Message}\n\n{ex.InnerException?.Message}", "Erreur Configuration", MessageBoxButton.OK, MessageBoxImage.Error);
                //Shutdown();
                return;
            }

            // Afficher la fenêtre de connexion
            try
            {
                var tokenStorage = _serviceProvider.GetRequiredService<ITokenStorageService>();
                var settings = AcadSign.Desktop.Properties.Settings.Default;
                var shouldRemember = settings.RememberMe;
                LogStartup($"RememberMe={shouldRemember}");

                var tokenFilePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "AcadSign",
                    "tokens.dat");

                if (!shouldRemember)
                {
                    try
                    {
                        if (File.Exists(tokenFilePath))
                        {
                            File.Delete(tokenFilePath);
                        }
                        LogStartup("Tokens deleted because RememberMe is false");
                    }
                    catch
                    {
                        LogStartup("Token deletion failed");
                    }
                }
                else if (File.Exists(tokenFilePath))
                {
                    LogStartup("Remembered token file found, opening main window");
                    ShowMainWindow();
                    return;
                }

                var authService = _serviceProvider.GetRequiredService<IAuthenticationService>();
                LogStartup("Authentication service resolved");
                var loginWindow = new LoginWindow(authService);
                LogStartup("LoginWindow created");
                
                // S'abonner à l'événement de connexion réussie
                loginWindow.LoginSuccess += async (sender, args) =>
                {
                    try
                    {
                        var appTokenStorage = _serviceProvider.GetRequiredService<ITokenStorageService>();
                        if (args.RememberMe)
                        {
                            await appTokenStorage.SaveTokensAsync(args.AccessToken, args.RefreshToken);
                        }
                        else
                        {
                            await appTokenStorage.DeleteTokensAsync();
                        }
                    }
                    catch
                    {
                        LogStartup("Token save/delete failed during login success");
                    }

                    // Fermer la fenêtre de connexion
                    loginWindow.Close();
                    
                    // Afficher la fenêtre principale
                    LogStartup("Login success, showing main window");
                    ShowMainWindow();
                };
                
                loginWindow.Show();
                LogStartup("LoginWindow shown");
            }
            catch (Exception ex)
            {
                LogStartup($"Login window startup failed: {ex}");
                MessageBox.Show($"Erreur lors de la création de la fenêtre de connexion:\n{ex.Message}\n\n{ex.InnerException?.Message}\n\nStack:\n{ex.StackTrace}", "Erreur Fenêtre", MessageBoxButton.OK, MessageBoxImage.Error);
                //Shutdown();
            }
        }
        catch (Exception ex)
        {
            LogStartup($"General startup failure: {ex}");
            MessageBox.Show($"Erreur générale au démarrage:\n{ex.Message}\n\n{ex.StackTrace}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            //Shutdown();
        }
    }

    private void ShowMainWindow()
    {
        try
        {
            LogStartup("ShowMainWindow begin");
            var mainView = _serviceProvider!.GetRequiredService<MainView>();
            
            // Définir MainView comme fenêtre principale
            MainWindow = mainView;
            
            // Changer le mode de fermeture pour que l'app se ferme quand MainWindow se ferme
           ShutdownMode = ShutdownMode.OnMainWindowClose;
            
            mainView.Show();
            LogStartup("MainView shown");
        }
        catch (Exception ex)
        {
            LogStartup($"ShowMainWindow failed: {ex}");
            MessageBox.Show($"Erreur lors de la création de la fenêtre principale:\n{ex.Message}\n\n{ex.InnerException?.Message}\n\nStack:\n{ex.StackTrace}", "Erreur Fenêtre Principale", MessageBoxButton.OK, MessageBoxImage.Error);
            //Shutdown();
        }
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddHttpClient();

        // Services - Singleton
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<ITokenStorageService, TokenStorageService>();
        services.AddSingleton<IAuthenticationService, AuthenticationService>();
        services.AddTransient<AuthHeaderHandler>();
        services
            .AddRefitClient<IAcadSignApi>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(AcadSign.Desktop.Properties.Settings.Default.ApiEndpoint))
            .AddHttpMessageHandler<AuthHeaderHandler>();
        services.AddSingleton<IApiClientService, RefitApiClientService>();
        services.AddSingleton<IDongleService, Pkcs11DongleService>();
        services.AddSingleton<ICertificateValidationService, CertificateValidationService>();
        services.AddSingleton<ISignatureService, PadesSignatureService>();
        services.AddSingleton<IBatchSigningService, BatchSigningService>();
        
        // ViewModels - Transient
        services.AddTransient<LoginViewModel>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<SigningViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<DongleStatusViewModel>();
        services.AddTransient<BatchSigningViewModel>();
        
        // Views
        services.AddTransient<MainView>(provider =>
        {
            var viewModel = provider.GetRequiredService<MainViewModel>();
            return new MainView { DataContext = viewModel };
        });

        services.AddTransient<SigningView>();
        services.AddTransient<BatchSigningView>();
    }

    public void ShowLoginWindow()
    {
        try
        {
            var authService = _serviceProvider!.GetRequiredService<IAuthenticationService>();
            var loginWindow = new LoginWindow(authService);
            
            // S'abonner à l'événement de connexion réussie
            loginWindow.LoginSuccess += async (sender, args) =>
            {
                try
                {
                    var tokenStorage = _serviceProvider.GetRequiredService<ITokenStorageService>();
                    if (args.RememberMe)
                    {
                        await tokenStorage.SaveTokensAsync(args.AccessToken, args.RefreshToken);
                    }
                    else
                    {
                        await tokenStorage.DeleteTokensAsync();
                    }
                }
                catch
                {
                }

                // Fermer la fenêtre de connexion
                loginWindow.Close();
                
                // Afficher la fenêtre principale
                ShowMainWindow();
            };
            
            loginWindow.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de la création de la fenêtre de connexion:\n{ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }

    private static void LogStartup(string message)
    {
        try
        {
            File.AppendAllText(StartupLogPath, $"[{DateTime.Now:O}] {message}{Environment.NewLine}");
        }
        catch
        {
        }
    }
}