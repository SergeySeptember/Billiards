using System.Collections.ObjectModel;
using Billiards.Abstractions;
using Billiards.DataBase.Entities;

namespace Billiards.Core;

public sealed class MatchesStore(IMatchStatsRepository repo) : IMatchesStore
{
    public ObservableCollection<MatchStats> Matches { get; } = new();

    public async Task ReloadAsync()
    {
        var items = await repo.GetAllAsync();

        MainThread.BeginInvokeOnMainThread(() =>
        {
            Matches.Clear();
            foreach (var m in items)
            {
                Matches.Add(m);
            }
        });
    }

    public async Task AddAsync(MatchStats match)
    {
        await repo.AddAsync(match);
        MainThread.BeginInvokeOnMainThread(() => Matches.Add(match));
    }

    public async Task DeleteAllAsync()
    {
        await repo.DeleteAllAsync();
        MainThread.BeginInvokeOnMainThread(() => Matches.Clear());
    }

    public async Task DeleteByPlayerAsync(string playerName)
    {
        await repo.DeleteByPlayerAsync(playerName);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            for (var i = Matches.Count - 1; i >= 0; i--)
            {
                var m = Matches[i];
                if (string.Equals(m.WinnerPlayer, playerName, StringComparison.CurrentCultureIgnoreCase) ||
                    string.Equals(m.LosePlayer, playerName, StringComparison.CurrentCultureIgnoreCase))
                {
                    Matches.RemoveAt(i);
                }
            }
        });
    }
}