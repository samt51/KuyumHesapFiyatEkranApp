using KuyumHesapFiyatEkran.Core.Common.Models;
using MediatR;

namespace KuyumHesapFiyatEkran.Core.Features.LiveRates.Queries.GetLiveRateScreen;

public sealed class GetLiveRateScreenQuery : IRequest<ResponseDto<LiveRateScreenResponse>>
{
}

public sealed class LiveRateScreenResponse
{
    public List<LiveRateScreenColumn> Columns { get; set; } = new();

    public string LastUpdate { get; set; } = string.Empty;
}

public sealed class LiveRateScreenColumn
{
    public int ColumnIndex { get; set; }

    public List<LiveRateScreenItem> Items { get; set; } = new();
}

public sealed class LiveRateScreenItem
{
    public string Title { get; set; } = string.Empty;

    public decimal Buy { get; set; }

    public decimal Sell { get; set; }

    public string? ChangePercent { get; set; }

    public decimal? SellChangePercent { get; set; }

    public bool IsUp { get; set; }
}
