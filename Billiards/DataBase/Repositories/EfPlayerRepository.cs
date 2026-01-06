using Billiards.Abstractions;
using Billiards.DataBase.Entities;
using Microsoft.EntityFrameworkCore;

namespace Billiards.DataBase.Repositories;

public class EfPlayerRepository(IDbContextFactory<BilliardsDbContext> dbFactory) : IPlayerRepository
{
    public async Task<Player> AddAsync(string name)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        var existing = await db.Players.FirstOrDefaultAsync(p => p.Name == name);
        if (existing is not null)
        {
            return existing;
        }

        var player = new Player { Name = name };
        db.Players.Add(player);
        await db.SaveChangesAsync();

        return player;
    }

    public async Task<List<Player>> GetAllAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.Players.ToListAsync();
    }

    public async Task DeleteAllAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        db.Players.RemoveRange(db.Players);
        await db.SaveChangesAsync();
    }

    public async Task<bool> DeletePlayerAsync(string name)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var rowsDeleted = await db.Players
            .Where(p => p.Name == name)
            .ExecuteDeleteAsync();

        return rowsDeleted > 0;
    }
}