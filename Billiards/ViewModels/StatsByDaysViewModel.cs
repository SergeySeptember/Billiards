using System.Collections.ObjectModel;
using Billiards.Abstractions;
using Billiards.DataBase.Entities;
using Billiards.ModelAndDto;

namespace Billiards.ViewModels;

public class StatsByDaysViewModel : BaseViewModel
{
    private readonly IMatchesStore _matchesStore;

    public ObservableCollection<FullMatchStatsRow> Rows { get; } = new();

    private DateTime _selectedDate = DateTime.Today;

    public DateTime SelectedDate
    {
        get => _selectedDate;
        set
        {
            if (SetProperty(ref _selectedDate, value))
            {
                RebuildRows();
            }
        }
    }

    public bool IsTableVisible => Rows.Count > 0;

    private int _matchesCount;

    public int MatchesCount
    {
        get => _matchesCount;
        private set => SetProperty(ref _matchesCount, value);
    }

    public StatsByDaysViewModel(IMatchesStore matchesStore)
    {
        _matchesStore = matchesStore;
        _matchesStore.Matches.CollectionChanged += (_, _) => RebuildRows();
        RebuildRows();
    }

    private void RebuildRows()
    {
        Rows.Clear();

        var day = SelectedDate.Date;
        var ordered = _matchesStore.Matches
            .Where(m => m.CurrentDateTime.Date == day)
            .OrderBy(m => m.CurrentDateTime)
            .ToList();

        MatchesCount = ordered.Count;
        if (ordered.Count == 0)
        {
            OnPropertyChanged(nameof(IsTableVisible));
            return;
        }

        var dayIndex = ordered
            .Select((m, i) => new { m.Id, No = i + 1 })
            .ToDictionary(x => x.Id, x => x.No);

        var pairOrder = new Dictionary<PlayerPair, int>();
        var orderCounter = 0;

        foreach (var m in ordered)
        {
            var key = MakePair(m.WinnerPlayer, m.LosePlayer);
            if (!pairOrder.ContainsKey(key))
            {
                pairOrder[key] = ++orderCounter;
            }
        }

        var groups = ordered
            .GroupBy(m => MakePair(m.WinnerPlayer, m.LosePlayer))
            .OrderBy(g => pairOrder[g.Key])
            .ToList();

        foreach (var g in groups)
        {
            var groupMatches = g.OrderBy(m => m.CurrentDateTime).ToList();

            foreach (var m in groupMatches)
            {
                Rows.Add(new()
                {
                    MatchNo = dayIndex.TryGetValue(m.Id, out var no) ? no.ToString() : string.Empty,
                    Start = m.CurrentDateTime.ToString("HH:mm"),
                    Winner = m.WinnerPlayer,
                    Loser = m.LosePlayer,

                    Score = $"{m.BallsWinnerPlayer}:{m.BallsLosePlayer}",
                    Accidental = $"{m.AccidentalBallsWinnerPlayer}:{m.AccidentalBallsLosePlayer}",
                    Fouls = $"{m.FoulsBallsWinnerPlayer}:{m.FoulsBallsLosePlayer}",
                    BreakShot = string.IsNullOrWhiteSpace(m.BreakShotPlayer) ? string.Empty : m.BreakShotPlayer,
                    Time = m.MatchTime,
                    GameType = m.GameTypes
                });
            }

            Rows.Add(BuildSummaryRow(groupMatches));
        }

        OnPropertyChanged(nameof(IsTableVisible));
    }

    private static FullMatchStatsRow BuildSummaryRow(List<MatchStats> matches)
    {
        if (matches.Count == 0)
        {
            return new() { MatchNo = "Σ" };
        }

        var firstPlayerName = matches[0].WinnerPlayer;
        var secondPlayerName = matches[0].LosePlayer;

        var firstPlayerPoints = 0;
        var secondPlayerPoints = 0;

        var firstPlayerMatchWin = 0;
        var secondPlayerMatchWin = 0;

        foreach (var match in matches)
        {
            if (match.WinnerPlayer == firstPlayerName)
            {
                firstPlayerPoints += match.BallsWinnerPlayer - match.FoulsBallsWinnerPlayer;
                secondPlayerPoints += match.BallsLosePlayer - match.FoulsBallsLosePlayer;
                firstPlayerMatchWin++;
            }
            else
            {
                secondPlayerPoints += match.BallsWinnerPlayer - match.FoulsBallsWinnerPlayer;
                firstPlayerPoints += match.BallsLosePlayer - match.FoulsBallsLosePlayer;
                secondPlayerMatchWin++;
            }
        }

        var avgTime = TimeSpan.FromSeconds(
            matches.Select(m => TryParseTime(m.MatchTime).TotalSeconds).Average()
        );

        var firstIsWinner = firstPlayerMatchWin == secondPlayerMatchWin
            ? firstPlayerPoints > secondPlayerPoints
            : firstPlayerMatchWin > secondPlayerMatchWin;

        var winner = firstIsWinner ? firstPlayerName : secondPlayerName;
        var loser = firstIsWinner ? secondPlayerName : firstPlayerName;

        var score = firstIsWinner
            ? $"{firstPlayerMatchWin}:{secondPlayerMatchWin} ({firstPlayerPoints}:{secondPlayerPoints})"
            : $"{secondPlayerMatchWin}:{firstPlayerMatchWin} ({secondPlayerPoints}:{firstPlayerPoints})";

        return new()
        {
            MatchNo = "Σ",
            Winner = winner,
            Loser = loser,
            Score = score,
            Time = FormatTime(avgTime),
            Fouls = "", // Todo:
            Accidental = "" // Todo:
        };
    }

    private static TimeSpan TryParseTime(string? text)
        => TimeSpan.TryParse(text, out var ts) ? ts : TimeSpan.Zero;

    private static string FormatTime(TimeSpan ts)
        => $"{(int)ts.TotalHours:00}:{ts.Minutes:00}:{ts.Seconds:00}";

    private readonly record struct PlayerPair(string A, string B);

    private static PlayerPair MakePair(string p1, string p2)
        => string.Compare(p1, p2, StringComparison.CurrentCultureIgnoreCase) <= 0
            ? new(p1, p2)
            : new PlayerPair(p2, p1);
}