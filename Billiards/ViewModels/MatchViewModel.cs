using System.Collections.ObjectModel;
using System.Windows.Input;
using Billiards.Abstractions;
using Billiards.Core;
using Billiards.DataBase.Entities;
using Billiards.Enum;
using Billiards.Utils;

namespace Billiards.ViewModels;

public class MatchViewModel : BaseViewModel
{
    private readonly MatchTimer _matchTimer = new();
    private readonly IDispatcherTimer _uiTimer;
    private readonly IPlayersStore _playersStore;
    private readonly IMatchesStore _matchesStore;
    private readonly ISoundService _soundService;
    private readonly IAppPreferences _appPreferences;
    private readonly IAppDialogService _appDialogService;

    // ----- Виды бильярда -----
    public ObservableCollection<string> GameTypes { get; } = new()
    {
        "Свободная пирамида",
        "Сибирская пирамида",
        "Московская пирамида",
        "Невская пирамида",
        "Бесконечная пирамида"
    };

    public bool IsEditable
    {
        get;
        set => SetProperty(ref field, value);
    } = true;

    public string SelectedGameType
    {
        get;
        set => SetProperty(ref field, value);
    }

    // ----- Игроки -----
    public ObservableCollection<Player> Players => _playersStore.Players;

    public Player? PlayerA
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                RefreshBreakerCandidates();
            }
        }
    }

    public Player? PlayerB
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                RefreshBreakerCandidates();
            }
        }
    }

    public ObservableCollection<Player> BreakerCandidates { get; } = new();

    public Player? BreakerPlayer
    {
        get;
        set => SetProperty(ref field, value);
    }

    public string? BreakShotPlayerName
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                OnPropertyChanged(nameof(IsBreakShotA));
                OnPropertyChanged(nameof(IsBreakShotB));
            }
        }
    }

    public bool IsBreakShotA => !string.IsNullOrEmpty(BreakShotPlayerName) && BreakShotPlayerName == PlayerA?.Name;
    public bool IsBreakShotB => !string.IsNullOrEmpty(BreakShotPlayerName) && BreakShotPlayerName == PlayerB?.Name;

    // ----- Счётчики -----
    public int MainBallsA
    {
        get;
        set
        {
            if (!_appPreferences.GetBoolean(Const.NegativeScore, false) && value < 0)
            {
                return;
            }
            SetProperty(ref field, value);
        }
    }

    public int MainBallsB
    {
        get;
        set
        {
            if (!_appPreferences.GetBoolean(Const.NegativeScore, false) && value < 0)
            {
                return;
            }
            SetProperty(ref field, value);
        }
    }

    public int AccidentalBallsA
    {
        get;
        set
        {
            if (value >= 0)
            {
                SetProperty(ref field, value);
            }
        }
    }

    public int AccidentalBallsB
    {
        get;
        set
        {
            if (value >= 0)
            {
                SetProperty(ref field, value);
            }
        }
    }

    public int FoulsA
    {
        get;
        set
        {
            if (value >= 0)
            {
                SetProperty(ref field, value);
            }
        }
    }

    public int FoulsB
    {
        get;
        set
        {
            if (value >= 0)
            {
                SetProperty(ref field, value);
            }
        }
    }

    // ----- Таймер -----
    public string TimerText
    {
        get;
        set => SetProperty(ref field, value);
    } = "00:00:00";

    public string StartStopButtonText =>
        !_matchTimer.IsRunning
            ? "Старт"
            : _matchTimer.IsPaused
                ? "Продолжить"
                : "Стоп";

    // ----- Команды -----
    public ICommand StartStopCommand { get; }
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

    public MatchViewModel(
        IDispatcher dispatcher,
        IPlayersStore playersStore,
        IMatchesStore matchesStore,
        ISoundService soundService,
        IAppPreferences appPreferences,
        IAppDialogService appDialogService)
    {
        _playersStore = playersStore;
        _matchesStore = matchesStore;
        _soundService = soundService;
        _appPreferences = appPreferences;
        _appDialogService = appDialogService;

        SelectedGameType = GameTypes.First();

        _uiTimer = dispatcher.CreateTimer();
        _uiTimer.Interval = TimeSpan.FromSeconds(1);
        _uiTimer.Tick += (_, _) => UpdateTimerText();

        StartStopCommand = new Command(StartStop);
        NewMatchCommand = new Command(async () => await NewMatchAsync());

        ToggleBreakShotCommand = new Command<Player?>(ToggleBreakShot);
        ClearBreakShotCommand = new Command(() => BreakShotPlayerName = null);

        MainBallsIncrementACommand = new Command(() =>
        {
            MainBallsA++;
            soundService.PlayMainBallsIncrementAsync();
        });
        MainBallsDecrementACommand = new Command(() => MainBallsA--);
        MainBallsIncrementBCommand = new Command(() =>
        {
            MainBallsB++;
            soundService.PlayMainBallsIncrementAsync();
        });
        MainBallsDecrementBCommand = new Command(() => MainBallsB--);


        AccidentalBallsIncrementACommand = new Command(() =>
        {
            AccidentalBallsA++;
            MainBallsA++;
            soundService.PlayAccidentalIncrementAsync();
        });
        AccidentalBallsIncrementBCommand = new Command(() =>
        {
            AccidentalBallsB++;
            MainBallsB++;
            soundService.PlayAccidentalIncrementAsync();
        });
        AccidentalBallsDecrementACommand = new Command(() =>
        {
            if (AccidentalBallsA > 0)
            {
                AccidentalBallsA--;
                MainBallsA--;
            }
        });
        AccidentalBallsDecrementBCommand = new Command(() =>
        {
            if (AccidentalBallsB > 0)
            {
                AccidentalBallsB--;
                MainBallsB--;
            }
        });

        FoulsIncrementACommand = new Command(() =>
        {
            FoulsA++;
            var foulMode = _appPreferences.GetString(Const.FoulModeKey, Const.ModeShelf);
            if (foulMode == Const.ModeShelf)
            {
                MainBallsB++;
            }
            else
            {
                MainBallsA--;
            }
            soundService.PlayFoulsIncrementAsync();
        });
        FoulsIncrementBCommand = new Command(() =>
        {
            FoulsB++;
            var foulMode = _appPreferences.GetString(Const.FoulModeKey, Const.ModeShelf);
            if (foulMode == Const.ModeShelf)
            {
                MainBallsA++;
            }
            else
            {
                MainBallsB--;
            }
            soundService.PlayFoulsIncrementAsync();
        });
        FoulsDecrementACommand = new Command(() =>
        {
            FoulsA--;
            var foulMode = _appPreferences.GetString(Const.FoulModeKey, Const.ModeShelf);
            if (foulMode == Const.ModeShelf)
            {
                MainBallsB--;
            }
            else
            {
                MainBallsA++;
            }
        });
        FoulsDecrementBCommand = new Command(() =>
        {
            FoulsB--;
            var foulMode = _appPreferences.GetString(Const.FoulModeKey, Const.ModeShelf);
            if (foulMode == Const.ModeShelf)
            {
                MainBallsA--;
            }
            else
            {
                MainBallsB++;
            }
        });
    }

    private void StartStop()
    {
        if (!_matchTimer.IsRunning)
        {
            if (!ValidatePlayers())
            {
                return;
            }

            _matchTimer.Start();
            _uiTimer.Start();
            UpdateTimerText();

            IsEditable = false;

            OnPropertyChanged(nameof(StartStopButtonText));
            _soundService.PlayStartButtonAsync();
            return;
        }

        if (!_matchTimer.IsPaused)
        {
            _matchTimer.Pause();
            _uiTimer.Stop();
            UpdateTimerText();

            IsEditable = true;

            OnPropertyChanged(nameof(StartStopButtonText));
            return;
        }

        if (!ValidatePlayers())
        {
            return;
        }

        _matchTimer.Resume();
        _uiTimer.Start();
        UpdateTimerText();

        IsEditable = false;

        OnPropertyChanged(nameof(StartStopButtonText));
    }

    private void Stop()
    {
        _matchTimer.Stop();
        _uiTimer.Stop();
        UpdateTimerText();

        IsEditable = true;

        OnPropertyChanged(nameof(StartStopButtonText));
    }

    private async Task NewMatchAsync()
    {
        Stop();

        var hasActivity = TimerText != "00:00:00";

        if (hasActivity)
        {
            var save = await _appDialogService.ShowConfirmationAsync(
                "Новая партия",
                "Сохранить статистику текущей партии перед началом новой?",
                "Сохранить",
                "Не сохранять");

            if (save)
            {
                var ok = await SaveMatchAsync();
                if (!ok)
                {
                    return;
                }
            }
        }

        // Сбрасываем результат партии
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

        _matchTimer.Reset();
        _uiTimer.Stop();
        TimerText = "00:00:00";

        IsEditable = true;

        MainBallsA = 0;
        MainBallsB = 0;
        AccidentalBallsA = 0;
        AccidentalBallsB = 0;
        FoulsA = 0;
        FoulsB = 0;
        BreakShotPlayerName = null;

        OnPropertyChanged(nameof(StartStopButtonText));
    }

    private bool ValidatePlayers()
    {
        if (PlayerA is null || PlayerB is null)
        {
            _ = _appDialogService.ShowMessageAsync("Ошибка", "Выбери игроков!", "Ок");
            return false;
        }

        if (PlayerA.Name == PlayerB.Name)
        {
            _ = _appDialogService.ShowMessageAsync("Ошибка", "Выбери разных игроков!", "Ок");
            return false;
        }

        if (BreakerPlayer is null)
        {
            _ = _appDialogService.ShowMessageAsync("Ошибка", "Выбери кто будет разбивать!", "Ок");
            return false;
        }

        return true;
    }

    private async Task<bool> SaveMatchAsync()
    {
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
            _ = _appDialogService.ShowMessageAsync("Не сохранено", "Победитель не определён (нужно 8+ шаров).", "Ок");
            return false;
        }

        await _matchesStore.AddAsync(matchStats);
        return true;
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
        if (BreakShotPlayerName is null)
        {
            if (player.Name == PlayerA?.Name)
            {
                MainBallsA++;
            }
            else if (player.Name == PlayerB?.Name)
            {
                MainBallsB++;
            }
            _soundService.PlayAsync(SoundId.BreakShot);
        }
        if (BreakShotPlayerName is not null)
        {
            if (player.Name == PlayerA?.Name)
            {
                MainBallsA--;
            }
            else if (player.Name == PlayerB?.Name)
            {
                MainBallsB--;
            }
        }

        BreakShotPlayerName = BreakShotPlayerName == player.Name ? null : player.Name;
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
    }
}
