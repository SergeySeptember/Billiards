using Billiards.Abstractions;
using Billiards.ViewModels;

namespace Billiards;

public partial class MainPage : ContentPage
{
    private bool _isInitialized;

    public MainPage(MainViewModel mainViewModel, IPlayersStore players, IMatchesStore matches, SettingsViewModel settingsViewModel)
    {
        InitializeComponent();
        BindingContext = mainViewModel;

        Loaded += async (_, _) =>
        {
            if (_isInitialized)
            {
                return;
            }

            _isInitialized = true;
            settingsViewModel.SyncThemeWithSystemIfNotSet();
            await Task.WhenAll(players.ReloadAsync(), matches.ReloadAsync());
        };
    }
}
