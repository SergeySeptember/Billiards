using Billiards.Core.Entities.DB;

namespace Billiards.Data.Repositories;

public interface IMatchStatsRepository
{
    Task AddAsync(MatchStats match, CancellationToken ct = default);
}