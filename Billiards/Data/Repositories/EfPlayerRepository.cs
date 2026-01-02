using Billiards.Core.Entities.DB;
using Microsoft.EntityFrameworkCore;

namespace Billiards.Data.Repositories;

public class EfPlayerRepository : IPlayerRepository
{
    private readonly IDbContextFactory<BilliardsDbContext> _dbFactory;

    public EfPlayerRepository(IDbContextFactory<BilliardsDbContext> dbFactory) => _dbFactory = dbFactory;

    public async Task<Player> AddAsync(string name, CancellationToken ct = default)
    {
        name = (name ?? "").Trim();
        if (name.Length == 0)
        {
            throw new ArgumentException("Имя игрока не может быть пустым.", nameof(name));
        }

        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var existing = await db.Players.FirstOrDefaultAsync(p => p.Name == name, ct);
        if (existing is not null)
        {
            return existing;
        }

        var player = new Player { Name = name };
        db.Players.Add(player);
        await db.SaveChangesAsync(ct);

        return player;
    }

    public async Task<List<Player>> GetAllAsync(CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        return await db.Players
            .OrderBy(p => p.Name)
            .ToListAsync(ct);
    }
}