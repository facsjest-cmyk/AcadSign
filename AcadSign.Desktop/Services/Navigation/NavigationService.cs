using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

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
        var viewModel = _serviceProvider.GetRequiredService<TViewModel>();
        
        var viewType = GetViewType(typeof(TViewModel));
        if (viewType == null)
        {
            throw new InvalidOperationException($"View not found for {typeof(TViewModel).Name}");
        }
        
        var view = (Window)Activator.CreateInstance(viewType)!;
        view.DataContext = viewModel;
        
        if (parameter != null && viewModel is INavigationAware aware)
        {
            aware.OnNavigatedTo(parameter);
        }
        
        _currentWindow?.Close();
        _currentWindow = view;
        view.Show();
    }
    
    private Type? GetViewType(Type viewModelType)
    {
        var viewName = viewModelType.Name.Replace("ViewModel", "View");
        var viewTypeName = $"AcadSign.Desktop.Views.{viewName}";
        return Type.GetType(viewTypeName);
    }
    
    public void GoBack()
    {
        // Implement if needed
    }
}
