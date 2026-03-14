using Billiards.Abstractions;
using Billiards.ViewModels;
#if ANDROID
using Android.Util;
using AndroidX.Core.View;
#endif

namespace Billiards;

public partial class MainPage : ContentPage
{
    private bool _isInitialized;
    private bool _hasAppeared;

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
            ApplyMainPageLayout();
            settingsViewModel.SyncThemeWithSystemIfNotSet();
            await Task.WhenAll(players.ReloadAsync(), matches.ReloadAsync());
            ApplyMainPageLayout();
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        var applyOverlay = _hasAppeared;
        _hasAppeared = true;

        ApplyMainPageLayout(applyOverlay);
        Dispatcher.Dispatch(() => ApplyMainPageLayout(applyOverlay));
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(32), () => ApplyMainPageLayout(applyOverlay));
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(96), () => ApplyMainPageLayout(applyOverlay));
    }

    private void ApplyMainPageLayout(bool applyOverlay = false)
    {
        NavigationPage.SetHasNavigationBar(this, false);
        NavigationPage.SetHasBackButton(this, false);
        ApplyAndroidStatusBarInset();
        if (applyOverlay)
        {
            ApplyAndroidNavigationBarOverlay();
        }
        else if (RootCarousel.Margin.Top != 0)
        {
            RootCarousel.Margin = new Thickness(0);
        }
        RootCarousel.InvalidateMeasure();
        InvalidateMeasure();
        (Content as VisualElement)?.InvalidateMeasure();
    }

#if ANDROID
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
    }

    private void ApplyAndroidNavigationBarOverlay()
    {
        var activity = Platform.CurrentActivity;
        if (activity is null)
        {
            return;
        }

        var typedValue = new TypedValue();
        if (!activity.Theme?.ResolveAttribute(global::Android.Resource.Attribute.ActionBarSize, typedValue, true) ?? true)
        {
            return;
        }

        var actionBarHeightPx = TypedValue.ComplexToDimensionPixelSize(typedValue.Data, activity.Resources?.DisplayMetrics);
        var density = DeviceDisplay.MainDisplayInfo.Density;
        var overlayTop = density > 0 ? -(actionBarHeightPx / density) : 0;

        if (Math.Abs(RootCarousel.Margin.Top - overlayTop) < 0.5)
        {
            return;
        }

        RootCarousel.Margin = new Thickness(0, overlayTop, 0, 0);
    }
#else
    private void ApplyAndroidStatusBarInset()
    {
    }

    private void ApplyAndroidNavigationBarOverlay()
    {
    }
#endif
}
