using KuyumHesapFiyatEkran.Core.Abstractions.Http;
using KuyumHesapFiyatEkran.Core.Common.Models;
using MediatR;

namespace KuyumHesapFiyatEkran.Core.Features.LiveRates.Queries.GetLiveRates;

internal sealed class GetLiveRatesQueryHandler(IBackendApiClient backendApiClient)
    : IRequestHandler<GetLiveRatesQuery, ResponseDto<GetLiveRatesQueryResponse>>
{
    private const string Endpoint = "api/Cure";

    public Task<ResponseDto<GetLiveRatesQueryResponse>> Handle(GetLiveRatesQuery request, CancellationToken cancellationToken)
    {
        return backendApiClient.GetAsync<GetLiveRatesQueryResponse>(Endpoint, cancellationToken);
    }
}
