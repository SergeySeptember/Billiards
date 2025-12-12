using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Billiards.Core;

namespace Billiards.ViewModels;

public class MatchViewModel : BaseViewModel
{
    private readonly MatchTimer _matchTimer = new();
    private readonly IDispatcherTimer _uiTimer;

    public ObservableCollection<string> GameTypes { get; } =
        new()
        {
            "Свободная пирамида",
            "Московская пирамида",
            "Сибирская пирамида",
            "Бесконечная пирамида"
        };

    private string _selectedGameType;

    public string SelectedGameType
    {
        get => _selectedGameType;
        set => SetProperty(ref _selectedGameType, value);
    }

    private string _playerAName = "Игрок A";

    public string PlayerAName
    {
        get => _playerAName;
        set => SetProperty(ref _playerAName, value);
    }

    private string _playerBName = "Игрок B";

    public string PlayerBName
    {
        get => _playerBName;
        set => SetProperty(ref _playerBName, value);
    }

    private bool _isNamesEditable = true;

    public bool IsNamesEditable
    {
        get => _isNamesEditable;
        set => SetProperty(ref _isNamesEditable, value);
    }

    // Счётчики как строки, чтобы нормально парсить +/-
    private string _mainBallsA = "0";

    public string MainBallsA
    {
        get => _mainBallsA;
        set => SetProperty(ref _mainBallsA, value);
    }

    private string _mainBallsB = "0";

    public string MainBallsB
    {
        get => _mainBallsB;
        set => SetProperty(ref _mainBallsB, value);
    }

    private string _accidentalBallsA = "0";

    public string AccidentalBallsA
    {
        get => _accidentalBallsA;
        set => SetProperty(ref _accidentalBallsA, value);
    }

    private string _accidentalBallsB = "0";

    public string AccidentalBallsB
    {
        get => _accidentalBallsB;
        set => SetProperty(ref _accidentalBallsB, value);
    }

    private string _foulsA = "0";

    public string FoulsA
    {
        get => _foulsA;
        set => SetProperty(ref _foulsA, value);
    }

    private string _foulsB = "0";

    public string FoulsB
    {
        get => _foulsB;
        set => SetProperty(ref _foulsB, value);
    }

    private string _timerText = "00:00:00";

    public string TimerText
    {
        get => _timerText;
        set => SetProperty(ref _timerText, value);
    }

    // Текст кнопки Старт/Пауза/Продолжить
    public string StartPauseButtonText =>
        !_matchTimer.IsRunning
            ? "Старт"
            : _matchTimer.IsPaused
                ? "Продолжить"
                : "Пауза";

    // Команды
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
        _selectedGameType = GameTypes.First();

        var dispatcher = Application.Current?.Dispatcher
                         ?? throw new InvalidOperationException("Dispatcher not available");

        _uiTimer = dispatcher.CreateTimer();
        _uiTimer.Interval = TimeSpan.FromSeconds(1);
        _uiTimer.Tick += (_, _) => UpdateTimerText();

        StartPauseCommand = new Command(OnStartPause);
        StopCommand = new Command(OnStop);
        NewMatchCommand = new Command(OnNewMatch);

        MainBallsIncrementACommand = new Command(() => MainBallsA = ChangeInt(MainBallsA, +1));
        MainBallsDecrementACommand = new Command(() => MainBallsA = ChangeInt(MainBallsA, -1));
        MainBallsIncrementBCommand = new Command(() => MainBallsB = ChangeInt(MainBallsB, +1));
        MainBallsDecrementBCommand = new Command(() => MainBallsB = ChangeInt(MainBallsB, -1));

        AccidentalBallsIncrementACommand = new Command(() => AccidentalBallsA = ChangeInt(AccidentalBallsA, +1));
        AccidentalBallsDecrementACommand = new Command(() => AccidentalBallsA = ChangeInt(AccidentalBallsA, -1));
        AccidentalBallsIncrementBCommand = new Command(() => AccidentalBallsB = ChangeInt(AccidentalBallsB, +1));
        AccidentalBallsDecrementBCommand = new Command(() => AccidentalBallsB = ChangeInt(AccidentalBallsB, -1));

        FoulsIncrementACommand = new Command(() => FoulsA = ChangeInt(FoulsA, +1));
        FoulsDecrementACommand = new Command(() => FoulsA = ChangeInt(FoulsA, -1));
        FoulsIncrementBCommand = new Command(() => FoulsB = ChangeInt(FoulsB, +1));
        FoulsDecrementBCommand = new Command(() => FoulsB = ChangeInt(FoulsB, -1));
    }

    private static string ChangeInt(string current, int delta)
    {
        if (!int.TryParse(current, out var value))
        {
            value = 0;
        }

        value += delta;
        return value.ToString();
    }

    private void OnStartPause()
    {
        if (!_matchTimer.IsRunning)
        {
            // Первый старт
            _matchTimer.Start();
            IsNamesEditable = false;
            _uiTimer.Start();
        }
        else if (_matchTimer.IsPaused)
        {
            // Продолжить
            _matchTimer.Resume();
            _uiTimer.Start();
        }
        else
        {
            // Пауза
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

        // TODO: здесь позже спросим "Сохранить партию?"
        // и вызовем сервис сохранения матча.
    }

    private void OnNewMatch()
    {
        _matchTimer.Reset();
        _uiTimer.Stop();

        TimerText = "00:00:00";
        IsNamesEditable = true;

        MainBallsA = "0";
        MainBallsB = "0";
        AccidentalBallsA = "0";
        AccidentalBallsB = "0";
        FoulsA = "0";
        FoulsB = "0";

        // Имена игроков можно оставлять прежними или очищать — на твой вкус.
        OnPropertyChanged(nameof(StartPauseButtonText));
    }

    private void UpdateTimerText()
    {
        var elapsed = _matchTimer.GetElapsed(DateTime.UtcNow);
        TimerText = $"{(int)elapsed.TotalHours:00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}";
    }
}