using System.Collections.ObjectModel;
using System.Windows.Input;
using Billiards.Core;
using Billiards.Core.Entities.DB;
using Microsoft.Maui.Storage;

namespace Billiards.ViewModels;

public class MatchViewModel : BaseViewModel
{
    private readonly MatchTimer _matchTimer = new();
    private readonly IDispatcherTimer _uiTimer;
    private const string PrefLastMatchDateKey = "last_match_date";

    // ----- Виды бильярда -----
    public ObservableCollection<string> GameTypes { get; } = new()
    {
        "Свободная пирамида",
        "Сибирская пирамида",
        "Московская пирамида",
        "Невская пирамида",
        "Бесконечная пирамида"
    };

    public ObservableCollection<Player> Players { get; } = new()
    {
        new() { Name = "Мешков Сергей" },
        new() { Name = "Ахмедулин Сергей" }
    };

    private bool _isNamesEditable = true;

    public bool IsNamesEditable
    {
        get => _isNamesEditable;
        set
        {
            if (SetProperty(ref _isNamesEditable, value))
            {
                OnPropertyChanged(nameof(IsBreakGestureEnabled));
            }
        }
    }

    // LongPress включаем только когда имена “заморожены”
    public bool IsBreakGestureEnabled => !IsNamesEditable;

    private string _selectedGameType;

    public string SelectedGameType
    {
        get => _selectedGameType;
        set => SetProperty(ref _selectedGameType, value);
    }

    // ----- Игроки -----
    private Player? _playerA;

    public Player? PlayerA
    {
        get => _playerA;
        set
        {
            if (SetProperty(ref _playerA, value))
            {
                RefreshBreakerCandidates();
                NormalizeBreakShot();
            }
        }
    }

    private Player? _playerB;

    public Player? PlayerB
    {
        get => _playerB;
        set
        {
            if (SetProperty(ref _playerB, value))
            {
                RefreshBreakerCandidates();
                NormalizeBreakShot();
            }
        }
    }

    // ----- Разбой: кто разбивает пирамиду -----
    public ObservableCollection<Player> BreakerCandidates { get; } = new();

    private Player? _breakerPlayer;

    public Player? BreakerPlayer
    {
        get => _breakerPlayer;
        set => SetProperty(ref _breakerPlayer, value);
    }

    // ----- “Забил с разбоя” (визуальный эффект + запись в MatchStats.BreakShotPlayer) -----
    private string? _breakShotPlayerName;

    public string? BreakShotPlayerName
    {
        get => _breakShotPlayerName;
        set
        {
            if (SetProperty(ref _breakShotPlayerName, value))
            {
                OnPropertyChanged(nameof(IsBreakShotA));
                OnPropertyChanged(nameof(IsBreakShotB));
            }
        }
    }

    public bool IsBreakShotA => !string.IsNullOrEmpty(BreakShotPlayerName) && BreakShotPlayerName == PlayerA?.Name;
    public bool IsBreakShotB => !string.IsNullOrEmpty(BreakShotPlayerName) && BreakShotPlayerName == PlayerB?.Name;

    // ----- Счётчики -----
    private int _mainBallsA;

    public int MainBallsA
    {
        get => _mainBallsA;
        set => SetProperty(ref _mainBallsA, value);
    }

    private int _mainBallsB;

    public int MainBallsB
    {
        get => _mainBallsB;
        set => SetProperty(ref _mainBallsB, value);
    }

    private int _accidentalBallsA;

    public int AccidentalBallsA
    {
        get => _accidentalBallsA;
        set => SetProperty(ref _accidentalBallsA, value);
    }

    private int _accidentalBallsB;

    public int AccidentalBallsB
    {
        get => _accidentalBallsB;
        set => SetProperty(ref _accidentalBallsB, value);
    }

    private int _foulsA;

    public int FoulsA
    {
        get => _foulsA;
        set => SetProperty(ref _foulsA, value);
    }

    private int _foulsB;

    public int FoulsB
    {
        get => _foulsB;
        set => SetProperty(ref _foulsB, value);
    }

    // ----- Таймер -----
    private string _timerText = "00:00:00";

    public string TimerText
    {
        get => _timerText;
        set => SetProperty(ref _timerText, value);
    }

    public string StartPauseButtonText =>
        !_matchTimer.IsRunning
            ? "Старт"
            : _matchTimer.IsPaused
                ? "Продолжить"
                : "Пауза";

    // ----- Команды -----
    public ICommand StartPauseCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand NewMatchCommand { get; }

    public ICommand MainBallsIncrementACommand { get; }
    public ICommand MainBallsDecrementACommand { get; }
    public ICommand MainBallsIncrementBCommand { get; }
    public ICommand MainBallsDecrementBCommand { get; }

    public ICommand AccidentalBallsIncrementACommand { get; }
    public ICommand AccidentalBallsDecrementACommand { get; }
    public ICommand AccidentalBallsIncrementBCommand { get; }
    public ICommand AccidentalBallsDecrementBCommand { get; }

