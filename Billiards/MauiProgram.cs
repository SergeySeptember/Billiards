using Billiards.Abstractions;
using Billiards.Core;
using Billiards.Core.Service;
using Billiards.DataBase;
using Billiards.DataBase.Repositories;
using Billiards.ViewModels;
using Billiards.Views;
using CommunityToolkit.Maui;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Billiards;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });
        builder.UseMauiCommunityToolkit();

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "billiards.db");
        var connectionString = $"Data Source={dbPath}";
        builder.Services.AddDbContextFactory<BilliardsDbContext>(options => options.UseSqlite(connectionString));

        builder.Services.AddSingleton<IPlayerRepository, EfPlayerRepository>();
        builder.Services.AddSingleton<IMatchStatsRepository, EfMatchStatsRepository>();

        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddSingleton<MatchViewModel>();
        builder.Services.AddSingleton<SettingsViewModel>();
        builder.Services.AddSingleton<StatsViewModel>();

        builder.Services.AddTransient<MatchView>();
        builder.Services.AddTransient<SettingsView>();
        builder.Services.AddTransient<StatsView>();

        builder.Services.AddSingleton<MainCarouselTemplateSelector>();
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<AppShell>();

        builder.Services.AddSingleton<IPlayersStore, PlayersStore>();
        builder.Services.AddSingleton<IMatchesStore, MatchesStore>();

        builder.Services.AddSingleton<IDatabaseBackupService, DatabaseBackupService>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        using var scope = app.Services.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<BilliardsDbContext>>();
        using var db = factory.CreateDbContext();
        db.Database.EnsureCreated();

        return app;
    }
}