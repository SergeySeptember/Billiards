using System.Collections.ObjectModel;
using Billiards.DataBase.Entities;

namespace Billiards.Abstractions;

public interface IMatchesStore
{
    ObservableCollection<MatchStats> Matches { get; }
    Task ReloadAsync();
    Task AddAsync(MatchStats match);
    Task DeleteAllAsync();
    Task DeleteByPlayerAsync(string playerName);
}