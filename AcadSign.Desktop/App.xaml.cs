using System.Windows;
using AcadSign.Desktop.ViewModels;
using AcadSign.Desktop.Views;
using AcadSign.Desktop.Services.Navigation;
using AcadSign.Desktop.Services.Authentication;
using AcadSign.Desktop.Services.Api;
using AcadSign.Desktop.Services.Signature;
using AcadSign.Desktop.Services.Dongle;
using AcadSign.Desktop.Services.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace AcadSign.Desktop;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        var navigationService = _serviceProvider.GetRequiredService<INavigationService>();
        navigationService.NavigateTo<LoginViewModel>();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Services - Singleton
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<ITokenStorageService, TokenStorageService>();
        services.AddSingleton<IAuthenticationService, AuthenticationService>();
        services.AddSingleton<IApiClientService, ApiClientService>();
        services.AddSingleton<ISignatureService, SignatureService>();
        services.AddSingleton<IDongleService, DongleService>();
        
        // ViewModels - Transient
        services.AddTransient<LoginViewModel>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<SigningViewModel>();
        services.AddTransient<SettingsViewModel>();
        
        // Views
        services.AddTransient<MainView>(provider =>
        {
            var viewModel = provider.GetRequiredService<MainViewModel>();
            return new MainView { DataContext = viewModel };
        });
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}