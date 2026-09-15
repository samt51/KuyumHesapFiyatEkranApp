using System.Collections.ObjectModel;
using KuyumHesapFiyatEkran.Core.Features.LiveRates;
using KuyumHesapFiyatEkran.UI.Models;
using KuyumHesapFiyatEkran.UI.Services;
using Microsoft.Maui.Controls.Shapes;
using KuyumHesapFiyatEkran.UI.Views;

namespace KuyumHesapFiyatEkran.UI.Pages;

public sealed class FavoritesPage : ContentPage
{
    private readonly LiveRatesStore liveRatesStore;
    private readonly ObservableCollection<RateDisplayItem> favorites = new();
    private readonly Label emptyLabel = new()
    {
        Text = "Henüz favori eklenmedi. Canlı Fiyatlar ekranında yıldız simgesine dokunarak favori listenizi oluşturabilirsiniz.",
        FontSize = 14,
        HorizontalTextAlignment = TextAlignment.Center
    };

    public FavoritesPage(LiveRatesStore liveRatesStore)
    {
        this.liveRatesStore = liveRatesStore;
        Title = "Favoriler";
        SetDynamicResource(BackgroundColorProperty, "AppBackground");
        liveRatesStore.FavoritesChanged += (_, _) => RefreshFavorites();

        emptyLabel.SetDynamicResource(Label.TextColorProperty, "AppTextMuted");

        var collection = new CollectionView
        {
            ItemsSource = favorites,
            SelectionMode = SelectionMode.None,
            ItemTemplate = new DataTemplate(() =>
            {
                var name = new Label { FontSize = 16, FontAttributes = FontAttributes.Bold };
                name.SetDynamicResource(Label.TextColorProperty, "AppTextPrimary");
                name.SetBinding(Label.TextProperty, nameof(RateDisplayItem.Name));

                var code = new Label { FontSize = 11 };
                code.SetDynamicResource(Label.TextColorProperty, "AppTextMuted");
                code.SetBinding(Label.TextProperty, nameof(RateDisplayItem.Code));
                code.SetBinding(IsVisibleProperty, nameof(RateDisplayItem.HasCode));

                var buy = new Label { FontSize = 15, FontAttributes = FontAttributes.Bold, HorizontalTextAlignment = TextAlignment.End };
                buy.SetDynamicResource(Label.TextColorProperty, "AppTextPrimary");
                buy.SetBinding(Label.TextProperty, nameof(RateDisplayItem.Buy));

                var sell = new Label { FontSize = 15, FontAttributes = FontAttributes.Bold, HorizontalTextAlignment = TextAlignment.End };
                sell.SetDynamicResource(Label.TextColorProperty, "AppTextPrimary");
                sell.SetBinding(Label.TextProperty, nameof(RateDisplayItem.Sell));

                var buyTitle = new Label { Text = "Alış", FontSize = 11, HorizontalTextAlignment = TextAlignment.End };
                buyTitle.SetDynamicResource(Label.TextColorProperty, "AppTextMuted");

                var sellTitle = new Label { Text = "Satış", FontSize = 11, HorizontalTextAlignment = TextAlignment.End };
                sellTitle.SetDynamicResource(Label.TextColorProperty, "AppTextMuted");

                var priceStack = new VerticalStackLayout
                {
                    Spacing = 3,
                    Children =
                    {
                        buyTitle,
                        buy,
                        sellTitle,
                        sell
                    }
                };
                Grid.SetColumn(priceStack, 1);
                var row = new Grid
                {
                    ColumnDefinitions =
                    {
                        new ColumnDefinition(GridLength.Star),
                        new ColumnDefinition(new GridLength(120))
                    }
                };
                row.Add(new VerticalStackLayout { Spacing = 3, VerticalOptions = LayoutOptions.Center, Children = { name, code } });
                row.Add(priceStack);

                var border = new Border
                {
                    Margin = new Thickness(0, 5),
                    Padding = new Thickness(16, 12),
                    StrokeShape = new RoundRectangle { CornerRadius = 12 },
                    Content = row
                };
                border.SetDynamicResource(Border.StrokeProperty, "AppBorder");
                border.SetDynamicResource(Border.BackgroundColorProperty, "AppCardBackground");

                return border;
            })
        };

        var header = new Label 
        { 
            Text = "FAVORİLER", 
            FontSize = 20, 
            FontAttributes = FontAttributes.Bold, 
            CharacterSpacing = 2,
            FontFamily = "serif",
            HorizontalTextAlignment = TextAlignment.Center
        };
        header.SetDynamicResource(Label.TextColorProperty, "AppBrandText");

        var topStack = new VerticalStackLayout { Margin = new Thickness(0, 12, 0, 12), Children = { emptyLabel } };
        Grid.SetRow(topStack, 1);
        Grid.SetRow(collection, 2);

        var root = new Grid
        {
            Padding = 18,
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star)
            }
        };
        root.Add(header);
        root.Add(topStack);
        root.Add(collection);

        if (string.Equals(BusinessInfo.Current.Key, "SalihCetinkaya", StringComparison.OrdinalIgnoreCase))
        {
            Shell.SetNavBarIsVisible(this, false);
            Content = SalihUi.Page(CreateSalihFavorites(), 1);
        }
        else
        {
            Content = root;
        }

        ApplyTheme();
        AppThemeManager.ThemeChanged += OnThemeChanged;
    }

    private View CreateSalihFavorites()
    {
        var list = new CollectionView { ItemsSource = favorites, SelectionMode = SelectionMode.None, ItemTemplate = new DataTemplate(() =>
        {
            var star = new LucideIcon(LucideIconKind.Star, 21, SalihUi.Gold, true);
            var name = SalihUi.Text("", 12, true); name.SetBinding(Label.TextProperty, nameof(RateDisplayItem.Name));
            var sell = SalihUi.Text("", 12, true); sell.HorizontalTextAlignment = TextAlignment.End; sell.SetBinding(Label.TextProperty, nameof(RateDisplayItem.Sell));
            var change = SalihUi.Text("", 10, true, Color.FromArgb("#8A8A8A"));
            change.HorizontalTextAlignment = TextAlignment.End;
            change.SetBinding(Label.TextProperty, nameof(RateDisplayItem.ChangePercent));
            change.Triggers.Add(new DataTrigger(typeof(Label))
            {
                Binding = new Binding(nameof(RateDisplayItem.IsUp)),
                Value = true,
                Setters = { new Setter { Property = Label.TextColorProperty, Value = Color.FromArgb("#26B96F") } }
            });
            change.Triggers.Add(new DataTrigger(typeof(Label))
            {
                Binding = new Binding(nameof(RateDisplayItem.IsDown)),
                Value = true,
                Setters = { new Setter { Property = Label.TextColorProperty, Value = Color.FromArgb("#D84555") } }
            });
            SemanticProperties.SetDescription(change, RateChangeFormatter.SellChangeAccessibility);
            var up = new LucideIcon(LucideIconKind.TrendingUp, 11, Color.FromArgb("#26B96F")); up.SetBinding(IsVisibleProperty, nameof(RateDisplayItem.IsUp));
            var down = new LucideIcon(LucideIconKind.TrendingDown, 11, Color.FromArgb("#D84555")); down.SetBinding(IsVisibleProperty, nameof(RateDisplayItem.IsDown));
            var trend = new HorizontalStackLayout { Spacing = 3, HorizontalOptions = LayoutOptions.End, Children = { up, down, change } };
            var grid = new Grid { Padding = new Thickness(4, 14), ColumnDefinitions = { new ColumnDefinition(new GridLength(28)), new ColumnDefinition(GridLength.Star), new ColumnDefinition(new GridLength(95)) } };
            grid.Add(star); Grid.SetColumn(name, 1); grid.Add(name); var values = new VerticalStackLayout { Spacing = 2, Children = { sell, trend } }; Grid.SetColumn(values, 2); grid.Add(values); return grid;
        }) };
        var root = new Grid { Padding = new Thickness(14, 0), RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Star) } };
        root.Add(SalihUi.Header("Favorilerim", "Düzenle"));
        var tabs = new Grid { HeightRequest = 40, BackgroundColor = Color.FromArgb("#F5F3EF"), ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) } };
        tabs.Add(new Border { Background = SalihUi.GoldGradient(), StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = 20 }, Content = Centered("Altın", Colors.White) }); var fx = Centered("Döviz", SalihUi.Ink); Grid.SetColumn(fx, 1); tabs.Add(fx); Grid.SetRow(tabs, 1); root.Add(tabs);
        Grid.SetRow(list, 2); root.Add(list); return root;
    }

    private static Label Centered(string text, Color color) { var label = SalihUi.Text(text, 12, false, color); label.HorizontalTextAlignment = TextAlignment.Center; return label; }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ReloadAsync();
        ApplyTheme();
    }

    private async Task ReloadAsync()
    {
        await liveRatesStore.EnsureLoadedAsync(CancellationToken.None);
        RefreshFavorites();
    }

    private void RefreshFavorites()
    {
        favorites.Clear();
        foreach (var item in liveRatesStore.GetFavoriteRates())
        {
            favorites.Add(new RateDisplayItem(item, true));
        }

        emptyLabel.IsVisible = favorites.Count == 0;
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
}
