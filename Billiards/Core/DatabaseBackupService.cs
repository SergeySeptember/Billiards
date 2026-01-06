using Billiards.Abstractions;
using Billiards.DataBase;
using Billiards.DataBase.Entities;
using Billiards.ModelAndDto;
using Microsoft.EntityFrameworkCore;

namespace Billiards.Core;

public sealed class DatabaseBackupService(IDbContextFactory<BilliardsDbContext> dbFactory) : IDatabaseBackupService
{
    public async Task<BilliardsBackupDto> BuildBackupAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        var players = await db.Players
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .Select(p => new PlayerDto { Name = p.Name })
            .ToListAsync();

        var matches = await db.MatchStats
            .AsNoTracking()
            .OrderByDescending(m => m.CurrentDateTime)
            .Select(m => new MatchStatsDto
            {
                CurrentDateTime = m.CurrentDateTime,
                GameTypes = m.GameTypes,

                WinnerPlayer = m.WinnerPlayer,
                LosePlayer = m.LosePlayer,

                MatchTime = m.MatchTime,

                BallsWinnerPlayer = m.BallsWinnerPlayer,
                BallsLosePlayer = m.BallsLosePlayer,

                AccidentalBallsWinnerPlayer = m.AccidentalBallsWinnerPlayer,
                AccidentalBallsLosePlayer = m.AccidentalBallsLosePlayer,

                FoulsBallsWinnerPlayer = m.FoulsBallsWinnerPlayer,
                FoulsBallsLosePlayer = m.FoulsBallsLosePlayer,

                BreakShotPlayer = m.BreakShotPlayer
            })
            .ToListAsync();

        return new()
        {
            Players = players,
            Matches = matches
        };
    }

    public async Task RestoreBackupAsync(BilliardsBackupDto backup)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        db.MatchStats.RemoveRange(db.MatchStats);
        db.Players.RemoveRange(db.Players);
        await db.SaveChangesAsync();

        var players = backup.Players
            .Select(p => p.Name)
            .Select(name => new Player { Name = name })
            .ToList();

        await db.Players.AddRangeAsync(players);
        await db.SaveChangesAsync();

        var matches = backup.Matches.Select(m => new MatchStats
        {
            CurrentDateTime = m.CurrentDateTime,
            GameTypes = m.GameTypes,

            WinnerPlayer = m.WinnerPlayer,
            LosePlayer = m.LosePlayer,

            MatchTime = m.MatchTime,

            BallsWinnerPlayer = m.BallsWinnerPlayer,
            BallsLosePlayer = m.BallsLosePlayer,

            AccidentalBallsWinnerPlayer = m.AccidentalBallsWinnerPlayer,
            AccidentalBallsLosePlayer = m.AccidentalBallsLosePlayer,

            FoulsBallsWinnerPlayer = m.FoulsBallsWinnerPlayer,
            FoulsBallsLosePlayer = m.FoulsBallsLosePlayer,

            BreakShotPlayer = m.BreakShotPlayer
        }).ToList();

        await db.MatchStats.AddRangeAsync(matches);
        await db.SaveChangesAsync();
    }
}