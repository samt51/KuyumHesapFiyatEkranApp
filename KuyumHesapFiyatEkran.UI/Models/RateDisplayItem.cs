using KuyumHesapFiyatEkran.Core.Features.LiveRates;
using KuyumHesapFiyatEkran.Core.Features.LiveRates.Queries.GetLiveRates;

namespace KuyumHesapFiyatEkran.UI.Models;

public sealed class RateDisplayItem
{
    public RateDisplayItem(LiveRateItem rate, bool isFavorite, DateTimeOffset? lastSuccessfulRefresh = null)
    {
        Rate = rate;
        IsFavorite = isFavorite;
        UpdateTime = lastSuccessfulRefresh?.ToOffset(TimeSpan.FromHours(3))
            .ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture) ?? "--:--";
    }

    public string UpdateTime { get; }

    public string UpdateTimeAccessibility => $"Son başarılı fiyat güncellemesi: {UpdateTime}";

    public LiveRateItem Rate { get; }

    public string Name => Rate.Name;

    public string DisplayName => System.Globalization.CultureInfo.GetCultureInfo("tr-TR").TextInfo.ToTitleCase(Rate.Name.ToLowerInvariant());

    public string? Code => Rate.Code;

    public string Buy => Rate.Buy;

    public string BuyDisplay => AddCurrency(Rate.Buy);

    public string Sell => Rate.Sell;

    public string SellDisplay => AddCurrency(Rate.Sell);

    public string ChangePercent => Rate.ChangeDisplay;

    public string TrendText => Rate.TrendText;

    public bool IsUp => Rate.HasPositiveChange;

    public bool IsDown => Rate.HasNegativeChange;

    public bool IsChangeNeutral => Rate.IsChangeNeutral;

    public string ChangeAccessibility => RateChangeFormatter.SellChangeAccessibility;

    public bool HasCode => Rate.HasCode;

    public bool IsAlternateRow => Rate.IsAlternateRow;

    public bool IsFavorite { get; }

    public bool IsNotFavorite => !IsFavorite;

    private static string AddCurrency(string value) => string.IsNullOrWhiteSpace(value) || value.Contains("TL", StringComparison.OrdinalIgnoreCase) ? value : $"{value} TL";
}
