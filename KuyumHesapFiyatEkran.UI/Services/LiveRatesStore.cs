using System.Globalization;
using KuyumHesapFiyatEkran.Core.Features.LiveRates.Queries.GetLiveRates;
using MediatR;

namespace KuyumHesapFiyatEkran.UI.Services;

public sealed class LiveRatesStore
{
    private const string FavoritesKey = "favorite-rate-keys";
    private readonly IMediator mediator;
    private readonly HashSet<string> favoriteKeys = new(StringComparer.OrdinalIgnoreCase);

    public LiveRatesStore(IMediator mediator)
    {
        this.mediator = mediator;
        LoadFavorites();
    }

    public event EventHandler? FavoritesChanged;

    public GetLiveRatesQueryResponse? CurrentRates { get; private set; }

    public string? LastError { get; private set; }

    public async Task<GetLiveRatesQueryResponse?> LoadAsync(CancellationToken cancellationToken)
    {
        try
        {
            LastError = null;
            var response = await mediator.Send(new GetLiveRatesQuery(), cancellationToken);

            if (!response.IsSuccess || response.Data is null)
            {
                LastError = response.Errors?.Count > 0
                    ? string.Join(Environment.NewLine, response.Errors)
                    : "Canli fiyat bilgileri alinamadi.";
                return CurrentRates;
            }

            CurrentRates = response.Data;
            return CurrentRates;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            return CurrentRates;
        }
    }

    public async Task<GetLiveRatesQueryResponse?> EnsureLoadedAsync(CancellationToken cancellationToken)
    {
        return CurrentRates ?? await LoadAsync(cancellationToken);
    }

    public IReadOnlyList<LiveRateItem> GetAllRates()
    {
        if (CurrentRates is null)
        {
            return Array.Empty<LiveRateItem>();
        }

        return CurrentRates.Sarrafiye
            .Concat(CurrentRates.Maden)
            .Concat(CurrentRates.Doviz)
            .ToList();
    }

    public IReadOnlyList<LiveRateItem> GetFavoriteRates()
    {
        return GetAllRates()
            .Where(item => IsFavorite(item))
            .ToList();
    }

    public bool IsFavorite(LiveRateItem item)
    {
        return favoriteKeys.Contains(GetRateKey(item));
    }

    public void ToggleFavorite(LiveRateItem item)
    {
        var key = GetRateKey(item);

        if (!favoriteKeys.Add(key))
        {
            favoriteKeys.Remove(key);
        }

        SaveFavorites();
        FavoritesChanged?.Invoke(this, EventArgs.Empty);
    }

    public LiveRateItem? FindRate(params string[] namesOrCodes)
    {
        var candidates = namesOrCodes
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToArray();

        return GetAllRates().FirstOrDefault(item =>
            candidates.Any(candidate =>
                string.Equals(item.Code, candidate, StringComparison.OrdinalIgnoreCase)
                || item.Name.Contains(candidate, StringComparison.OrdinalIgnoreCase)));
    }

    public static decimal ParsePrice(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 0;
        }

        var cleaned = new string(value.Where(ch => char.IsDigit(ch) || ch == ',' || ch == '.').ToArray());
        if (string.IsNullOrWhiteSpace(cleaned))
        {
            return 0;
        }

        var normalized = cleaned;
        var lastComma = normalized.LastIndexOf(',');
        var lastDot = normalized.LastIndexOf('.');

        if (lastComma > lastDot)
        {
            normalized = normalized.Replace(".", string.Empty).Replace(',', '.');
        }
        else if (lastDot > lastComma)
        {
            normalized = normalized.Replace(",", string.Empty);
        }

        return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var result)
            ? result
            : 0;
    }

    public static string FormatCurrency(decimal value)
    {
        return string.Create(CultureInfo.GetCultureInfo("tr-TR"), $"{value:N2} TL");
    }

    public static string GetRateKey(LiveRateItem item)
    {
        return string.IsNullOrWhiteSpace(item.Code)
            ? item.Name.Trim()
            : $"{item.Code.Trim()}|{item.Name.Trim()}";
    }

    private void LoadFavorites()
    {
        var saved = Preferences.Default.Get(FavoritesKey, string.Empty);
        foreach (var key in saved.Split(";;", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            favoriteKeys.Add(key);
        }
    }

    private void SaveFavorites()
    {
        Preferences.Default.Set(FavoritesKey, string.Join(";;", favoriteKeys.OrderBy(key => key)));
    }
}
