using System.Globalization;
using KuyumHesapFiyatEkran.UI.Brands.MazlumKuyumculuk.Services;
using KuyumHesapFiyatEkran.UI.Services.Firebase;

namespace KuyumHesapFiyatEkran.UI.Brands.MazlumKuyumculuk.Pages;

public partial class MazlumDetailPage : ContentPage
{
    private readonly MazlumFirebasePrice item;
    private readonly MazlumFavoritesService favoritesService;

    public MazlumDetailPage(
        MazlumFirebasePrice item,
        MazlumFavoritesService favoritesService)
    {
        InitializeComponent();

        this.item = item;
        this.favoritesService = favoritesService;

        BindItem();
        RefreshFavorite();
    }

    private void BindItem()
    {
        ProductNameLabel.Text = item.Name;

        BuyLabel.Text =
            item.Buy > 0
                ? FormatPrice(item.Buy, item.Unit)
                : "—";

        SellLabel.Text =
            FormatPrice(item.Sell, item.Unit);

        DifferenceLabel.Text =
            item.Buy > 0
                ? FormatPrice(
                    item.Sell - item.Buy,
                    item.Unit)
                : "—";

        BuyChangeLabel.Text =
            FormatChange(item.BuyChangePercent);

        BuyChangeLabel.TextColor =
            GetChangeColor(item.BuyChangePercent);

        SellChangeLabel.Text =
            FormatChange(item.SellChangePercent);

        SellChangeLabel.TextColor =
            GetChangeColor(item.SellChangePercent);
    }

    private void RefreshFavorite()
    {
        FavoriteIcon.Text =
            favoritesService.IsFavorite(item.Id)
                ? "★"
                : "☆";
    }

    private async void OnBackTapped(
        object sender,
        TappedEventArgs e)
    {
        await Navigation.PopAsync();
    }

    private void OnFavoriteTapped(
        object sender,
        TappedEventArgs e)
    {
        var isFavorite =
            favoritesService.Toggle(item.Id);

        FavoriteIcon.Text =
            isFavorite
                ? "★"
                : "☆";
    }

    private static string FormatPrice(
        decimal value,
        string unit)
    {
        var culture =
            CultureInfo.GetCultureInfo("tr-TR");

        var formatted =
            value.ToString("#,##0.##", culture);

        return string.IsNullOrWhiteSpace(unit)
            ? formatted
            : $"{formatted} {unit}";
    }

    private static string FormatChange(
        decimal? value)
    {
        if (!value.HasValue)
            return string.Empty;

        var prefix =
            value.Value > 0
                ? "▲ "
                : value.Value < 0
                    ? "▼ "
                    : string.Empty;

        return
            prefix +
            value.Value.ToString(
                "0.##",
                CultureInfo.InvariantCulture) +
            "%";
    }

    private static Color GetChangeColor(
        decimal? value)
    {
        if (!value.HasValue ||
            value.Value == 0)
        {
            return Color.FromArgb("#777C84");
        }

        return value.Value > 0
            ? Color.FromArgb("#3DCF8E")
            : Color.FromArgb("#EF6B6B");
    }
}