    public ICommand FoulsIncrementACommand { get; }
    public ICommand FoulsDecrementACommand { get; }
    public ICommand FoulsIncrementBCommand { get; }
    public ICommand FoulsDecrementBCommand { get; }

    public ICommand ToggleBreakShotCommand { get; }
    public ICommand ClearBreakShotCommand { get; }

    public MatchViewModel()
    {
        _selectedGameType = GameTypes.First();

        var dispatcher = Application.Current?.Dispatcher
                         ?? throw new InvalidOperationException("Dispatcher not available");

        _uiTimer = dispatcher.CreateTimer();
        _uiTimer.Interval = TimeSpan.FromSeconds(1);
        _uiTimer.Tick += (_, _) => UpdateTimerText();

        StartPauseCommand = new Command(async () => await StartPauseAsync());
        StopCommand = new Command(Stop);
        NewMatchCommand = new Command(async () => await NewMatchAsync());

        ToggleBreakShotCommand = new Command<Player?>(ToggleBreakShot);
        ClearBreakShotCommand = new Command(() => BreakShotPlayerName = null);

        MainBallsIncrementACommand = new Command(() => MainBallsA++);
        MainBallsDecrementACommand = new Command(() => MainBallsA--);
        MainBallsIncrementBCommand = new Command(() => MainBallsB++);
        MainBallsDecrementBCommand = new Command(() => MainBallsB--);

        AccidentalBallsIncrementACommand = new Command(() => AccidentalBallsA++);
        AccidentalBallsDecrementACommand = new Command(() => AccidentalBallsA--);
        AccidentalBallsIncrementBCommand = new Command(() => AccidentalBallsB++);
        AccidentalBallsDecrementBCommand = new Command(() => AccidentalBallsB--);

        FoulsIncrementACommand = new Command(() => FoulsA++);
        FoulsDecrementACommand = new Command(() => FoulsA--);
        FoulsIncrementBCommand = new Command(() => FoulsB++);
        FoulsDecrementBCommand = new Command(() => FoulsB--);

        // на старте — кандидаты пустые, заполнятся когда выберешь игроков
        RefreshBreakerCandidates();
    }

    private async Task StartPauseAsync()
    {
        if (!_matchTimer.IsRunning)
        {
            BreakShotPlayerName = null;
            var page = GetCurrentPage();

            if (PlayerA is null || PlayerB is null)
            {
                _ = page.DisplayAlert("Ошибка", "Выбери игроков!", "Ок");
                return;
            }

            if (PlayerA.Name == PlayerB.Name)
            {
                _ = page.DisplayAlert("Ошибка", "Выбери разных игроков!", "Ок");
                return;
            }

            RefreshBreakerCandidates();
            
            if (!IsFirstMatchToday())
            {
                var a = BreakerCandidates[0].Name;
                var b = BreakerCandidates[1].Name;

                var choice = await page.DisplayActionSheet(
                    "Кто разбивает пирамиду?",
                    "Отмена",
                    null,
                    a, b);

                if (choice == "Отмена")
                {
                    return;
                }

                BreakerPlayer = BreakerCandidates.FirstOrDefault(p => p.Name == choice);
            }
            else
            {
                BreakerPlayer ??= BreakerCandidates.FirstOrDefault();
            }

            // фиксируем “сегодня уже была партия” именно на старте
            MarkMatchHappenedToday();

            _matchTimer.Start();
            IsNamesEditable = false;

            _uiTimer.Start();
            UpdateTimerText();
            OnPropertyChanged(nameof(StartPauseButtonText));
            return;
        }

        if (_matchTimer.IsPaused)
        {
            _matchTimer.Resume();
            _uiTimer.Start();
        }
        else
        {
            _matchTimer.Pause();
            _uiTimer.Stop();
        }

        UpdateTimerText();
        OnPropertyChanged(nameof(StartPauseButtonText));
    }

    private void Stop()
    {
        _matchTimer.Stop();
        _uiTimer.Stop();
        UpdateTimerText();

        IsNamesEditable = true;

        OnPropertyChanged(nameof(StartPauseButtonText));
    }

    private async Task NewMatchAsync()
    {
        Stop();

        var page = GetCurrentPage();
        var hasActivity = TimerText != "00:00:00";

        if (hasActivity && page is not null)
        {
            var save = await page.DisplayAlert(
                "Новая партия",
                "Сохранить статистику текущей партии перед началом новой?",
                "Сохранить",
                "Не сохранять");

            if (save)
            {
                var ok = await TryBuildAndSaveMatchAsync();
                if (!ok)
                {
                    // если не сохранилось — не начинаем новую, чтобы не потерять данные
                    return;
                }
            }

            // по требованию: каждый новый матч — меняем разбивающего
            ToggleBreakerForNextMatch();
            BreakShotPlayerName = null;
        }

        ResetMatchState();
    }

