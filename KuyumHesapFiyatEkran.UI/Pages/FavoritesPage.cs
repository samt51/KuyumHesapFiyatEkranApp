using System.Collections.ObjectModel;
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
            var pageGrid = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition(GridLength.Star),
                    new RowDefinition(GridLength.Auto)
                }
            };
            pageGrid.Add(root);

            var bottomBar = new BottomNavigationBar(1);
            Grid.SetRow(bottomBar, 1);
            pageGrid.Add(bottomBar);

            Content = pageGrid;
        }
        else
        {
            Content = root;
        }

        ApplyTheme();
        AppThemeManager.ThemeChanged += OnThemeChanged;
    }

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
