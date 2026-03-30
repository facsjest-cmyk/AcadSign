using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AcadSign.Desktop.ViewModels
{
    public class ThemeViewModel : INotifyPropertyChanged
    {
        private readonly Services.ThemeService _themeService;
        private Services.ThemeType _currentTheme;

        public Services.ThemeType CurrentTheme
        {
            get => _currentTheme;
            set
            {
                if (_currentTheme != value)
                {
                    _currentTheme = value;
                    OnPropertyChanged();
                    _themeService.SetTheme(_currentTheme);
                }
            }
        }

        public ThemeViewModel()
        {
            _themeService = Services.ThemeService.Instance;
            _currentTheme = _themeService.CurrentTheme;
            _themeService.ThemeChanged += (sender, theme) =>
            {
                CurrentTheme = theme;
            };
        }

        public void ToggleTheme()
        {
            _themeService.ToggleTheme();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
