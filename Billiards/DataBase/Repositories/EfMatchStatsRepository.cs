using Billiards.Abstractions;
using Billiards.DataBase.Entities;
using Microsoft.EntityFrameworkCore;

namespace Billiards.DataBase.Repositories;

public class EfMatchStatsRepository(IDbContextFactory<BilliardsDbContext> dbFactory) : IMatchStatsRepository
{
    public async Task AddAsync(MatchStats match)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        db.MatchStats.Add(match);
        await db.SaveChangesAsync();
    }

    public async Task<List<MatchStats>> GetAllAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.MatchStats.ToListAsync();
    }

    public async Task DeleteAllAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        db.MatchStats.RemoveRange(db.MatchStats);
        await db.SaveChangesAsync();
    }
}