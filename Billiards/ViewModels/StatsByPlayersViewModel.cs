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

    private bool _isEmptyVisible;

    public bool IsEmptyVisible
    {
        get => _isEmptyVisible;
        set => SetProperty(ref _isEmptyVisible, value);
    }

    private bool _isTableVisible;

    public bool IsTableVisible
    {
        get => _isTableVisible;
        set => SetProperty(ref _isTableVisible, value);
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

        var players = await _playerRepository.GetAllAsync();
        if (players.Count == 0)
        {
            IsEmptyVisible = true;
            IsTableVisible = false;
            return;
        }

        var matches = await _matchStatsRepository.GetAllAsync();
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

            Rows.Add(new()
            {
                PlayerName = name,
                GamePlayed = played,
                WinRate = played == 0 ? 0 : (double)wins / played * 100.0,
                AccidentalBalls = accidental,
                FoulsBalls = fouls,
                BreakShot = breakShot
            });
        }

        IsEmptyVisible = Rows.Count == 0;
        IsTableVisible = Rows.Count > 0;
    }
}