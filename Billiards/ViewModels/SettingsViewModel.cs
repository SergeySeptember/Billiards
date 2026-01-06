using System.Text;
using System.Text.Json;
using System.Windows.Input;
using Billiards.Abstractions;
using Billiards.ModelAndDto;
using CommunityToolkit.Maui.Storage;

namespace Billiards.ViewModels;

public class SettingsViewModel : BaseViewModel
{
    private const string ThemeKey = "theme";
    private const string SoundsKey = "sounds_enabled";

    private readonly IPlayersStore _playersStore;
    private readonly IMatchesStore _matchesStore;

    private bool _isDarkTheme;

    private readonly IDatabaseBackupService _backupService;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public ICommand ExportDataCommand { get; }
    public ICommand ImportDataCommand { get; }

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

    private bool _isSoundsEnabled;

    public bool IsSoundsEnabled
    {
        get => _isSoundsEnabled;
        set
        {
            if (SetProperty(ref _isSoundsEnabled, value))
            {
                Preferences.Default.Set(SoundsKey, value);
            }
        }
    }

    public ICommand AddPlayerCommand { get; }
    public ICommand DeletePlayerCommand { get; }
    public ICommand ClearDbCommand { get; }

    public ICommand OpenGithubCommand { get; }
    public ICommand OpenTelegramCommand { get; }
    public ICommand OpenRulesCommand { get; }

    public SettingsViewModel(IPlayersStore playersStore, IMatchesStore matchesStore, IDatabaseBackupService backupService)
    {
        _playersStore = playersStore;
        _matchesStore = matchesStore;
        _backupService = backupService;

        ExportDataCommand = new Command(async () => await ExportDataAsync());
        ImportDataCommand = new Command(async () => await ImportDataAsync());

        // загрузка настроек
        var savedTheme = Preferences.Default.Get(ThemeKey, "light");
        _isDarkTheme = savedTheme == "dark";

        _isSoundsEnabled = Preferences.Default.Get(SoundsKey, false);

        ApplyTheme();

        AddPlayerCommand = new Command(async () => await AddPlayerAsync());
        DeletePlayerCommand = new Command(async () => await DeletePlayerAsync());
        ClearDbCommand = new Command(async () => await ClearDbAsync());

        OpenGithubCommand = new Command(async () => await OpenUrlAsync("https://github.com/SergeySeptember"));
        OpenTelegramCommand = new Command(async () => await OpenUrlAsync("https://t.me/Sergey_September"));
        OpenRulesCommand = new Command(async () => await OpenUrlAsync("https://www.fbsrf.ru/sites/default/files/04-novaya_redakciya_pravil_piramidy_2025-09.pdf"));
    }

    private async Task ExportDataAsync()
    {
        try
        {
            var page = Shell.Current.CurrentPage;
            var backup = await _backupService.BuildBackupAsync();
            var json = JsonSerializer.Serialize(backup, JsonOptions);

            await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

            var suggestedName = $"billiards-backup-{DateTime.Now:yyyyMMdd-HHmmss}.json";

            var result = await FileSaver.Default.SaveAsync(suggestedName, stream, CancellationToken.None);

            if (result.IsSuccessful)
            {
                await page.DisplayAlert("Готово", "Бэкап сохранён.", "Ок");
            }
            else
            {
                await page.DisplayAlert("Не сохранилось", result.Exception?.Message ?? "Неизвестная ошибка", "Ок");
            }
        }
        catch (Exception ex)
        {
            await (Shell.Current?.CurrentPage?.DisplayAlert("Ошибка экспорта", ex.Message, "Ок") ?? Task.CompletedTask);
        }
    }

    private async Task ImportDataAsync()
    {
        try
        {
            var page = Shell.Current.CurrentPage;
            var file = await FilePicker.Default.PickAsync(new()
            {
                PickerTitle = "Выбери JSON-бэкап"
            });

            if (file is null)
            {
                return;
            }

            var confirm = await page.DisplayAlert(
                "Импорт данных",
                "Импорт удалит текущие данные и заменит их данными из файла. Продолжить?",
                "Да",
                "Отмена");

            if (!confirm)
            {
                return;
            }

            await using var readStream = await file.OpenReadAsync();
            using var reader = new StreamReader(readStream, Encoding.UTF8);
            var json = await reader.ReadToEndAsync();

            var backup = JsonSerializer.Deserialize<BilliardsBackupDto>(json, JsonOptions);
            if (backup is null)
            {
                await page.DisplayAlert("Ошибка", "Не смог прочитать файл бэкапа.", "Ок");
                return;
            }

            await _backupService.RestoreBackupAsync(backup);

            await page.DisplayAlert("Готово", "Данные загружены.", "Ок");

            await _matchesStore.ReloadAsync();
            await _playersStore.ReloadAsync();
        }
        catch (Exception ex)
        {
            await (Shell.Current.CurrentPage.DisplayAlert("Ошибка импорта", ex.Message, "Ок") ?? Task.CompletedTask);
        }
    }

    private void ApplyTheme()
    {
        var app = Application.Current;
        if (app is null)
        {
            return;
        }

        app.UserAppTheme = _isDarkTheme ? AppTheme.Dark : AppTheme.Light;
        Preferences.Default.Set(ThemeKey, _isDarkTheme ? "dark" : "light");
    }

    private async Task AddPlayerAsync()
    {
        var page = Shell.Current.CurrentPage;
        var name = await page.DisplayPromptAsync(
            "Новый игрок",
            "Введи имя игрока",
            "Добавить",
            "Отмена",
            "Иосиф Абрамов");

        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        name = name.Trim();
        await _playersStore.AddAsync(name);
        await page.DisplayAlert("Готово", $"Игрок добавлен: {name}", "Ок");
    }

    private async Task DeletePlayerAsync()
    {
        var page = Shell.Current.CurrentPage;
        var name = await page.DisplayPromptAsync(
            "Удалить игрока",
            "Введи имя игрока",
            "Удалить",
            "Отмена",
            "Иосиф Абрамов");
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        name = name.Trim();

        var result = await _playersStore.DeleteAsync(name);
        if (result)
        {
            await page.DisplayAlert("Готово", $"Игрок {name} удалён.", "Ок");
        }
        else
        {
            await page.DisplayAlert("Ой", $"Игрок {name} почему-то не удалился...", "Ок");
        }
    }

    private async Task ClearDbAsync()
    {
        var page = Shell.Current.CurrentPage;
        if (page is null)
        {
            return;
        }

        var confirm = await page.DisplayAlert(
            "Очистить БД",
            "Удалить всех игроков и всю статистику матчей?\nЭто действие нельзя отменить.",
            "Да, очистить",
            "Отмена");

        if (!confirm)
        {
            return;
        }

        await _matchesStore.DeleteAllAsync();
        await _playersStore.DeleteAllAsync();

        await page.DisplayAlert("Готово", "База очищена.", "Ок");
    }

    private static async Task OpenUrlAsync(string url)
    {
        try
        {
            await Launcher.Default.OpenAsync(new Uri(url));
        }
        catch
        {
            var page = Shell.Current.CurrentPage;
            if (page is not null)
            {
                await page.DisplayAlert("Ошибка", "Не удалось открыть ссылку.", "Ок");
            }
        }
    }
}