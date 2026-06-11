using KuyumHesapFiyatEkran.UI.Services;

namespace KuyumHesapFiyatEkran.UI.Pages;

public sealed class AboutPage : ContentPage
{
    public AboutPage()
    {
        Title = "Hakkımızda";
        BackgroundColor = Color.FromArgb("#FFFFFF");

        Content = GoldCalculatorPage.CreateLayout(
            "Hakkımızda",
            "Aslan Kuyumculuk mobil uygulaması, güncel altın ve döviz fiyatlarını kolayca takip edebilmeniz ve temel hesaplamaları hızlıca yapabilmeniz için geliştirilmiştir.",
            CreateText($"Uygulama versiyonu: {AppInfo.Current.VersionString} ({AppInfo.Current.BuildString})"),
            new Button
            {
                Text = "Gizlilik Politikası",
                Command = new Command(async () => await Launcher.Default.OpenAsync(BusinessInfo.PrivacyPolicyUrl))
            }
        );
        ApplyTheme();
        AppThemeManager.ThemeChanged += OnThemeChanged;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ApplyTheme();
    }

    private void OnThemeChanged(object? sender, AslanThemeMode mode)
    {
        MainThread.BeginInvokeOnMainThread(ApplyTheme);
    }

    private void ApplyTheme()
    {
        ThemePageStyler.Apply(this);
    }

    private static Label CreateText(string text)
    {
        return new Label { Text = text, FontSize = 16, TextColor = Color.FromArgb("#2F2A1D") };
    }
}
