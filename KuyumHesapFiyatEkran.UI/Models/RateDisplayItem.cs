using KuyumHesapFiyatEkran.Core.Features.LiveRates.Queries.GetLiveRates;

namespace KuyumHesapFiyatEkran.UI.Models;

public sealed class RateDisplayItem
{
    public RateDisplayItem(LiveRateItem rate, bool isFavorite)
    {
        Rate = rate;
        IsFavorite = isFavorite;
    }

    public LiveRateItem Rate { get; }

    public string Name => Rate.Name;

    public string? Code => Rate.Code;

    public string Buy => Rate.Buy;

    public string Sell => Rate.Sell;

    public string ChangePercent => Rate.ChangePercent;

    public string TrendText => Rate.TrendText;

    public bool IsUp => Rate.IsUp;

    public bool HasCode => Rate.HasCode;

    public bool IsAlternateRow => Rate.IsAlternateRow;

    public bool IsFavorite { get; }

    public string FavoriteText => IsFavorite ? "★" : "☆";
}
