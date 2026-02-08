using System.Collections.ObjectModel;
using System.Windows.Input;
using Billiards.Abstractions;
using Billiards.Core;
using Billiards.DataBase.Entities;
using Billiards.Enum;

namespace Billiards.ViewModels;

public class MatchViewModel : BaseViewModel
{
    private readonly MatchTimer _matchTimer = new();
    private readonly IDispatcherTimer _uiTimer;
    private readonly IPlayersStore _playersStore;
    private readonly IMatchesStore _matchesStore;

    // ----- Виды бильярда -----
    public ObservableCollection<string> GameTypes { get; } = new()
    {
        "Свободная пирамида",
        "Сибирская пирамида",
        "Московская пирамида",
        "Невская пирамида",
        "Бесконечная пирамида"
    };

    private bool _isEditable = true;

    public bool IsEditable
    {
        get => _isEditable;
        set => SetProperty(ref _isEditable, value);
    }

    private string _selectedGameType;

    public string SelectedGameType
    {
        get => _selectedGameType;
        set => SetProperty(ref _selectedGameType, value);
    }

    // ----- Игроки -----
    public ObservableCollection<Player> Players => _playersStore.Players;

    private Player? _playerA;

    public Player? PlayerA
    {
        get => _playerA;
        set
        {
            if (SetProperty(ref _playerA, value))
            {
                RefreshBreakerCandidates();
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
            }
        }
    }

    public ObservableCollection<Player> BreakerCandidates { get; } = new();

    private Player? _breakerPlayer;

    public Player? BreakerPlayer
    {
        get => _breakerPlayer;
        set => SetProperty(ref _breakerPlayer, value);
    }

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
        set
        {
            var negativeScore = Preferences.Default.Get("negative_score", "false");
            if (string.Equals(negativeScore, "false", StringComparison.OrdinalIgnoreCase) && value < 0)
            {
                return;
            }
            SetProperty(ref _mainBallsA, value);
        }
    }

    private int _mainBallsB;

    public int MainBallsB
    {
        get => _mainBallsB;
        set
        {
            var negativeScore = Preferences.Default.Get("negative_score", "false");
            if (string.Equals(negativeScore, "false", StringComparison.OrdinalIgnoreCase) && value < 0)
            {
                return;
            }
            SetProperty(ref _mainBallsB, value);
        }
    }

    private int _accidentalBallsA;

    public int AccidentalBallsA
    {
        get => _accidentalBallsA;
        set
        {
            if (value >= 0)
            {
                SetProperty(ref _accidentalBallsA, value);
            }
        }
    }

    private int _accidentalBallsB;

    public int AccidentalBallsB
    {
        get => _accidentalBallsB;
        set
        {
            if (value >= 0)
            {
                SetProperty(ref _accidentalBallsB, value);
            }
        }
    }

    private int _foulsA;

    public int FoulsA
    {
        get => _foulsA;
        set
        {
            if (value >= 0)
            {
                SetProperty(ref _foulsA, value);
            }
        }
    }

    private int _foulsB;

    public int FoulsB
    {
        get => _foulsB;
        set
        {
            if (value >= 0)
            {
                SetProperty(ref _foulsB, value);
            }
        }
    }

    // ----- Таймер -----
    private string _timerText = "00:00:00";

    public string TimerText
    {
        get => _timerText;
        set => SetProperty(ref _timerText, value);
    }

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

    public MatchViewModel(IDispatcher dispatcher, IPlayersStore playersStore, IMatchesStore matchesStore, ISoundService soundService)
    {
        _playersStore = playersStore;
        _matchesStore = matchesStore;

        _selectedGameType = GameTypes.First();

        _uiTimer = dispatcher.CreateTimer();
        _uiTimer.Interval = TimeSpan.FromSeconds(1);
        _uiTimer.Tick += (_, _) => UpdateTimerText();

        StartStopCommand = new Command(StartStop);
        NewMatchCommand = new Command(async () => await NewMatchAsync());

        ToggleBreakShotCommand = new Command<Player?>(ToggleBreakShot);
        ClearBreakShotCommand = new Command(() => BreakShotPlayerName = null);

        MainBallsIncrementACommand = new Command(() => MainBallsA++);
        MainBallsDecrementACommand = new Command(() => MainBallsA--);
        MainBallsIncrementBCommand = new Command(() => MainBallsB++);
        MainBallsDecrementBCommand = new Command(() => MainBallsB--);


        AccidentalBallsIncrementACommand = new Command(() =>
        {
            AccidentalBallsA++;
            MainBallsA++;
            soundService.PlayAsync(SoundId.AccidentalPlus);
        });
        AccidentalBallsIncrementBCommand = new Command(() =>
        {
            AccidentalBallsB++;
            MainBallsB++;
            soundService.PlayAsync(SoundId.AccidentalPlus);
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
            var foulMode = Preferences.Default.Get("foul_mode", "shelf");
            if (foulMode == "shelf")
            {
                MainBallsB++;
            }
            else
            {
                MainBallsA--;
            }
            soundService.PlayAsync(SoundId.Fall);
        });
        FoulsIncrementBCommand = new Command(() =>
        {
            FoulsB++;
            var foulMode = Preferences.Default.Get("foul_mode", "shelf");
            if (foulMode == "shelf")
            {
                MainBallsA++;
            }
            else
            {
                MainBallsB--;
            }
            soundService.PlayAsync(SoundId.Fall);
        });
        FoulsDecrementACommand = new Command(() =>
        {
            FoulsA--;
            var foulMode = Preferences.Default.Get("foul_mode", "shelf");
            if (foulMode == "shelf")
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
            var foulMode = Preferences.Default.Get("foul_mode", "shelf");
            if (foulMode == "shelf")
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
        var page = Shell.Current.CurrentPage;

        if (!_matchTimer.IsRunning)
        {
            if (!ValidatePlayers(page))
            {
                return;
            }

            _matchTimer.Start();
            _uiTimer.Start();
            UpdateTimerText();

            IsEditable = false;

            OnPropertyChanged(nameof(StartStopButtonText));
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

        if (!ValidatePlayers(page))
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

        var page = Shell.Current.CurrentPage;
        var hasActivity = TimerText != "00:00:00";

        if (hasActivity)
        {
            var save = await page.DisplayAlert(
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

    private bool ValidatePlayers(Page page)
    {
        if (PlayerA is null || PlayerB is null)
        {
            _ = page.DisplayAlert("Ошибка", "Выбери игроков!", "Ок");
            return false;
        }

        if (PlayerA.Name == PlayerB.Name)
        {
            _ = page.DisplayAlert("Ошибка", "Выбери разных игроков!", "Ок");
            return false;
        }

        if (BreakerPlayer is null)
        {
            _ = page.DisplayAlert("Ошибка", "Выбери кто будет разбивать!", "Ок");
            return false;
        }

        return true;
    }

    private async Task<bool> SaveMatchAsync()
    {
        var page = Shell.Current.CurrentPage;
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
            _ = page.DisplayAlert("Не сохранено", "Победитель не определён (нужно 8+ шаров).", "Ок");
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