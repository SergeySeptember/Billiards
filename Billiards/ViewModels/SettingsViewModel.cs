namespace Billiards.ViewModels;

public class SettingsViewModel : BaseViewModel
{
    private bool _isDarkTheme;

    public bool IsDarkTheme
    {
        get => _isDarkTheme;
        set
        {
            if (SetProperty(ref _isDarkTheme, value))
            {
                ApplyTheme();
            }
        }
    }

    public SettingsViewModel()
    {
        // Загружаем сохранённое значение, по умолчанию светлая тема
        var saved = Preferences.Default.Get("theme", "light");
        _isDarkTheme = saved == "dark";

        ApplyTheme();
    }

    private void ApplyTheme()
    {
        var app = Application.Current;
        if (app is null)
        {
            return;
        }

        app.UserAppTheme = _isDarkTheme ? AppTheme.Dark : AppTheme.Light;

        Preferences.Default.Set("theme", _isDarkTheme ? "dark" : "light");
    }
}