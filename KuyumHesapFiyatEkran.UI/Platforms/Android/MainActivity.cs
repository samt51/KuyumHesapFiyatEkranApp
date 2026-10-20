using Android.App;
using Android.Content.PM;
using Android.OS;

namespace KuyumHesapFiyatEkran.UI
{
    [Activity(
        Theme = "@style/Maui.SplashTheme",
        MainLauncher = true,
        LaunchMode = LaunchMode.SingleTop,
        ConfigurationChanges =
            ConfigChanges.ScreenSize |
            ConfigChanges.Orientation |
            ConfigChanges.UiMode |
            ConfigChanges.ScreenLayout |
            ConfigChanges.SmallestScreenSize |
            ConfigChanges.Density)]
    [IntentFilter(
        new[] { Android.Content.Intent.ActionMain },
        Categories = new[]
        {
            Android.Content.Intent.CategoryLeanbackLauncher
        })]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(
            Android.OS.Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var uiModeManager =
                GetSystemService(
                    Android.Content.Context.UiModeService)
                as Android.App.UiModeManager;

            var isTelevision =
                uiModeManager?.CurrentModeType ==
                Android.Content.Res.UiMode.TypeTelevision;

            if (isTelevision)
            {
                RequestedOrientation =
                    Android.Content.PM.ScreenOrientation.Landscape;
            }
        }

    }
}
