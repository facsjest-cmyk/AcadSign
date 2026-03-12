using System.Linq;

namespace AcadSign.Desktop.Services.Navigation;

public interface INavigationService
{
    void NavigateTo<TViewModel>(object? parameter = null) where TViewModel : class;
    void GoBack();
}
