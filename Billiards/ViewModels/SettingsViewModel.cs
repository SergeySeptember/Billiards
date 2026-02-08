using System.Text;
using System.Text.Json;
using System.Windows.Input;
using Billiards.Abstractions;
using Billiards.Enum;
using Billiards.ModelAndDto;
using Billiards.Utils;
using CommunityToolkit.Maui.Storage;

namespace Billiards.ViewModels;

public class SettingsViewModel : BaseViewModel
{
    private readonly IPlayersStore _playersStore;
    private readonly IMatchesStore _matchesStore;

    private bool _isDarkTheme;

    private readonly IDatabaseBackupService _backupService;
    private readonly ISoundService _soundService;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

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
            Preferences.Default.Set(Const.SoundsKey, value);
            SetProperty(ref _isSoundsEnabled, value);
        }
    }

    private bool _isNegativeScore;
    public bool IsNegativeScore
    {
        get => _isNegativeScore;
        set
        {
            if (SetProperty(ref _isNegativeScore, value))
            {
                Preferences.Default.Set(Const.NegativeScore, value.ToString());
            }
        }
    }

    private bool _minusRandomBalls;
    public bool MinusRandomBalls
    {
        get => _minusRandomBalls;
        set
        {
            if (SetProperty(ref _minusRandomBalls, value))
            {
                Preferences.Default.Set(Const.MinusRandomBalls, value.ToString());
                _matchesStore.ReloadAsync();
            }
        }
    }

    private string _foulMode = Const.ModeShelf;
    private bool _guard;

    public bool IsFoulToShelf
    {
        get => _foulMode == Const.ModeShelf;
        set
        {
            if (_guard)
            {
                return;
            }

            if (value)
            {
                SetFoulMode(Const.ModeShelf);
                return;
            }

            if (_foulMode == Const.ModeShelf)
            {
                SetFoulMode(Const.ModeTable);
            }
        }
    }

    public bool IsFoulToTable
    {
        get => _foulMode == Const.ModeTable;
        set
        {
            if (_guard)
            {
                return;
            }

            if (value)
            {
                SetFoulMode(Const.ModeTable);
                return;
            }

            if (_foulMode == Const.ModeTable)
            {
                SetFoulMode(Const.ModeShelf);
            }
        }
    }

    public ICommand ExportDataCommand { get; }
    public ICommand ImportDataCommand { get; }

    public ICommand AddPlayerCommand { get; }
    public ICommand DeletePlayerCommand { get; }
    public ICommand ClearDbCommand { get; }

    public ICommand OpenGithubCommand { get; }
    public ICommand OpenTelegramCommand { get; }
    public ICommand OpenRulesCommand { get; }

    public SettingsViewModel(IPlayersStore playersStore, IMatchesStore matchesStore, IDatabaseBackupService backupService, ISoundService soundService)
    {
        _playersStore = playersStore;
        _matchesStore = matchesStore;
        _backupService = backupService;
        _soundService = soundService;

        ExportDataCommand = new Command(async () => await ExportDataAsync());
        ImportDataCommand = new Command(async () => await ImportDataAsync());

        _isSoundsEnabled = Preferences.Default.Get(Const.SoundsKey, false);

        AddPlayerCommand = new Command(async () => await AddPlayerAsync());
        DeletePlayerCommand = new Command(async () => await DeletePlayerAsync());
        ClearDbCommand = new Command(async () => await ClearDbAsync());

        OpenGithubCommand = new Command(async () => await OpenUrlAsync("https://github.com/SergeySeptember"));
        OpenTelegramCommand = new Command(async () => await OpenUrlAsync("https://t.me/Sergey_September"));
        OpenRulesCommand = new Command(async () => await OpenUrlAsync("https://www.fbsrf.ru/sites/default/files/04-novaya_redakciya_pravil_piramidy_2025-09.pdf"));

        LoadFoulModeSettings();
        LoadMinusRandomBallsSettings();
    }

    private void SetFoulMode(string mode)
    {
        if (_foulMode == mode)
        {
            return;
        }

        _foulMode = mode;

        _guard = true;
        OnPropertyChanged(nameof(IsFoulToShelf));
        OnPropertyChanged(nameof(IsFoulToTable));
        _guard = false;

        Preferences.Default.Set(Const.FoulModeKey, _foulMode);
    }

    private void LoadMinusRandomBallsSettings()
    {
        var saved = Preferences.Default.Get(Const.MinusRandomBalls, "false");
        MinusRandomBalls = saved == "true";

        OnPropertyChanged(nameof(MinusRandomBalls));
    }

    private void LoadFoulModeSettings()
    {
        var saved = Preferences.Default.Get(Const.FoulModeKey, Const.ModeShelf);
        _foulMode = saved == Const.ModeTable ? Const.ModeTable : Const.ModeShelf;

        OnPropertyChanged(nameof(IsFoulToShelf));
        OnPropertyChanged(nameof(IsFoulToTable));
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
            await (Shell.Current.CurrentPage.DisplayAlert("Ошибка экспорта", ex.Message, "Ок") ?? Task.CompletedTask);
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
        Preferences.Default.Set(Const.ThemeKey, _isDarkTheme ? "dark" : "light");
    }

    public void SyncThemeWithSystemIfNotSet()
    {
        var app = Application.Current;
        if (app is null)
        {
            return;
        }

        if (Preferences.Default.ContainsKey(Const.ThemeKey))
        {
            IsDarkTheme = Preferences.Default.Get(Const.ThemeKey, "light") == "dark";
            app.UserAppTheme = IsDarkTheme ? AppTheme.Dark : AppTheme.Light;
        }
        else
        {
            IsDarkTheme = app.RequestedTheme == AppTheme.Dark;
            OnPropertyChanged(nameof(IsDarkTheme));
            app.UserAppTheme = AppTheme.Unspecified;
        }
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
        _soundService.PlayAsync(SoundId.FreshMeat);
        await page.DisplayAlert("Готово", $"Игрок добавлен: {name}", "Ок");
    }

    private async Task DeletePlayerAsync()
    {
        var page = Shell.Current.CurrentPage;

        var names = _playersStore.Players
            .Select(p => p.Name)
            .ToArray();
        if (names.Length == 0)
        {
            await page.DisplayAlert("Пусто", "Удалять некого — список игроков пуст.", "Ок");
            return;
        }

        var selected = await page.DisplayActionSheet(
            "Удалить игрока",
            "Отмена",
            null,
            names);
        if (string.IsNullOrWhiteSpace(selected) || selected == "Отмена")
        {
            return;
        }

        var confirm = await page.DisplayAlert(
            "Подтверди удаление",
            $"Удалить игрока «{selected}» и ВСЕ партии, где он участвовал?",
            "Удалить",
            "Отмена");

        if (!confirm)
        {
            return;
        }

        await _matchesStore.DeleteByPlayerAsync(selected);
        var deletedPlayer = await _playersStore.DeleteAsync(selected);

        if (deletedPlayer)
        {
            await page.DisplayAlert(
                "Готово",
                $"Игрок удалён: {selected}",
                "Ок");
        }
        else
        {
            await page.DisplayAlert("Ой", $"Игрок «{selected}» почему-то не удалился…", "Ок");
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