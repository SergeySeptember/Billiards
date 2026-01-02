using Billiards.Core.Entities.DB;
using Microsoft.EntityFrameworkCore;

namespace Billiards.Data;

public class BilliardsDbContext : DbContext
{
    public BilliardsDbContext(DbContextOptions<BilliardsDbContext> options) : base(options) { }

    public DbSet<Player> Players => Set<Player>();
    public DbSet<MatchStats> MatchStats => Set<MatchStats>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Player>(e =>
        {
            e.ToTable("players");
            e.HasKey(x => x.Id);

            e.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(128);

            e.HasIndex(x => x.Name)
                .IsUnique();
        });

        modelBuilder.Entity<MatchStats>(e =>
        {
            e.ToTable("match_stats");
            e.HasKey(x => x.Id);

            e.Property(x => x.CurrentDateTime).IsRequired();

            e.Property(x => x.GameTypes)
                .IsRequired()
                .HasMaxLength(64);

            e.Property(x => x.MatchTime)
                .IsRequired()
                .HasMaxLength(16);

            e.Property(x => x.WinnerPlayer)
                .IsRequired()
                .HasMaxLength(128);

            e.Property(x => x.LosePlayer)
                .IsRequired()
                .HasMaxLength(128);

            e.Property(x => x.BreakShotPlayer)
                .HasMaxLength(128);
        });
    }
}