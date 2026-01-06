using System.Collections.ObjectModel;
using Billiards.Abstractions;
using Billiards.Core.Entities.DB;

namespace Billiards.Service;

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
}