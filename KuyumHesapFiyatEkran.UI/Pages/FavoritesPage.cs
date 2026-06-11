using System.Collections.ObjectModel;
using KuyumHesapFiyatEkran.UI.Models;
using KuyumHesapFiyatEkran.UI.Services;
using Microsoft.Maui.Controls.Shapes;

namespace KuyumHesapFiyatEkran.UI.Pages;

public sealed class FavoritesPage : ContentPage
{
    private readonly LiveRatesStore liveRatesStore;
    private readonly ObservableCollection<RateDisplayItem> favorites = new();
    private readonly Label emptyLabel = new()
    {
        Text = "Henuz favori eklenmedi. Canli Fiyatlar ekraninda yildiz simgesine dokunarak favori listenizi olusturabilirsiniz.",
        FontSize = 15,
        TextColor = Color.FromArgb("#9A7417"),
        HorizontalTextAlignment = TextAlignment.Center
    };

    public FavoritesPage(LiveRatesStore liveRatesStore)
    {
        this.liveRatesStore = liveRatesStore;
        Title = "Favoriler";
        BackgroundColor = Color.FromArgb("#FFFFFF");
        liveRatesStore.FavoritesChanged += (_, _) => RefreshFavorites();

        var collection = new CollectionView
        {
            ItemsSource = favorites,
            SelectionMode = SelectionMode.None,
            ItemTemplate = new DataTemplate(() =>
            {
                var name = new Label { FontSize = 17, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#2F2A1D") };
                name.SetBinding(Label.TextProperty, nameof(RateDisplayItem.Name));
                var code = new Label { FontSize = 12, TextColor = Color.FromArgb("#9A7417") };
                code.SetBinding(Label.TextProperty, nameof(RateDisplayItem.Code));
                code.SetBinding(IsVisibleProperty, nameof(RateDisplayItem.HasCode));
                var buy = new Label { FontSize = 14, FontAttributes = FontAttributes.Bold, HorizontalTextAlignment = TextAlignment.End, TextColor = Color.FromArgb("#2F2A1D") };
                buy.SetBinding(Label.TextProperty, nameof(RateDisplayItem.Buy));
                var sell = new Label { FontSize = 14, FontAttributes = FontAttributes.Bold, HorizontalTextAlignment = TextAlignment.End, TextColor = Color.FromArgb("#2F2A1D") };
                sell.SetBinding(Label.TextProperty, nameof(RateDisplayItem.Sell));
                var priceStack = new VerticalStackLayout
                {
                    Spacing = 4,
                    Children =
                    {
                        new Label { Text = "Alış", FontSize = 11, TextColor = Color.FromArgb("#9A7417"), HorizontalTextAlignment = TextAlignment.End },
                        buy,
                        new Label { Text = "Satış", FontSize = 11, TextColor = Color.FromArgb("#9A7417"), HorizontalTextAlignment = TextAlignment.End },
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
                row.Add(new VerticalStackLayout { Spacing = 3, Children = { name, code } });
                row.Add(priceStack);

                return new Border
                {
                    Margin = new Thickness(0, 5),
                    Padding = 14,
                    Stroke = Color.FromArgb("#D4AF37"),
                    BackgroundColor = Color.FromArgb("#FFFFFF"),
                    StrokeShape = new RoundRectangle { CornerRadius = 8 },
                    Content = row
                };
            })
        };

        var header = new Label { Text = "Favoriler", FontSize = 24, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#111111") };
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
        Content = root;
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
