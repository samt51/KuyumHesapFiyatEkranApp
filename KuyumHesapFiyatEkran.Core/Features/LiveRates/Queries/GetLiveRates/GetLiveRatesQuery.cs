using KuyumHesapFiyatEkran.Core.Common.Models;
using MediatR;

namespace KuyumHesapFiyatEkran.Core.Features.LiveRates.Queries.GetLiveRates;

public sealed class GetLiveRatesQuery : IRequest<ResponseDto<GetLiveRatesQueryResponse>>
{
}

public sealed class GetLiveRatesQueryRequest
{
}
