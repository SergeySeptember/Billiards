using System.Collections.ObjectModel;
using System.Windows.Input;
using Billiards.Core;
using Billiards.Core.Entities.DB;

namespace Billiards.ViewModels;

public class MatchViewModel : BaseViewModel
{
    private readonly MatchTimer _matchTimer = new();
    private readonly IDispatcherTimer _uiTimer;

    // ----- Виды бильярда -----
    public ObservableCollection<object> GameTypes { get; } =
    [
        "Свободная пирамида",
        "Сибирская пирамида",
        "Московская пирамида",
        "Невская пирамида",
        "Бесконечная пирамида"
    ];

    public ObservableCollection<Player> Players { get; set; } =
        new()
        {
            new Player { Name = "Мешков Сергей" },
            new Player { Name = "Ахмедулин Сергей" },
            new Player { Name = "Игрок 3" }
        };

    private bool _isNamesEditable = true;
    public bool IsNamesEditable
    {
        get => _isNamesEditable;
        set => SetProperty(ref _isNamesEditable, value);
    }

    private string _selectedGameType;

    public string SelectedGameType
    {
        get => _selectedGameType;
        set => SetProperty(ref _selectedGameType, value);
    }

    // ----- Имена игроков -----
    private Player? _playerA;
    public Player? PlayerA
    {
        get => _playerA;
        set => SetProperty(ref _playerA, value);
    }

    private Player? _playerB;
    public Player? PlayerB
    {
        get => _playerB;
        set => SetProperty(ref _playerB, value);
    }

    // ----- Счётчики -----
    private int _mainBallsA = 0;

    public int MainBallsA
    {
        get => _mainBallsA;
        set => SetProperty(ref _mainBallsA, value);
    }

    private int _mainBallsB = 0;

    public int MainBallsB
    {
        get => _mainBallsB;
        set => SetProperty(ref _mainBallsB, value);
    }

    private int _accidentalBallsA = 0;

    public int AccidentalBallsA
    {
        get => _accidentalBallsA;
        set => SetProperty(ref _accidentalBallsA, value);
    }

    private int _accidentalBallsB = 0;

    public int AccidentalBallsB
    {
        get => _accidentalBallsB;
        set => SetProperty(ref _accidentalBallsB, value);
    }

    private int _foulsA = 0;

    public int FoulsA
    {
        get => _foulsA;
        set => SetProperty(ref _foulsA, value);
    }

    private int _foulsB = 0;

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

    public MatchViewModel()
    {
        _selectedGameType = GameTypes.First().ToString()!;

        var dispatcher = Application.Current?.Dispatcher ?? throw new InvalidOperationException("Dispatcher not available");

        _uiTimer = dispatcher.CreateTimer();
        _uiTimer.Interval = TimeSpan.FromSeconds(1);
        _uiTimer.Tick += (_, _) => UpdateTimerText();

        StartPauseCommand = new Command(OnStartPause);
        StopCommand = new Command(OnStop);
        NewMatchCommand = new Command(async () => await OnNewMatchAsync());

        // Забитые шары
        MainBallsIncrementACommand = new Command(() => MainBallsA++);
        MainBallsDecrementACommand = new Command(() => MainBallsA--);
        MainBallsIncrementBCommand = new Command(() => MainBallsB++);
        MainBallsDecrementBCommand = new Command(() => MainBallsB--);

        // Дураки
        AccidentalBallsIncrementACommand = new Command(() => AccidentalBallsA++);
        AccidentalBallsDecrementACommand = new Command(() => AccidentalBallsA--);
        AccidentalBallsIncrementBCommand = new Command(() => AccidentalBallsB++);
        AccidentalBallsDecrementBCommand = new Command(() => AccidentalBallsB--);

        // Штрафы
        FoulsIncrementACommand = new Command(() => FoulsA++);
        FoulsDecrementACommand = new Command(() => FoulsA--);
        FoulsIncrementBCommand = new Command(() => FoulsB++);
        FoulsDecrementBCommand = new Command(() => FoulsB--);
    }