    private async Task<bool> TryBuildAndSaveMatchAsync()
    {
        var page = GetCurrentPage();
        MatchStats matchStats = new()
        {
            CurrentDateTime = DateTime.Now,
            MatchTime = TimerText,
            GameTypes = SelectedGameType,
            BreakShotPlayer = BreakShotPlayerName
        };

        if (MainBallsA >= 8)
        {
            matchStats.WinnerPlayer = PlayerA!.Name;
            matchStats.LosePlayer = PlayerB!.Name;

            matchStats.BallsWinnerPlayer = MainBallsA;
            matchStats.BallsLosePlayer = MainBallsB;

            matchStats.AccidentalBallsWinnerPlayer = AccidentalBallsA;
            matchStats.AccidentalBallsLosePlayer = AccidentalBallsB;

            matchStats.FoulsBallsWinnerPlayer = FoulsA;
            matchStats.FoulsBallsLosePlayer = FoulsB;
        }
        else if (MainBallsB >= 8)
        {
            matchStats.WinnerPlayer = PlayerB!.Name;
            matchStats.LosePlayer = PlayerA!.Name;

            matchStats.BallsWinnerPlayer = MainBallsB;
            matchStats.BallsLosePlayer = MainBallsA;

            matchStats.AccidentalBallsWinnerPlayer = AccidentalBallsB;
            matchStats.AccidentalBallsLosePlayer = AccidentalBallsA;

            matchStats.FoulsBallsWinnerPlayer = FoulsB;
            matchStats.FoulsBallsLosePlayer = FoulsA;
        }
        else
        {
            _ = page.DisplayAlert("Не сохраняю", $"Победитель не определён (нужно 8+ шаров).", "Ок");
            return false;
        }

        // TODO: здесь будет сохранение в БД
        // await _matchRepository.AddAsync(matchStats);
        // Пока заглушка:
        await Task.CompletedTask;

        return true;
    }

    private void ResetMatchState()
    {
        _matchTimer.Reset();
        _uiTimer.Stop();

        TimerText = "00:00:00";
        IsNamesEditable = true;

        MainBallsA = 0;
        MainBallsB = 0;
        AccidentalBallsA = 0;
        AccidentalBallsB = 0;
        FoulsA = 0;
        FoulsB = 0;
        BreakShotPlayerName = null;

        OnPropertyChanged(nameof(StartPauseButtonText));
    }

    private void UpdateTimerText()
    {
        var elapsed = _matchTimer.GetElapsed(DateTime.UtcNow);
        TimerText = $"{(int)elapsed.TotalHours:00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}";
    }

    private void ToggleBreakShot(Player? player)
    {
        if (player?.Name is null)
        {
            return;
        }

        BreakShotPlayerName = BreakShotPlayerName == player.Name ? null : player.Name;
    }

    private void NormalizeBreakShot()
    {
        if (BreakShotPlayerName is null)
        {
            OnPropertyChanged(nameof(IsBreakShotA));
            OnPropertyChanged(nameof(IsBreakShotB));
            return;
        }

        var a = PlayerA?.Name;
        var b = PlayerB?.Name;

        if (BreakShotPlayerName != a && BreakShotPlayerName != b)
        {
            BreakShotPlayerName = null;
        }
        else
        {
            OnPropertyChanged(nameof(IsBreakShotA));
            OnPropertyChanged(nameof(IsBreakShotB));
        }
    }

    private void RefreshBreakerCandidates()
    {
        BreakerCandidates.Clear();

        if (PlayerA is not null)
        {
            BreakerCandidates.Add(PlayerA);
        }

        if (PlayerB is not null && PlayerB != PlayerA)
        {
            BreakerCandidates.Add(PlayerB);
        }

        if (BreakerPlayer is null || !BreakerCandidates.Contains(BreakerPlayer))
        {
            BreakerPlayer = BreakerCandidates.FirstOrDefault();
        }
    }

    private void ToggleBreakerForNextMatch()
    {
        if (PlayerA is null || PlayerB is null)
        {
            return;
        }

        if (BreakerPlayer is null)
        {
            BreakerPlayer = PlayerA;
            return;
        }

        BreakerPlayer = BreakerPlayer.Name == PlayerA.Name ? PlayerB : PlayerA;
    }

    private static bool IsFirstMatchToday()
    {
        // Todo: брать данные из БД
        var last = Preferences.Default.Get(PrefLastMatchDateKey, "");
        var today = DateTime.Today.ToString("yyyy-MM-dd");
        return last != today;
    }

    private static void MarkMatchHappenedToday()
    {
        Preferences.Default.Set(PrefLastMatchDateKey, DateTime.Today.ToString("yyyy-MM-dd"));
    }

    private static Page GetCurrentPage()
    {
        if (Shell.Current?.CurrentPage is { } shellPage)
        {
            return shellPage;
        }

        return Application.Current?.Windows.FirstOrDefault()?.Page;
    }
}