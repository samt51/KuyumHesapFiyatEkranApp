using KuyumHesapFiyatEkran.Core.Abstractions.Http;
using KuyumHesapFiyatEkran.Core.Common.Models;
using MediatR;

namespace KuyumHesapFiyatEkran.Core.Features.LiveRates.Queries.GetLiveRateScreen;

internal sealed class GetLiveRateScreenQueryHandler(IBackendApiClient backendApiClient)
    : IRequestHandler<GetLiveRateScreenQuery, ResponseDto<LiveRateScreenResponse>>
{
    private const string Endpoint = "api/LiveRate/screen";

    public Task<ResponseDto<LiveRateScreenResponse>> Handle(GetLiveRateScreenQuery request, CancellationToken cancellationToken)
    {
        return backendApiClient.GetAsync<LiveRateScreenResponse>(Endpoint, cancellationToken);
    }
}
