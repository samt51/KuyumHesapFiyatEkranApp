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

    public decimal Buy { get; set; }

    public decimal Sell { get; set; }

    public string? Description { get; set; }

    public string ChangePercent { get; set; } = "0,00%";

    public bool IsUp { get; set; } = true;

    public string TrendText => $"{(IsUp ? "↑" : "↓")} {ChangePercent}";

    public bool HasCode => !string.IsNullOrWhiteSpace(Code);

    public bool IsAlternateRow { get; set; }
}
