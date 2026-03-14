using Billiards.Abstractions;
using Billiards.Core;
using Billiards.DataBase;
using Billiards.DataBase.Repositories;
using Billiards.Platforms.Android;
using Billiards.ViewModels;
using Billiards.Views;
using CommunityToolkit.Maui;
using Microsoft.EntityFrameworkCore;
using Plugin.Maui.Audio;

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

        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<MainCarouselTemplateSelector>();

        builder.Services.AddSingleton<MatchViewModel>();
        builder.Services.AddSingleton<SettingsViewModel>();
        builder.Services.AddSingleton<StatsViewModel>();

        builder.Services.AddTransient<StatsByDaysPage>();
        builder.Services.AddTransient<StatsByDaysViewModel>();

        builder.Services.AddTransient<StatsByPlayersViewModel>();
        builder.Services.AddTransient<StatsByPlayersPage>();

        builder.Services.AddSingleton<IPlayerRepository, EfPlayerRepository>();
        builder.Services.AddSingleton<IMatchStatsRepository, EfMatchStatsRepository>();

        builder.Services.AddSingleton<IPlayersStore, PlayersStore>();
        builder.Services.AddSingleton<IMatchesStore, MatchesStore>();

        builder.Services.AddSingleton<IDatabaseBackupService, DatabaseBackupService>();
        builder.Services.AddSingleton<IAppPreferences, AndroidAppPreferences>();
        builder.Services.AddSingleton<IAppDialogService, AndroidAppDialogService>();
        builder.Services.AddSingleton<IExternalLinkService, AndroidExternalLinkService>();
        builder.Services.AddSingleton<IStatsDatePickerService, StatsDatePickerService>();

        builder.Services.AddSingleton(AudioManager.Current);
        builder.Services.AddSingleton<ISoundService, SoundService>();

        var app = builder.Build();

        using var scope = app.Services.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<BilliardsDbContext>>();
        using var db = factory.CreateDbContext();
        db.Database.EnsureCreated();

        return app;
    }
}
