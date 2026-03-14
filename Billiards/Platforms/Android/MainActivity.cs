using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.View;

namespace Billiards;

[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.ScreenSize
        | ConfigChanges.Orientation
        | ConfigChanges.UiMode
        | ConfigChanges.ScreenLayout
        | ConfigChanges.SmallestScreenSize
        | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        var window = Window;
        if (window is null)
        {
            return;
        }

        WindowCompat.SetDecorFitsSystemWindows(window, true);
#pragma warning disable CA1422
        window.SetStatusBarColor(global::Android.Graphics.Color.ParseColor("#512BD4"));
#pragma warning restore CA1422

        var insetsController = WindowCompat.GetInsetsController(window, window.DecorView);
        if (insetsController is not null)
        {
            insetsController.AppearanceLightStatusBars = false;
        }
    }
}
