using KuyumHesapFiyatEkran.Core.Common.Models;

namespace KuyumHesapFiyatEkran.Core.Abstractions.Http;

public interface IBackendApiClient
{
    Task<ResponseDto<TResponse>> GetAsync<TResponse>(string requestUri, CancellationToken cancellationToken = default);

    Task<ResponseDto<TResponse>> PostAsync<TRequest, TResponse>(string requestUri, TRequest request, CancellationToken cancellationToken = default);

    Task<ResponseDto<TResponse>> PutAsync<TRequest, TResponse>(string requestUri, TRequest request, CancellationToken cancellationToken = default);

    Task<ResponseDto<TResponse>> DeleteAsync<TResponse>(string requestUri, CancellationToken cancellationToken = default);
}
