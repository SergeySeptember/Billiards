using System.Collections.ObjectModel;
using Billiards.DataBase.Entities;

namespace Billiards.Abstractions;

public interface IPlayersStore
{
    ObservableCollection<Player> Players { get; }
    Task ReloadAsync();
    Task AddAsync(string name);
    Task DeleteAllAsync();
    Task<bool> DeleteAsync(string name);
}