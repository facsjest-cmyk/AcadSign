using System.Windows;
using AcadSign.Services;
using AcadSign.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace AcadSign;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();

        // Services
        services.AddHttpClient<ISisApiService, SisApiService>(client =>
        {
            client.BaseAddress = new Uri("https://sis.uh2.ac.ma/api/");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddSingleton<IESignService, ESignService>();
        services.AddSingleton<IS3StorageService, S3StorageService>();
        services.AddSingleton<IEmailService, EmailService>();
        services.AddSingleton<IPdfGeneratorService, PdfGeneratorService>();
        services.AddSingleton<IPdfViewerService, PdfViewerService>();

        // ViewModels
        services.AddSingleton<MainViewModel>();
        services.AddTransient<DocumentListViewModel>();
        services.AddTransient<PdfViewerViewModel>();
        services.AddTransient<SignatureViewModel>();
        services.AddTransient<BatchSignViewModel>();
        services.AddTransient<EmailViewModel>();

        Services = services.BuildServiceProvider();

        // Show main window
        var mainWindow = new Views.MainWindow
        {
            DataContext = Services.GetRequiredService<MainViewModel>()
        };
        mainWindow.Show();
    }
}
