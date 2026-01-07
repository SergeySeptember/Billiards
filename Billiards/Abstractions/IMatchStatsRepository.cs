using Billiards.DataBase.Entities;

namespace Billiards.Abstractions;

public interface IMatchStatsRepository
{
    Task AddAsync(MatchStats match);
    Task<List<MatchStats>> GetAllAsync();
    Task DeleteAllAsync();
    Task DeleteByPlayerAsync(string playerName);
}