    private void GetPlayersName()
    {
        // здесь из БД получаем имена игроков
        // времено так

    }

    private async Task OnNewMatchAsync()
    {
        OnStop();

        var hasActivity = TimerText != "00:00:00";
        if (hasActivity)
        {
            var page = GetCurrentPage();
            if (page is not null)
            {
                var save = await page.DisplayAlert(
                    title: "Новая партия",
                    message: "Сохранить статистику текущей партии перед началом новой?",
                    accept: "Сохранить",
                    cancel: "Не сохранять");

                if (save)
                {
                    var matchStats = new MatchStats()
                    {
                        CurrentDateTime = DateTime.Now,
                        MatchTime = TimerText,
                        GameTypes = _selectedGameType
                    };
                    if (MainBallsA >= 8)
                    {
                        matchStats.WinnerPlayer = PlayerA?.Name;
                        matchStats.BallsWinnerPlayer = MainBallsA;
                        matchStats.AccidentalBallsWinnerPlayer = AccidentalBallsA;
                        matchStats.FoulsBallsWinnerPlayer = FoulsA;

                        matchStats.LosePlayer = PlayerB?.Name;
                        matchStats.BallsLosePlayer = MainBallsB;
                        matchStats.AccidentalBallsLosePlayer = AccidentalBallsB;
                        matchStats.FoulsBallsLosePlayer = FoulsB;
                    }
                    else if (MainBallsB >= 8)
                    {
                        matchStats.WinnerPlayer = PlayerB?.Name;
                        matchStats.BallsWinnerPlayer = MainBallsB;
                        matchStats.AccidentalBallsWinnerPlayer = AccidentalBallsB;
                        matchStats.FoulsBallsWinnerPlayer = FoulsB;

                        matchStats.LosePlayer = PlayerA?.Name;
                        matchStats.BallsLosePlayer = MainBallsA;
                        matchStats.AccidentalBallsLosePlayer = AccidentalBallsA;
                        matchStats.FoulsBallsLosePlayer = FoulsA;
                    }
                    else
                    {
                        _ = page.DisplayAlert("Ошибка", "Что то не так!", "Ладно...");
                    }
                }
            }
        }

        ResetMatchState();
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

        OnPropertyChanged(nameof(StartPauseButtonText));
    }

    private void OnStartPause()
    {
        if (!_matchTimer.IsRunning)
        {
            var page = GetCurrentPage();
            if (string.IsNullOrEmpty(_playerA?.Name) || string.IsNullOrEmpty(_playerB?.Name))
            {
                _ = page?.DisplayAlert("Ошибка", "Выберите игроков!", "Ок");
                return;
            }
            if (_playerA.Name == _playerB.Name)
            {
                _ = page?.DisplayAlert("Ошибка", "Выберите разных игроков!", "Ок");
                return;
            }

            // первый старт
            _matchTimer.Start();
            IsNamesEditable = false;
            _uiTimer.Start();
        }
        else if (_matchTimer.IsPaused)
        {
            // продолжить
            _matchTimer.Resume();
            _uiTimer.Start();
        }
        else
        {
            // пауза
            _matchTimer.Pause();
            _uiTimer.Stop();
        }

        UpdateTimerText();
        OnPropertyChanged(nameof(StartPauseButtonText));
    }

    private void OnStop()
    {
        _matchTimer.Stop();
        _uiTimer.Stop();
        UpdateTimerText();
        OnPropertyChanged(nameof(StartPauseButtonText));
    }

    private void UpdateTimerText()
    {
        var elapsed = _matchTimer.GetElapsed(DateTime.UtcNow);
        TimerText = $"{(int)elapsed.TotalHours:00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}";
    }

    private static Page? GetCurrentPage()
    {
        if (Shell.Current?.CurrentPage is { } shellPage)
            return shellPage;

        return Application.Current?.Windows.FirstOrDefault()?.Page;
    }
}