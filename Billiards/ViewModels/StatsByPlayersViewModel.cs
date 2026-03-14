using System.Collections.ObjectModel;
using System.Windows.Input;
using Billiards.Abstractions;
using Billiards.ModelAndDto;

namespace Billiards.ViewModels;

public class StatsByPlayersViewModel : BaseViewModel
{
    private readonly IPlayerRepository _playerRepository;
    private readonly IMatchStatsRepository _matchStatsRepository;

    public ObservableCollection<PlayerStats> Rows { get; } = new();

    public string AverageMatchTimeText
    {
        get;
        set => SetProperty(ref field, value);
    } = "Среднее время игр за все время: 00:00:00";

    public bool IsEmptyVisible
    {
        get;
        set => SetProperty(ref field, value);
    }

    public bool IsTableVisible
    {
        get;
        set => SetProperty(ref field, value);
    }

    public ICommand RefreshCommand { get; }

    public StatsByPlayersViewModel(IPlayerRepository playerRepository, IMatchStatsRepository matchStatsRepository)
    {
        _playerRepository = playerRepository;
        _matchStatsRepository = matchStatsRepository;

        RefreshCommand = new Command(async () => await LoadAsync());
    }

    public async Task LoadAsync()
    {
        Rows.Clear();

        var matches = await _matchStatsRepository.GetAllAsync();
        var averageMatchTime = TimeSpan.FromSeconds(
            matches.Select(m => TryParseTime(m.MatchTime).TotalSeconds).DefaultIfEmpty(0).Average());
        AverageMatchTimeText = $"Среднее время игр за все время: {FormatTime(averageMatchTime)}";

        var players = await _playerRepository.GetAllAsync();
        if (players.Count == 0)
        {
            IsEmptyVisible = true;
            IsTableVisible = false;
            return;
        }

        var playerNames = players.Select(p => p.Name).ToList();

        foreach (var name in playerNames)
        {
            var pm = matches
                .Where(m =>
                    string.Equals(m.WinnerPlayer, name, StringComparison.CurrentCultureIgnoreCase) ||
                    string.Equals(m.LosePlayer, name, StringComparison.CurrentCultureIgnoreCase))
                .ToList();

            var played = pm.Count;
            var wins = pm.Count(m => string.Equals(m.WinnerPlayer, name, StringComparison.CurrentCultureIgnoreCase));

            var accidental = 0;
            var fouls = 0;

            foreach (var m in pm)
            {
                var isWinner = string.Equals(m.WinnerPlayer, name, StringComparison.CurrentCultureIgnoreCase);
                accidental += isWinner ? m.AccidentalBallsWinnerPlayer : m.AccidentalBallsLosePlayer;
                fouls += isWinner ? m.FoulsBallsWinnerPlayer : m.FoulsBallsLosePlayer;
            }

            var breakShot = pm.Count(m =>
                !string.IsNullOrWhiteSpace(m.BreakShotPlayer) &&
                string.Equals(m.BreakShotPlayer, name, StringComparison.CurrentCultureIgnoreCase));

            var dryWins = pm.Count(m =>
                string.Equals(m.WinnerPlayer, name, StringComparison.CurrentCultureIgnoreCase) &&
                m.BallsWinnerPlayer == 8 &&
                m.BallsLosePlayer == 0);

            var dryLosses = pm.Count(m =>
                string.Equals(m.LosePlayer, name, StringComparison.CurrentCultureIgnoreCase) &&
                m.BallsWinnerPlayer == 8 &&
                m.BallsLosePlayer == 0);

            Rows.Add(new()
            {
                PlayerName = name,
                GamePlayed = played,
                WinRate = played == 0 ? 0 : (double)wins / played * 100.0,
                AccidentalBalls = accidental,
                FoulsBalls = fouls,
                BreakShot = breakShot,
                DryWins = dryWins,
                DryLosses = dryLosses
            });
        }

        IsEmptyVisible = Rows.Count == 0;
        IsTableVisible = Rows.Count > 0;
    }

    private static TimeSpan TryParseTime(string? text)
        => TimeSpan.TryParse(text, out var ts) ? ts : TimeSpan.Zero;

    private static string FormatTime(TimeSpan ts)
        => $"{(int)ts.TotalHours:00}:{ts.Minutes:00}:{ts.Seconds:00}";
}
