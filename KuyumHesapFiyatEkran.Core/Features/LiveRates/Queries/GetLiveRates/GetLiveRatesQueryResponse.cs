using System.Text.Json.Serialization;
using KuyumHesapFiyatEkran.Core.Features.LiveRates;
using KuyumHesapFiyatEkran.Core.Serialization;

namespace KuyumHesapFiyatEkran.Core.Features.LiveRates.Queries.GetLiveRates;

public sealed class GetLiveRatesQueryResponse
{
    public string LastUpdate { get; set; } = string.Empty;

    public List<LiveRateItem> Sarrafiye { get; set; } = new();

    public List<LiveRateItem> Maden { get; set; } = new();

    public List<LiveRateItem> Doviz { get; set; } = new();
}

public sealed class LiveRateItem
{
    public string Name { get; set; } = string.Empty;

    public string? Code { get; set; }

    [JsonConverter(typeof(FlexibleStringJsonConverter))]
    public string Buy { get; set; } = string.Empty;

    [JsonConverter(typeof(FlexibleStringJsonConverter))]
    public string Sell { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? ChangePercent { get; set; }

    public decimal? SellChangePercent { get; set; }

    public bool IsUp { get; set; }

    [JsonIgnore]
    public string ChangeDisplay => RateChangeFormatter.Format(SellChangePercent, ChangePercent);

    [JsonIgnore]
    public bool HasPositiveChange => RateChangeFormatter.GetDirection(SellChangePercent, ChangePercent) > 0;

    [JsonIgnore]
    public bool HasNegativeChange => RateChangeFormatter.GetDirection(SellChangePercent, ChangePercent) < 0;

    [JsonIgnore]
    public bool IsChangeNeutral => !HasPositiveChange && !HasNegativeChange;

    [JsonIgnore]
    public string TrendText
    {
        get
        {
            if (HasPositiveChange)
            {
                return $"↑ {ChangeDisplay}";
            }

            if (HasNegativeChange)
            {
                return $"↓ {ChangeDisplay}";
            }

            return ChangeDisplay;
        }
    }

    public bool HasCode => !string.IsNullOrWhiteSpace(Code);

    public bool IsAlternateRow { get; set; }
}
