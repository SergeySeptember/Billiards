using System.Collections.ObjectModel;
using Billiards.Abstractions;
using Billiards.DataBase.Entities;

namespace Billiards.Core;

public sealed class PlayersStore(IPlayerRepository repo) : IPlayersStore
{
    public ObservableCollection<Player> Players { get; } = new ResettableObservableCollection<Player>();

    public async Task ReloadAsync()
    {
        var items = await repo.GetAllAsync();

        MainThread.BeginInvokeOnMainThread(() =>
        {
            ((ResettableObservableCollection<Player>)Players).ReplaceRange(items);
        });
    }

    public async Task AddAsync(string name)
    {
        var player = await repo.AddAsync(name);
        MainThread.BeginInvokeOnMainThread(() => Players.Add(player));
    }

    public async Task<bool> DeleteAsync(string name)
    {
        var player = Players.FirstOrDefault(x => x.Name == name);
        if (player is null)
        {
            return false;
        }

        MainThread.BeginInvokeOnMainThread(() => Players.Remove(player));
        var result = await repo.DeletePlayerAsync(name);
        return result;
    }

    public async Task DeleteAllAsync()
    {
        await repo.DeleteAllAsync();
        MainThread.BeginInvokeOnMainThread(() => Players.Clear());
    }
}
