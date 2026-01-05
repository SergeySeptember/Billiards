using Billiards.Core.Entities.DB;
using Microsoft.EntityFrameworkCore;

namespace Billiards.Data.Repositories;

public class EfMatchStatsRepository(IDbContextFactory<BilliardsDbContext> dbFactory) : IMatchStatsRepository
{
    public async Task AddAsync(MatchStats match, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        db.MatchStats.Add(match);
        await db.SaveChangesAsync(ct);
    }
}