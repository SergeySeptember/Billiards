using System.Collections.ObjectModel;
using System.Windows.Input;
using Billiards.Abstractions;
using Billiards.DataBase.Entities;
using Billiards.ModelAndDto;
using Billiards.Utils;

namespace Billiards.ViewModels;

public class StatsViewModel : BaseViewModel
{
    private readonly IMatchesStore _matchesStore;

    public ObservableCollection<StatsRow> Rows { get; } = new();

    public bool IsEmptyVisible => MatchesCount <= 1;
    public bool IsTableVisible => MatchesCount > 0;

    private int _matchesCount;

    public int MatchesCount
    {
        get => _matchesCount;
        private set => SetProperty(ref _matchesCount, value);
    }

    public string MatchesCountText => $"Матчей сегодня: {MatchesCount}";

    public ICommand OpenByDaysCommand { get; }
    public ICommand OpenByPlayersCommand { get; }

    public StatsViewModel(IMatchesStore matchesStore)
    {
        _matchesStore = matchesStore;

        Rows.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(IsTableVisible));
            OnPropertyChanged(nameof(IsEmptyVisible));
            OnPropertyChanged(nameof(MatchesCountText));
        };

        _matchesStore.Matches.CollectionChanged += (_, _) => RebuildRows();

        RebuildRows();

        OpenByDaysCommand = new Command(async () =>
        {
            if (Shell.Current is not null)
            {
                await Shell.Current.GoToAsync(nameof(Views.StatsByDaysPage));
            }
        });

        OpenByPlayersCommand = new Command(async () =>
        {
            if (Shell.Current is not null)
            {
                await Shell.Current.GoToAsync(nameof(Views.StatsByPlayersPage));
            }
        });
    }

    private void RebuildRows()
    {
        Rows.Clear();

        var today = DateTime.Today;

        var todayMatches = _matchesStore.Matches
            .Where(m => m.CurrentDateTime.Date == today)
            .OrderBy(m => m.CurrentDateTime)
            .ToList();

        MatchesCount = todayMatches.Count;

        if (todayMatches.Count == 0)
        {
            Rows.Add(BuildSummaryRow(todayMatches));
            return;
        }

        var dayIndex = todayMatches
            .Select((m, idx) => new { Match = m, No = idx + 1 })
            .ToDictionary(x => x.Match, x => x.No);
        var groups = todayMatches
            .GroupBy(m => MakePair(m.WinnerPlayer, m.LosePlayer))
            .OrderBy(g => g.Min(m => m.CurrentDateTime))
            .ToList();

        foreach (var g in groups)
        {
            var groupMatches = g
                .OrderBy(m => dayIndex[m])
                .ToList();

            foreach (var m in groupMatches)
            {
                Rows.Add(new()
                {
                    MatchNo = dayIndex[m].ToString(),
                    Winner = m.WinnerPlayer,
                    Loser = m.LosePlayer,
                    Score = $"{m.BallsWinnerPlayer}:{m.BallsLosePlayer}",
                    Time = m.MatchTime
                });
            }

            Rows.Add(BuildSummaryRow(groupMatches));
        }
    }

    private static StatsRow BuildSummaryRow(List<MatchStats> matches)
    {
        if (matches.Count == 0)
        {
            return new();
        }

        var firstPlayerName = matches.First().WinnerPlayer;
        var secondPlayerName = matches.First().LosePlayer;

        var firstPlayerPoints = 0;
        var secondPlayerPoints = 0;

        var firstPlayerMatchWin = 0;
        var secondPlayerMatchWin = 0;

        var firstAccidental = 0;
        var secondAccidental = 0;

        foreach (var match in matches)
        {
            if (match.WinnerPlayer == firstPlayerName)
            {
                firstPlayerMatchWin++;

                firstPlayerPoints += match.BallsWinnerPlayer;
                secondPlayerPoints += match.BallsLosePlayer;

                firstAccidental += match.AccidentalBallsWinnerPlayer;
                secondAccidental += match.AccidentalBallsLosePlayer;
            }
            else
            {
                secondPlayerMatchWin++;

                secondPlayerPoints += match.BallsWinnerPlayer;
                firstPlayerPoints += match.BallsLosePlayer;

                secondAccidental += match.AccidentalBallsWinnerPlayer;
                firstAccidental += match.AccidentalBallsLosePlayer;
            }
        }

        var minusRandomBalls = Preferences.Default.Get(Const.MinusRandomBalls, "false");
        if (string.Equals(minusRandomBalls, "true", StringComparison.OrdinalIgnoreCase))
        {
            firstPlayerPoints -= firstAccidental;
            secondPlayerPoints -= secondAccidental;
        }

        var avgTime = TimeSpan.FromSeconds(matches.Select(m => TryParseTime(m.MatchTime).TotalSeconds).Average());

        if (firstPlayerMatchWin == secondPlayerMatchWin)
        {
            var isFirstPlayerWin = firstPlayerPoints > secondPlayerPoints;
            return new()
            {
                MatchNo = "Σ",
                Winner = isFirstPlayerWin ? firstPlayerName : secondPlayerName,
                Loser = isFirstPlayerWin ? secondPlayerName : firstPlayerName,
                Score = isFirstPlayerWin ? $"{firstPlayerMatchWin}:{secondPlayerMatchWin} ({firstPlayerPoints}:{secondPlayerPoints})" : $"{secondPlayerMatchWin}:{firstPlayerMatchWin} ({secondPlayerPoints}:{firstPlayerPoints})",
                Time = FormatTime(avgTime)
            };
        }
        else
        {
            var isFirstPlayerWin = firstPlayerMatchWin > secondPlayerMatchWin;
            return new()
            {
                MatchNo = "Σ",
                Winner = isFirstPlayerWin ? firstPlayerName : secondPlayerName,
                Loser = isFirstPlayerWin ? secondPlayerName : firstPlayerName,
                Score = isFirstPlayerWin ? $"{firstPlayerMatchWin}:{secondPlayerMatchWin} ({firstPlayerPoints}:{secondPlayerPoints})" : $"{secondPlayerMatchWin}:{firstPlayerMatchWin} ({secondPlayerPoints}:{firstPlayerPoints})",
                Time = FormatTime(avgTime)
            };
        }
    }

    private static (string A, string B) MakePair(string a, string b) =>
        string.Compare(a, b, StringComparison.CurrentCultureIgnoreCase) <= 0
            ? (a, b)
            : (b, a);

    private static string FormatTime(TimeSpan ts)
        => $"{(int)ts.TotalHours:00}:{ts.Minutes:00}:{ts.Seconds:00}";

    private static TimeSpan TryParseTime(string? s)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            return TimeSpan.Zero;
        }

        return TimeSpan.TryParse(s, out var t) ? t : TimeSpan.Zero;
    }
}