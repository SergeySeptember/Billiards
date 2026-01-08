using Billiards.Abstractions;
using Billiards.ViewModels;

namespace Billiards;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel mainViewModel, IPlayersStore players, IMatchesStore matches, SettingsViewModel settingsViewModel)
    {
        InitializeComponent();
        BindingContext = mainViewModel;

        Loaded += async (_, _) =>
        {
            await players.ReloadAsync();
            await matches.ReloadAsync();
            settingsViewModel.SyncThemeWithSystemIfNotSet();
        };
    }
}