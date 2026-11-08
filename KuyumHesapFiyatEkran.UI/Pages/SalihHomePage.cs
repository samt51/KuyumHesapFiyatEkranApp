using KuyumHesapFiyatEkran.Core.Features.LiveRates;
using KuyumHesapFiyatEkran.Core.Features.LiveRates.Queries.GetLiveRates;
using KuyumHesapFiyatEkran.UI.Services;
using KuyumHesapFiyatEkran.UI.Views;
using Microsoft.Maui.Controls.Shapes;

namespace KuyumHesapFiyatEkran.UI.Pages;

public sealed class SalihHomePage : ContentPage
{
    private readonly LiveRatesStore store;
    private readonly Grid pageRoot = new();
    private VerticalStackLayout rates = new() { Spacing = 0 };
    private Label dateLabel = Text(string.Empty, 12, true);
    private Grid? themeOverlay;
    private AslanThemeMode theme;

    private Color PageColor => theme == AslanThemeMode.Light ? Color.FromArgb("#FAF7EF") : Color.FromArgb("#071522");
    private Color Surface => theme == AslanThemeMode.Light ? Colors.White : Color.FromArgb("#10243A");
    private Color Primary => theme == AslanThemeMode.Light ? Color.FromArgb("#171717") : Colors.White;
    private Color Secondary => theme == AslanThemeMode.Light ? Color.FromArgb("#6B6B6B") : Color.FromArgb("#B9C1CA");
    private Color BorderColor => theme == AslanThemeMode.Light ? Color.FromArgb("#ECE4D6") : Color.FromArgb("#30465B");

    public SalihHomePage(LiveRatesStore store)
    {
        this.store = store; Title = "Ana Sayfa"; Shell.SetNavBarIsVisible(this, false);
        AppThemeManager.ThemeChanged += OnThemeChanged; Render();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing(); theme = AppThemeManager.GetEffectiveTheme(); Render();
        await store.EnsureLoadedAsync(CancellationToken.None); PopulateRates();
    }

    private void OnThemeChanged(object? sender, AslanThemeMode mode) => MainThread.BeginInvokeOnMainThread(() => { theme = mode; Render(); PopulateRates(); });

    private void Render()
    {
        theme = AppThemeManager.GetEffectiveTheme(); BackgroundColor = PageColor;
        // The old table still belongs to the nested card after pageRoot is
        // cleared. Reusing it would attach the same Android view to two parents.
        rates.SizeChanged -= OnRatesSizeChanged;
        rates = new VerticalStackLayout { Spacing = 0 };
        rates.SizeChanged += OnRatesSizeChanged;
        CloseThemeMenu();
        pageRoot.Children.Clear(); pageRoot.RowDefinitions.Clear();
        pageRoot.RowDefinitions.Add(new RowDefinition(GridLength.Star)); pageRoot.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        pageRoot.Add(new ScrollView { BackgroundColor = PageColor, Content = BuildContent() });
        var nav = new BottomNavigationBar(0); Grid.SetRow(nav, 1); pageRoot.Add(nav); Content = pageRoot;
    }

    private View BuildContent()
    {
        var root = new VerticalStackLayout { Padding = new Thickness(16, 8, 16, 20), Spacing = 16 };
        root.Add(BuildHeader());
        dateLabel = Text(DateTime.Now.ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("tr-TR")), 12, true, Colors.White);
        var heroImage = new Image { Source = "light_home_jewelry.png", Aspect = Aspect.AspectFill };
        var shade = new BoxView { Background = new LinearGradientBrush { StartPoint = new Point(0, 0), EndPoint = new Point(1, 0), GradientStops = { new GradientStop(Color.FromRgba("#7A3D00CC"), 0), new GradientStop(Color.FromRgba("#7A3D0010"), 1) } } };
        var heroText = new VerticalStackLayout { Padding = new Thickness(16), Spacing = 7, VerticalOptions = LayoutOptions.Center, Children = { dateLabel, Text("Güncel Son Fiyatlar", 16, true, Colors.White), OutlineButton("Detayları Gör", async () => await Shell.Current.GoToAsync("///MainPage")) } };
        var heroGrid = new Grid(); heroGrid.Add(heroImage); heroGrid.Add(shade); heroGrid.Add(heroText);
        root.Add(Card(heroGrid, 154, 18, Colors.Transparent));

