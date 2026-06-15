using KuyumHesapFiyatEkran.UI.Services;
using KuyumHesapFiyatEkran.UI.Views;

namespace KuyumHesapFiyatEkran.UI.Pages;

public sealed class AboutPage : ContentPage
{
    public AboutPage()
    {
        Title = "Hakkımızda";
        SetDynamicResource(BackgroundColorProperty, "AppBackground");

        var privacyPolicyButton = new Button
        {
            Text = "Gizlilik Politikası",
            FontAttributes = FontAttributes.Bold,
            CornerRadius = 8,
            Command = new Command(async () => await Launcher.Default.OpenAsync(BusinessInfo.PrivacyPolicyUrl))
        };
        privacyPolicyButton.SetDynamicResource(Button.BackgroundColorProperty, "AppAccent");
        privacyPolicyButton.SetDynamicResource(Button.TextColorProperty, "AppBackground");
        privacyPolicyButton.SetDynamicResource(Button.BorderColorProperty, "AppBorder");

        var aboutContent = GoldCalculatorPage.CreateLayout(
            "Hakkımızda",
            $"{BusinessInfo.CompanyName} mobil uygulaması, güncel altın ve döviz fiyatlarını kolayca takip edebilmeniz ve temel hesaplamaları hızlıca yapabilmeniz için geliştirilmiştir.",
            CreateText($"Uygulama versiyonu: {AppInfo.Current.VersionString} ({AppInfo.Current.BuildString})"),
            privacyPolicyButton
        );

        if (string.Equals(BusinessInfo.Current.Key, "SalihCetinkaya", StringComparison.OrdinalIgnoreCase))
        {
            var pageGrid = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition(GridLength.Star),
                    new RowDefinition(GridLength.Auto)
                }
            };
            pageGrid.Add(aboutContent);

            var bottomBar = new BottomNavigationBar(4);
            Grid.SetRow(bottomBar, 1);
            pageGrid.Add(bottomBar);

            Content = pageGrid;
        }
        else
        {
            Content = aboutContent;
        }

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
        var label = new Label { Text = text, FontSize = 15, HorizontalTextAlignment = TextAlignment.Center };
        label.SetDynamicResource(Label.TextColorProperty, "AppTextPrimary");
        return label;
    }
}
