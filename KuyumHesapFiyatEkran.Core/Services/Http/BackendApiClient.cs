using System.Net.Http.Json;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using KuyumHesapFiyatEkran.Core.Abstractions.Http;
using KuyumHesapFiyatEkran.Core.Common.Models;
using KuyumHesapFiyatEkran.Core.Serialization;

namespace KuyumHesapFiyatEkran.Core.Services.Http;

internal sealed class BackendApiClient(HttpClient httpClient) : IBackendApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new TurkishDecimalJsonConverter(), new NullableTurkishDecimalJsonConverter() }
    };

    public async Task<ResponseDto<TResponse>> GetAsync<TResponse>(string requestUri, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync(requestUri, cancellationToken);
        return await ReadResponseAsync<TResponse>(response, cancellationToken);
    }

    public async Task<ResponseDto<TResponse>> PostAsync<TRequest, TResponse>(string requestUri, TRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync(requestUri, request, JsonOptions, cancellationToken);
        return await ReadResponseAsync<TResponse>(response, cancellationToken);
    }

    public async Task<ResponseDto<TResponse>> PutAsync<TRequest, TResponse>(string requestUri, TRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PutAsJsonAsync(requestUri, request, JsonOptions, cancellationToken);
        return await ReadResponseAsync<TResponse>(response, cancellationToken);
    }

    public async Task<ResponseDto<TResponse>> DeleteAsync<TResponse>(string requestUri, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.DeleteAsync(requestUri, cancellationToken);
        return await ReadResponseAsync<TResponse>(response, cancellationToken);
    }

    private static async Task<ResponseDto<TResponse>> ReadResponseAsync<TResponse>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.Content.Headers.ContentLength == 0)
        {
            return response.IsSuccessStatusCode
                ? new ResponseDto<TResponse>().Success()
                : new ResponseDto<TResponse>().Fail("Backend servisinden bos cevap dondu.", (int)response.StatusCode);
        }

        try
        {
            var dto = await response.Content.ReadFromJsonAsync<ResponseDto<TResponse>>(JsonOptions, cancellationToken);
            if (dto is not null)
            {
                dto.StatusCode = dto.StatusCode == 0 ? (int)response.StatusCode : dto.StatusCode;
                dto.IsSuccess = dto.IsSuccess && response.IsSuccessStatusCode;
                return dto;
            }
        }
        catch (JsonException)
        {
            // The backend contract is ResponseDto<T>; this fallback keeps callers in that shape.
        }

        var message = response.IsSuccessStatusCode
            ? "Backend cevabi beklenen ResponseDto formatinda degil."
            : $"Backend istegi basarisiz oldu. StatusCode: {(int)response.StatusCode}";

        return new ResponseDto<TResponse>().Fail(message, (int)response.StatusCode);
    }

    private sealed class TurkishDecimalJsonConverter : JsonConverter<decimal>
    {
        private static readonly CultureInfo TurkishCulture = CultureInfo.GetCultureInfo("tr-TR");

        public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number && reader.TryGetDecimal(out var number))
            {
                return number;
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                var value = reader.GetString();
                if (decimal.TryParse(value, NumberStyles.Number, TurkishCulture, out var turkishNumber))
                {
                    return turkishNumber;
                }

                if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var invariantNumber))
                {
                    return invariantNumber;
                }
            }

            throw new JsonException("Decimal degeri beklenen formatta degil.");
        }

        public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }
}
