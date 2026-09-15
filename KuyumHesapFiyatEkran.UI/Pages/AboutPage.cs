using KuyumHesapFiyatEkran.UI.Services;
using KuyumHesapFiyatEkran.UI.Views;

namespace KuyumHesapFiyatEkran.UI.Pages;

public sealed class AboutPage : ContentPage
{
    public AboutPage()
    {
        Title = "Hakkımızda";
        SetDynamicResource(BackgroundColorProperty, "AppBackground");

        if (string.Equals(BusinessInfo.Current.Key, "SalihCetinkaya", StringComparison.OrdinalIgnoreCase))
        {
            Shell.SetNavBarIsVisible(this, false);
            Content = SalihUi.Page(CreateSalihAbout(), 4);
            ApplyTheme();
            AppThemeManager.ThemeChanged += OnThemeChanged;
            return;
        }

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

    private static View CreateSalihAbout()
    {
        var hero = new Grid { HeightRequest = 310 };
        hero.Add(new Image { Source = "light_home_jewelry.png", Aspect = Aspect.AspectFill });
        var brand = new VerticalStackLayout { Margin = new Thickness(0, 18, 0, 0), Spacing = 0, VerticalOptions = LayoutOptions.Start, HorizontalOptions = LayoutOptions.Center,
            Children = { new Image { Source = BusinessInfo.Current.LogoImage, HeightRequest = 70, WidthRequest = 70 }, SalihUi.Text("SALİH ÇETİNKAYA", 20, false, SalihUi.Gold), SalihUi.Text("KUYUMCU", 10, true, SalihUi.Gold) } };
        ((Label)brand.Children[1]).FontFamily = "serif"; hero.Add(brand);

        var stack = new VerticalStackLayout { Spacing = 14 };
        stack.Add(hero);
        var copy = new VerticalStackLayout { Padding = new Thickness(16, 4, 16, 20), Spacing = 14 };
        copy.Add(SalihUi.Text("Hakkımızda", 19, true));
        copy.Add(SalihUi.Text("Salih Çetinkaya Kuyumcu, yılların tecrübesi ve güven anlayışıyla siz değerli müşterilerimize en kaliteli hizmeti sunmayı hedeflemektedir.", 12));
        copy.Add(Feature(LucideIconKind.ShieldCheck, "Güvenilir Hizmet", "Her zaman güven ve şeffaflık ilkesiyle çalışıyoruz."));
        copy.Add(Feature(LucideIconKind.CircleDollarSign, "Kaliteli Ürünler", "En kaliteli ve sertifikalı altın ürünlerini sunuyoruz."));
        copy.Add(Feature(LucideIconKind.RefreshCw, "Güncel Fiyatlar", "Anlık güncellenen fiyatlarla şeffaf işlem yapıyoruz."));
        stack.Add(copy);
        return new ScrollView { Content = stack };
    }

    private static Border Feature(LucideIconKind icon, string title, string detail)
    {
        var grid = new Grid { ColumnDefinitions = { new ColumnDefinition(new GridLength(42)), new ColumnDefinition(GridLength.Star) }, ColumnSpacing = 10 };
        grid.Add(new LucideIcon(icon, 22, SalihUi.Gold));
        var words = new VerticalStackLayout { Spacing = 3, Children = { SalihUi.Text(title, 13, true), SalihUi.Text(detail, 10, false, SalihUi.Muted) } }; Grid.SetColumn(words, 1); grid.Add(words);
        return SalihUi.Card(grid, Colors.White, 10, new Thickness(10));
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
        if (string.Equals(BusinessInfo.Current.Key, "SalihCetinkaya", StringComparison.OrdinalIgnoreCase))
        {
            Content = null; BackgroundColor = SalihUi.Cream; Content = SalihUi.Page(CreateSalihAbout(), 4); return;
        }
        ThemePageStyler.Apply(this);
    }

    private static Label CreateText(string text)
    {
        var label = new Label { Text = text, FontSize = 15, HorizontalTextAlignment = TextAlignment.Center };
        label.SetDynamicResource(Label.TextColorProperty, "AppTextPrimary");
        return label;
    }
}
