using Billiards.Core.Entities.DB;

namespace Billiards.Abstractions;

public interface IMatchStatsRepository
{
    Task AddAsync(MatchStats match);
    Task<List<MatchStats>> GetAllAsync();
    Task DeleteAllAsync();
}