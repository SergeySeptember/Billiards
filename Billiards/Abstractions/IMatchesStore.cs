using System.Collections.ObjectModel;
using Billiards.Core.Entities.DB;

namespace Billiards.Abstractions;

public interface IMatchesStore
{
    ObservableCollection<MatchStats> Matches { get; }
    Task ReloadAsync();
    Task AddAsync(MatchStats match);
    Task DeleteAllAsync();
}