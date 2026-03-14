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
    private readonly IAppPreferences _appPreferences;
    private readonly IAppDialogService _appDialogService;
    private readonly IExternalLinkService _externalLinkService;

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
            if (SetProperty(ref _isSoundsEnabled, value))
            {
                _appPreferences.SetBoolean(Const.SoundsKey, value);
            }
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
                _appPreferences.SetBoolean(Const.NegativeScore, value);
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
                _appPreferences.SetBoolean(Const.MinusRandomBalls, value);
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

    public SettingsViewModel(
        IPlayersStore playersStore,
        IMatchesStore matchesStore,
        IDatabaseBackupService backupService,
        ISoundService soundService,
        IAppPreferences appPreferences,
        IAppDialogService appDialogService,
        IExternalLinkService externalLinkService)
    {
        _playersStore = playersStore;
        _matchesStore = matchesStore;
        _backupService = backupService;
        _soundService = soundService;
        _appPreferences = appPreferences;
        _appDialogService = appDialogService;
        _externalLinkService = externalLinkService;

        ExportDataCommand = new Command(async () => await ExportDataAsync());
        ImportDataCommand = new Command(async () => await ImportDataAsync());

        _isSoundsEnabled = _appPreferences.GetBoolean(Const.SoundsKey, false);

        AddPlayerCommand = new Command(async () => await AddPlayerAsync());
        DeletePlayerCommand = new Command(async () => await DeletePlayerAsync());
        ClearDbCommand = new Command(async () => await ClearDbAsync());

        OpenGithubCommand = new Command(async () => await OpenUrlAsync("https://github.com/SergeySeptember"));
        OpenTelegramCommand = new Command(async () => await OpenUrlAsync("https://t.me/Sergey_September"));
        OpenRulesCommand = new Command(async () => await OpenUrlAsync("https://www.fbsrf.ru/sites/default/files/04-novaya_redakciya_pravil_piramidy_2025-09.pdf"));

        LoadFoulModeSettings();
        LoadNegativeScoreSettings();
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

        _appPreferences.SetString(Const.FoulModeKey, _foulMode);
    }

    private void LoadMinusRandomBallsSettings()
    {
        _minusRandomBalls = _appPreferences.GetBoolean(Const.MinusRandomBalls, false);
        OnPropertyChanged(nameof(MinusRandomBalls));
    }

    private void LoadNegativeScoreSettings()
    {
        _isNegativeScore = _appPreferences.GetBoolean(Const.NegativeScore, false);
        OnPropertyChanged(nameof(IsNegativeScore));
    }

    private void LoadFoulModeSettings()
    {
        var saved = _appPreferences.GetString(Const.FoulModeKey, Const.ModeShelf);
        _foulMode = saved == Const.ModeTable ? Const.ModeTable : Const.ModeShelf;

        OnPropertyChanged(nameof(IsFoulToShelf));
        OnPropertyChanged(nameof(IsFoulToTable));
    }

    private async Task ExportDataAsync()
    {
        try
        {
            var backup = await _backupService.BuildBackupAsync();
            var json = JsonSerializer.Serialize(backup, JsonOptions);

            await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

            var suggestedName = $"billiards-backup-{DateTime.Now:yyyyMMdd-HHmmss}.json";

            var result = await FileSaver.Default.SaveAsync(suggestedName, stream, CancellationToken.None);

            if (result.IsSuccessful)
            {
                await _appDialogService.ShowMessageAsync("Готово", "Бэкап сохранён.", "Ок");
            }
            else
            {
                await _appDialogService.ShowMessageAsync("Не сохранилось", result.Exception?.Message ?? "Неизвестная ошибка", "Ок");
            }
        }
        catch (Exception ex)
        {
            await _appDialogService.ShowMessageAsync("Ошибка экспорта", ex.Message, "Ок");
        }
    }

    private async Task ImportDataAsync()
    {
        try
        {
            var file = await FilePicker.Default.PickAsync(new()
            {
                PickerTitle = "Выбери JSON-бэкап"
            });

            if (file is null)
            {
                return;
            }

            var confirm = await _appDialogService.ShowConfirmationAsync(
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
                await _appDialogService.ShowMessageAsync("Ошибка", "Не смог прочитать файл бэкапа.", "Ок");
                return;
            }

            await _backupService.RestoreBackupAsync(backup);

            await _appDialogService.ShowMessageAsync("Готово", "Данные загружены.", "Ок");

            await _matchesStore.ReloadAsync();
            await _playersStore.ReloadAsync();
        }
        catch (Exception ex)
        {
            await _appDialogService.ShowMessageAsync("Ошибка импорта", ex.Message, "Ок");
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
        _appPreferences.SetString(Const.ThemeKey, _isDarkTheme ? "dark" : "light");
    }

    public void SyncThemeWithSystemIfNotSet()
    {
        var app = Application.Current;
        if (app is null)
        {
            return;
        }

        if (_appPreferences.Contains(Const.ThemeKey))
        {
            IsDarkTheme = _appPreferences.GetString(Const.ThemeKey, "light") == "dark";
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
        var name = await _appDialogService.ShowPromptAsync(
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
        await _soundService.PlayAsync(SoundId.FreshMeat);
        await _appDialogService.ShowMessageAsync("Готово", $"Игрок добавлен: {name}", "Ок");
    }

    private async Task DeletePlayerAsync()
    {
        var names = _playersStore.Players
            .Select(p => p.Name)
            .ToArray();
        if (names.Length == 0)
        {
            await _appDialogService.ShowMessageAsync("Пусто", "Удалять некого — список игроков пуст.", "Ок");
            return;
        }

        var selected = await _appDialogService.ShowSelectionAsync(
            "Удалить игрока",
            names,
            "Отмена");
        if (string.IsNullOrWhiteSpace(selected))
        {
            return;
        }

        var confirm = await _appDialogService.ShowConfirmationAsync(
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
            await _appDialogService.ShowMessageAsync(
                "Готово",
                $"Игрок удалён: {selected}",
                "Ок");
        }
        else
        {
            await _appDialogService.ShowMessageAsync("Ой", $"Игрок «{selected}» почему-то не удалился…", "Ок");
        }
    }

    private async Task ClearDbAsync()
    {
        var confirm = await _appDialogService.ShowConfirmationAsync(
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

        await _appDialogService.ShowMessageAsync("Готово", "База очищена.", "Ок");
    }

    private async Task OpenUrlAsync(string url)
    {
        if (!await _externalLinkService.OpenUrlAsync(url))
        {
            await _appDialogService.ShowMessageAsync("Ошибка", "Не удалось открыть ссылку.", "Ок");
        }
    }
}
