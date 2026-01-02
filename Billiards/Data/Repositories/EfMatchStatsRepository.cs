using Billiards.Core.Entities.DB;
using Microsoft.EntityFrameworkCore;

namespace Billiards.Data.Repositories;

public class EfMatchStatsRepository : IMatchStatsRepository
{
    private readonly IDbContextFactory<BilliardsDbContext> _dbFactory;

    public EfMatchStatsRepository(IDbContextFactory<BilliardsDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task AddAsync(MatchStats match, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        db.MatchStats.Add(match);
        await db.SaveChangesAsync(ct);
    }
}