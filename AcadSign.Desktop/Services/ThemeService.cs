using System;

namespace AcadSign.Desktop.Services
{
    public enum ThemeType
    {
        Light,
        Dark
    }

    public sealed class ThemeService
    {
        private static readonly Lazy<ThemeService> _instance = new(() => new ThemeService());
        public static ThemeService Instance => _instance.Value;

        public event EventHandler<ThemeType>? ThemeChanged;

        public ThemeType CurrentTheme { get; private set; } = ThemeType.Light;

        private ThemeService()
        {
        }

        public void SetTheme(ThemeType theme)
        {
            if (CurrentTheme == theme)
                return;

            CurrentTheme = theme;
            ThemeChanged?.Invoke(this, CurrentTheme);
        }

        public void ToggleTheme()
        {
            var next = CurrentTheme == ThemeType.Light ? ThemeType.Dark : ThemeType.Light;
            SetTheme(next);
        }
    }
}
