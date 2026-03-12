using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace AcadSign.Desktop.Services.Navigation;

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private Window? _currentWindow;
    
    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public void NavigateTo<TViewModel>(object? parameter = null) where TViewModel : class
    {
        _currentWindow ??= Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
                          ?? Application.Current?.MainWindow;

        var previousWindow = _currentWindow;

        var viewModel = _serviceProvider.GetRequiredService<TViewModel>();
        
        var viewType = GetViewType(typeof(TViewModel));
        if (viewType == null)
        {
            throw new InvalidOperationException($"View not found for {typeof(TViewModel).Name}");
        }
        
        Window view;
        try
        {
            view = (Window)Activator.CreateInstance(viewType)!;
        }
        catch (Exception ex)
        {
            var details = ex.InnerException?.Message ?? ex.Message;
            throw new InvalidOperationException($"Failed to create view '{viewType.FullName}': {details}", ex);
        }
        view.DataContext = viewModel;
        
        if (Application.Current != null)
        {
            Application.Current.MainWindow = view;
        }

        _currentWindow = view;
        view.Show();

        if (previousWindow != null && !ReferenceEquals(previousWindow, view))
        {
            previousWindow.Close();
        }

        if (parameter != null && viewModel is INavigationAware aware)
        {
            aware.OnNavigatedTo(parameter);
        }
    }
    
    private Type? GetViewType(Type viewModelType)
    {
        var viewName = viewModelType.Name.Replace("ViewModel", "View");
        var viewTypeName = $"AcadSign.Desktop.Views.{viewName}";
        return Type.GetType(viewTypeName) ?? typeof(AcadSign.Desktop.App).Assembly.GetType(viewTypeName);
    }
    
    public void GoBack()
    {
        // Implement if needed
    }
}