        root.Add(Text("Hızlı İşlemler", 17, true, Primary));
        var quick = new Grid { HeightRequest = 96, ColumnSpacing = 9, ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) } };
        var quickItems = new[] { (LucideIconKind.CircleDollarSign, "Fiyatlar", "///MainPage"), (LucideIconKind.Calculator, "Altın\nHesaplama", "///GoldCalculator"), (LucideIconKind.MapPin, "Yol Tarifi", "///Contact"), (LucideIconKind.Phone, "İletişim", "///Contact") };
        for (var i = 0; i < quickItems.Length; i++) { var item = Quick(quickItems[i]); Grid.SetColumn(item, i); quick.Add(item); } root.Add(quick);

        var rateHeader = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) } };
        rateHeader.Add(Text("Anlık Altın Fiyatları", 16, true, Primary));
        var all = new HorizontalStackLayout { Spacing = 2, Children = { Text("Tümünü Gör", 11, false, Secondary), new LucideIcon(LucideIconKind.ChevronRight, 15, Secondary) } }; Grid.SetColumn(all, 1); rateHeader.Add(all);
        all.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(async () => await Shell.Current.GoToAsync("///MainPage")) });
        root.Add(Card(new VerticalStackLayout { Padding = 14, Spacing = 8, Children = { rateHeader, rates } }, -1, 16, Surface));

        var banner = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(new GridLength(116)) } };
        banner.Add(Text("Güvenin ve Kalitenin\nParıltılı Adresi", 15, true, theme == AslanThemeMode.Light ? Color.FromArgb("#8D6814") : Color.FromArgb("#E1B744")));
        var goldImage = new Image { Source = "light_home_jewelry.png", Aspect = Aspect.AspectFill, WidthRequest = 112 }; Grid.SetColumn(goldImage, 1); banner.Add(goldImage);
        root.Add(Card(new Grid { Padding = new Thickness(16, 0, 0, 0), Children = { banner } }, 92, 16, theme == AslanThemeMode.Light ? Color.FromArgb("#FFF6DB") : Color.FromArgb("#10243A")));
        return root;
    }

    private View BuildHeader()
    {
        var brandGold = SalihUi.Gold;
        var logo = new Image { Source = BusinessInfo.Current.LogoImage, HeightRequest = 70, WidthRequest = 70, Aspect = Aspect.AspectFit, VerticalOptions = LayoutOptions.Center };
        var divider = new BoxView { WidthRequest = 1, HeightRequest = 56, Color = brandGold, Margin = new Thickness(12, 0, 14, 0), VerticalOptions = LayoutOptions.Center };
        var name = Text(BusinessInfo.Current.PrimaryName, 20, true, brandGold);
        name.FontFamily = "OpenSansSemibold"; name.CharacterSpacing = .7; name.LineBreakMode = LineBreakMode.NoWrap;
        var sub = Text(BusinessInfo.Current.SecondaryName, 13, false, brandGold);
        sub.FontFamily = "OpenSansSemibold"; sub.CharacterSpacing = 1.2; sub.LineBreakMode = LineBreakMode.NoWrap;
        var words = new VerticalStackLayout { Spacing = 0, VerticalOptions = LayoutOptions.Center, Children = { name, sub } };
        var identity = new HorizontalStackLayout { Spacing = 0, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center, Children = { logo, divider, words } };
        var actions = new VerticalStackLayout { Spacing = 2, HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.Start };
        actions.Add(IconButton(LucideIconKind.Bell, null));
        actions.Add(IconButton(AppThemeManager.GetThemeSelection() == ThemeSelection.Auto ? LucideIconKind.SunMoon : theme == AslanThemeMode.Light ? LucideIconKind.Sun : LucideIconKind.Moon, ShowThemeMenu));
        var header = new Grid { HeightRequest = 78, ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(new GridLength(40)) }, ColumnSpacing = 4 };
        header.SizeChanged += (_, _) =>
        {
            var compact = header.Width > 0 && header.Width < 330;
            logo.WidthRequest = logo.HeightRequest = compact ? 64 : 70;
            divider.HeightRequest = compact ? 52 : 56;
            divider.Margin = compact ? new Thickness(10, 0, 10, 0) : new Thickness(12, 0, 14, 0);
            name.FontSize = compact ? 18 : 20;
            name.CharacterSpacing = compact ? .3 : .7;
            sub.FontSize = compact ? 12 : 13;
        };
        header.Add(identity); Grid.SetColumn(actions, 1); header.Add(actions); return header;
    }

    private Border IconButton(LucideIconKind kind, Action? action)
    {
        var button = new Border { WidthRequest = 36, HeightRequest = 34, Padding = 6, BackgroundColor = Colors.Transparent, StrokeThickness = 0, Content = new LucideIcon(kind, 21, Primary) };
        var tap = new TapGestureRecognizer(); tap.Tapped += async (_, _) => { await button.ScaleTo(.9, 60); await button.ScaleTo(1, 90); action?.Invoke(); }; button.GestureRecognizers.Add(tap); return button;
    }

    private void ShowThemeMenu()
    {
        if (themeOverlay is not null) return;
        var panel = new VerticalStackLayout { Spacing = 2, Padding = 8 };
        panel.Add(ThemeChoice(ThemeSelection.Auto, LucideIconKind.SunMoon, "Otomatik", "Sistem saatine göre otomatik"));
        panel.Add(ThemeChoice(ThemeSelection.Light, LucideIconKind.Sun, "Açık Tema", "Açık temayı kullan"));
        panel.Add(ThemeChoice(ThemeSelection.Dark, LucideIconKind.Moon, "Koyu Tema", "Koyu temayı kullan"));
        var popup = new Border { WidthRequest = 300, Padding = 4, BackgroundColor = Surface, Stroke = BorderColor, StrokeThickness = 1, StrokeShape = new RoundRectangle { CornerRadius = 18 }, Shadow = new Shadow { Brush = Colors.Black, Opacity = .18f, Radius = 18, Offset = new Point(0, 7) }, Content = panel, HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.Start, Margin = new Thickness(0, 64, 12, 0) };
        themeOverlay = new Grid { BackgroundColor = Color.FromRgba("#00000033"), Children = { popup } };
        var dismiss = new TapGestureRecognizer(); dismiss.Tapped += (_, _) => CloseThemeMenu(); themeOverlay.GestureRecognizers.Add(dismiss);
        pageRoot.Add(themeOverlay); Grid.SetRowSpan(themeOverlay, 2);
    }

    private View ThemeChoice(ThemeSelection selection, LucideIconKind kind, string title, string subtitle)
    {
        var selected = AppThemeManager.GetThemeSelection() == selection;
        var row = new Grid { Padding = new Thickness(10, 9), ColumnDefinitions = { new ColumnDefinition(new GridLength(34)), new ColumnDefinition(GridLength.Star), new ColumnDefinition(new GridLength(28)) } };
        row.Add(new LucideIcon(kind, 21, selected ? SalihUi.Gold : Secondary));
        var copy = new VerticalStackLayout { Spacing = 1, Children = { Text(title, 14, true, Primary), Text(subtitle, 11, false, Secondary) } }; Grid.SetColumn(copy, 1); row.Add(copy);
        if (selected) { var check = new LucideIcon(LucideIconKind.Check, 19, SalihUi.Gold); Grid.SetColumn(check, 2); row.Add(check); }
        var tap = new TapGestureRecognizer(); tap.Tapped += (_, _) => { CloseThemeMenu(); AppThemeManager.SelectTheme(selection); }; row.GestureRecognizers.Add(tap); return row;
    }

    private void CloseThemeMenu() { if (themeOverlay is null) return; pageRoot.Children.Remove(themeOverlay); themeOverlay = null; }

    private void PopulateRates()
    {
        rates.Children.Clear();
        var current = store.CurrentRates;
        var items = (current?.Sarrafiye ?? Enumerable.Empty<LiveRateItem>())
            .Concat(current?.Maden ?? Enumerable.Empty<LiveRateItem>())
            .Take(5)
            .ToList();
        if (items.Count == 0)
        {
            rates.Add(Text(string.IsNullOrWhiteSpace(store.LastError) ? "Anlık fiyat bulunamadı." : store.LastError, 12, false, Secondary));
            return;
        }

        rates.Add(BuildRateTableRow(
            "Ürün", "Alış", "Satış", "Değişim (%)",
            Secondary, false, isHeader: true));

        foreach (var item in items)
        {
            rates.Add(BuildRateTableRow(
                Pretty(item.Name),
                item.Buy,
                item.Sell,
                item.ChangeDisplay,
                Primary,
                true,
                isHeader: false,
                item));
        }

        OnRatesSizeChanged(rates, EventArgs.Empty);
    }

    private void OnRatesSizeChanged(object? sender, EventArgs e)
    {
        if (rates.Width <= 0)
        {
            return;
        }

        var compact = rates.Width < 340;
        var headerSize = compact ? 9d : 10d;
        var nameSize = compact ? 10d : 11d;
        var valueSize = compact ? 10d : 11d;
        var changeSize = compact ? 9d : 10d;
        var isHeader = true;

        foreach (var grid in rates.Children.OfType<Grid>())
        {
            foreach (var child in grid.Children)
            {
                switch (child)
                {
                    case Label label:
                        var column = Grid.GetColumn(label);
                        label.FontSize = isHeader
                            ? headerSize
                            : column == 0
                                ? nameSize
                                : column == 3
                                    ? changeSize
                                    : valueSize;
                        break;
                    case Grid trend:
                        foreach (var inner in trend.Children.OfType<Label>())
                        {
                            inner.FontSize = changeSize;
                        }
                        break;
                }
            }

            isHeader = false;
        }
    }

    private Grid BuildRateTableRow(
        string product,
        string buy,
        string sell,
        string change,
        Color textColor,
        bool boldValues,
        bool isHeader,
        LiveRateItem? item = null)
    {
        var row = new Grid
        {
            Padding = new Thickness(0, isHeader ? 2 : 8),
            ColumnSpacing = 6,
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(1.2, GridUnitType.Star)),
                new ColumnDefinition(new GridLength(0.85, GridUnitType.Star)),
                new ColumnDefinition(new GridLength(0.85, GridUnitType.Star)),
                new ColumnDefinition(new GridLength(1.05, GridUnitType.Star))
            }
        };

        var name = Text(product, isHeader ? 10 : 11, isHeader, isHeader ? Secondary : textColor);
        name.LineBreakMode = LineBreakMode.TailTruncation;
        name.MaxLines = 1;
        name.HorizontalTextAlignment = TextAlignment.Start;
        row.Add(name);

        var buyLabel = Text(buy, isHeader ? 10 : 11, isHeader || boldValues, isHeader ? Secondary : textColor);
        buyLabel.HorizontalTextAlignment = TextAlignment.End;
        buyLabel.LineBreakMode = LineBreakMode.TailTruncation;
        buyLabel.MaxLines = 1;
        Grid.SetColumn(buyLabel, 1);
        row.Add(buyLabel);

        var sellLabel = Text(sell, isHeader ? 10 : 11, isHeader || boldValues, isHeader ? Secondary : textColor);
        sellLabel.HorizontalTextAlignment = TextAlignment.End;
        sellLabel.LineBreakMode = LineBreakMode.TailTruncation;
        sellLabel.MaxLines = 1;
        Grid.SetColumn(sellLabel, 2);
        row.Add(sellLabel);

        View changeView;
        if (isHeader)
        {
            var headerChange = Text(change, 10, true, Secondary);
            headerChange.HorizontalTextAlignment = TextAlignment.End;
            headerChange.LineBreakMode = LineBreakMode.TailTruncation;
            headerChange.MaxLines = 1;
            SemanticProperties.SetDescription(headerChange, RateChangeFormatter.SellChangeAccessibility);
            changeView = headerChange;
        }
        else
        {
            var direction = item is null ? 0 : RateChangeFormatter.GetDirection(item.SellChangePercent, item.ChangePercent);
            var trendColor = direction > 0
                ? Color.FromArgb("#28B663")
                : direction < 0
                    ? Color.FromArgb("#D84555")
                    : Secondary;
            var changeLabel = Text(change, 10, true, trendColor);
            changeLabel.HorizontalTextAlignment = TextAlignment.End;
            changeLabel.LineBreakMode = LineBreakMode.TailTruncation;
            changeLabel.MaxLines = 1;

            var trend = new Grid
            {
                ColumnSpacing = 2,
                HorizontalOptions = LayoutOptions.Fill,
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto)
                }
            };

            if (direction > 0)
            {
                var icon = new LucideIcon(LucideIconKind.TrendingUp, 12, trendColor)
                {
                    HorizontalOptions = LayoutOptions.End,
                    VerticalOptions = LayoutOptions.Center
                };
                trend.Add(icon);
                Grid.SetColumn(changeLabel, 1);
                trend.Add(changeLabel);
            }
            else if (direction < 0)
            {
                var icon = new LucideIcon(LucideIconKind.TrendingDown, 12, trendColor)
                {
                    HorizontalOptions = LayoutOptions.End,
                    VerticalOptions = LayoutOptions.Center
                };
                trend.Add(icon);
                Grid.SetColumn(changeLabel, 1);
                trend.Add(changeLabel);
            }
            else
            {
                changeLabel.HorizontalOptions = LayoutOptions.Fill;
                trend.Add(changeLabel);
                Grid.SetColumnSpan(changeLabel, 2);
            }

            SemanticProperties.SetDescription(trend, RateChangeFormatter.SellChangeAccessibility);
            changeView = trend;
        }

        Grid.SetColumn(changeView, 3);
        row.Add(changeView);
        return row;
    }

    private Border Card(View content, double height, float radius, Color background) => new() { Content = content, HeightRequest = height, BackgroundColor = background, Stroke = BorderColor, StrokeThickness = 1, StrokeShape = new RoundRectangle { CornerRadius = radius } };
    private View Quick((LucideIconKind Icon, string Text, string Route) item)
    {
        var caption = Text(item.Text, 9, false, Primary); caption.HorizontalTextAlignment = TextAlignment.Center;
        var view = Card(new VerticalStackLayout { Spacing = 7, VerticalOptions = LayoutOptions.Center, Children = { new LucideIcon(item.Icon, 24, SalihUi.Gold), caption } }, -1, 15, Surface);
        var tap = new TapGestureRecognizer(); tap.Tapped += async (_, _) => { await view.ScaleTo(.96, 70); await view.ScaleTo(1, 100); await Shell.Current.GoToAsync(item.Route); }; view.GestureRecognizers.Add(tap); return view;
    }

    private static string Pretty(string value) => System.Globalization.CultureInfo.GetCultureInfo("tr-TR").TextInfo.ToTitleCase(value.ToLowerInvariant());
    private static Label Text(string text, double size, bool bold = false, Color? color = null) => new() { Text = text, FontSize = size, FontAttributes = bold ? FontAttributes.Bold : FontAttributes.None, TextColor = color ?? Color.FromArgb("#171717"), FontAutoScalingEnabled = false, VerticalTextAlignment = TextAlignment.Center };
    private static Button OutlineButton(string text, Func<Task> action) { var b = new Button { Text = text, FontSize = 11, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#8D6814"), BackgroundColor = Colors.White, CornerRadius = 9, Padding = new Thickness(12, 6), HorizontalOptions = LayoutOptions.Start }; b.Clicked += async (_, _) => await action(); return b; }
}
