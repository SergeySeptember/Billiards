using Billiards.Abstractions;
using Billiards.ViewModels;
#if ANDROID
using AndroidX.Core.View;
#endif

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
            ApplyAndroidStatusBarInset();
            settingsViewModel.SyncThemeWithSystemIfNotSet();
            await Task.WhenAll(players.ReloadAsync(), matches.ReloadAsync());
            ApplyAndroidStatusBarInset();
        };
    }

#if ANDROID
    protected override void OnAppearing()
    {
        base.OnAppearing();
        ApplyAndroidStatusBarInset();
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(32), ApplyAndroidStatusBarInset);
    }

    private void ApplyAndroidStatusBarInset()
    {
        var decorView = Platform.CurrentActivity?.Window?.DecorView;
        if (decorView is null)
        {
            return;
        }

        var insets = ViewCompat.GetRootWindowInsets(decorView);
        if (insets is null)
        {
            return;
        }

        var topInsetPx = insets.GetInsets(WindowInsetsCompat.Type.StatusBars()).Top;
        var density = DeviceDisplay.MainDisplayInfo.Density;
        var topPadding = density > 0 ? topInsetPx / density : 0;

        if (Math.Abs(Padding.Top - topPadding) < 0.5)
        {
            return;
        }

        Padding = new Thickness(0, topPadding, 0, 0);
        RootCarousel.InvalidateMeasure();
    }
#endif
}
