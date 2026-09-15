using KuyumHesapFiyatEkran.Core.Features.LiveRates.Queries.GetLiveRates;
using KuyumHesapFiyatEkran.UI.Services;
using KuyumHesapFiyatEkran.UI.Views;
using Microsoft.Maui.Controls.Shapes;

namespace KuyumHesapFiyatEkran.UI.Pages;

public sealed class ProductSelectionPage : ContentPage
{
    public ProductSelectionPage(string categoryTitle, IReadOnlyList<LiveRateItem> items, string? selectedKey, Action<LiveRateItem> selected)
    {
        Shell.SetNavBarIsVisible(this, false); BackgroundColor = SalihUi.Cream;
        var stack = new VerticalStackLayout { Padding = new Thickness(20, 12, 20, 24), Spacing = 10 };
        var header = new Grid { HeightRequest = 52, ColumnDefinitions = { new ColumnDefinition(new GridLength(40)), new ColumnDefinition(GridLength.Star), new ColumnDefinition(new GridLength(40)) } };
        var close = new Grid { Children = { new LucideIcon(LucideIconKind.ChevronLeft, 23, SalihUi.Ink) } };
        var closeTap = new TapGestureRecognizer(); closeTap.Tapped += async (_, _) => await Navigation.PopModalAsync(); close.GestureRecognizers.Add(closeTap); header.Add(close);
        var title = SalihUi.Text($"{categoryTitle} Türü Seç", 20, true); title.HorizontalTextAlignment = TextAlignment.Center; Grid.SetColumn(title, 1); header.Add(title); stack.Add(header);

        foreach (var item in items)
        {
            var key = LiveRatesStore.GetRateKey(item); var isSelected = key == selectedKey;
            var row = new Grid { HeightRequest = 62, Padding = new Thickness(12, 8), ColumnDefinitions = { new ColumnDefinition(new GridLength(40)), new ColumnDefinition(GridLength.Star), new ColumnDefinition(new GridLength(28)) } };
            row.Add(new LucideIcon(LucideIconKind.CircleDollarSign, 25, SalihUi.Gold));
            var name = SalihUi.Text(DisplayName(item.Name), 15, true); Grid.SetColumn(name, 1); row.Add(name);
            var end = new LucideIcon(isSelected ? LucideIconKind.Check : LucideIconKind.ChevronRight, 20, isSelected ? SalihUi.Gold : SalihUi.Muted); Grid.SetColumn(end, 2); row.Add(end);
            var card = SalihUi.Card(row, radius: 14, padding: new Thickness(0)); var tap = new TapGestureRecognizer();
            tap.Tapped += async (_, _) => { selected(item); await Navigation.PopModalAsync(); }; card.GestureRecognizers.Add(tap); stack.Add(card);
        }
        Content = new ScrollView { Content = stack };
    }

    private static string DisplayName(string name) => System.Globalization.CultureInfo.GetCultureInfo("tr-TR").TextInfo.ToTitleCase(name.ToLowerInvariant());
}
