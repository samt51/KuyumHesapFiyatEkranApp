using System.Globalization;
using KuyumHesapFiyatEkran.Core.Features.LiveRates.Queries.GetLiveRateScreen;
using KuyumHesapFiyatEkran.Core.Features.LiveRates.Queries.GetLiveRates;
using MediatR;

namespace KuyumHesapFiyatEkran.UI.Services;

public sealed class LiveRatesStore
{
    private const string FavoritesKey = "favorite-rate-keys";
    private static readonly CultureInfo TurkishCulture = CultureInfo.GetCultureInfo("tr-TR");
    private readonly IMediator mediator;
    private readonly BrandSettings brandSettings;
    private readonly HashSet<string> favoriteKeys = new(StringComparer.OrdinalIgnoreCase);

    public LiveRatesStore(IMediator mediator, BrandSettings brandSettings)
    {
        this.mediator = mediator;
        this.brandSettings = brandSettings;
        LoadFavorites();
    }

    public event EventHandler? FavoritesChanged;

    public GetLiveRatesQueryResponse? CurrentRates { get; private set; }

    public DateTimeOffset? LastSuccessfulRefresh { get; private set; }

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

            var rates = response.Data;
            await MergeSalihScreenRatesAsync(rates, cancellationToken);
            var isSalih = string.Equals(brandSettings.Key, "SalihCetinkaya", StringComparison.OrdinalIgnoreCase);
            if (isSalih && rates.Sarrafiye.Count == 0 && rates.Maden.Count == 0 && rates.Doviz.Count == 0)
            {
                LastError = "Canli fiyat listesi bos dondu. Panelde urun ve kur kaynagi kontrol edilmeli.";
                return CurrentRates;
            }

            CurrentRates = rates;
            if (isSalih)
            {
                LastSuccessfulRefresh = DateTimeOffset.UtcNow;
            }

            var hasAnyRate = CurrentRates.Sarrafiye.Count > 0
                || CurrentRates.Maden.Count > 0
                || CurrentRates.Doviz.Count > 0;

            if (!hasAnyRate)
            {
                LastError = "Canli fiyat listesi bos dondu. Panelde urun ve kur kaynagi kontrol edilmeli.";
            }

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

        var lastComma = cleaned.LastIndexOf(',');
        var lastDot = cleaned.LastIndexOf('.');
        string normalized;

        if (lastComma >= 0 && lastDot >= 0)
        {
            normalized = lastComma > lastDot
                ? cleaned.Replace(".", string.Empty).Replace(',', '.')
                : cleaned.Replace(",", string.Empty);
        }
        else if (lastComma >= 0)
        {
            normalized = cleaned.Replace(',', '.');
        }
        else if (lastDot >= 0)
        {
            var fractionLength = cleaned.Length - lastDot - 1;
            var dotCount = cleaned.Count(ch => ch == '.');
            normalized = dotCount > 1 || fractionLength == 3
                ? cleaned.Replace(".", string.Empty)
                : cleaned;
        }
        else
        {
            normalized = cleaned;
        }

        return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var result)
            ? result
            : 0;
    }

    public static string FormatCurrency(decimal value)
    {
        var culture = CultureInfo.GetCultureInfo("tr-TR");
        var rounded = decimal.Round(value, 2, MidpointRounding.AwayFromZero);
        var formatted = rounded == decimal.Truncate(rounded)
            ? ((long)rounded).ToString("N0", culture)
            : rounded.ToString("N2", culture);
        return $"{formatted} TL";
    }

    public static string GetRateKey(LiveRateItem item)
    {
        return string.IsNullOrWhiteSpace(item.Code)
            ? item.Name.Trim()
            : $"{item.Code.Trim()}|{item.Name.Trim()}";
    }

    private async Task MergeSalihScreenRatesAsync(GetLiveRatesQueryResponse rates, CancellationToken cancellationToken)
    {
        if (!string.Equals(brandSettings.Key, "SalihCetinkaya", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var screenResponse = await mediator.Send(new GetLiveRateScreenQuery(), cancellationToken);
        if (!screenResponse.IsSuccess || screenResponse.Data?.Columns is null)
        {
            return;
        }

        var existingNames = new HashSet<string>(
            rates.Sarrafiye.Concat(rates.Maden).Concat(rates.Doviz).Select(item => NormalizeName(item.Name)),
            StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(screenResponse.Data.LastUpdate) && string.IsNullOrWhiteSpace(rates.LastUpdate))
        {
            rates.LastUpdate = screenResponse.Data.LastUpdate;
        }

        foreach (var item in screenResponse.Data.Columns.SelectMany(column => column.Items))
        {
            if (item.Buy == 0 && item.Sell == 0)
            {
                continue;
            }

            var name = item.Title?.Trim() ?? string.Empty;
            // Keep buy, sell and change from this screen item only; do not overlay onto Cure rows.
            if (string.IsNullOrWhiteSpace(name) || !existingNames.Add(NormalizeName(name)))
            {
                continue;
            }

            var rate = new LiveRateItem
            {
                Name = name,
                Buy = FormatScreenPrice(item.Buy),
                Sell = FormatScreenPrice(item.Sell),
                ChangePercent = string.IsNullOrWhiteSpace(item.ChangePercent) ? null : item.ChangePercent.Trim(),
                SellChangePercent = item.SellChangePercent,
                IsUp = item.IsUp
            };

            GetCategoryList(rates, name).Add(rate);
        }

        RenumberRows(rates.Sarrafiye);
        RenumberRows(rates.Maden);
        RenumberRows(rates.Doviz);
    }

    private static List<LiveRateItem> GetCategoryList(GetLiveRatesQueryResponse rates, string name)
    {
        var normalized = NormalizeName(name);

        if (normalized.Contains("GUMUS", StringComparison.Ordinal) || normalized.Contains("GÜMÜS", StringComparison.Ordinal))
        {
            return rates.Maden;
        }

        if (normalized.Contains("DOLAR", StringComparison.Ordinal)
            || normalized.Contains("EURO", StringComparison.Ordinal)
            || normalized.Contains("STERLIN", StringComparison.Ordinal)
            || normalized.Contains("FRANG", StringComparison.Ordinal)
            || normalized.Contains("RIYAL", StringComparison.Ordinal)
            || normalized.Contains("LIRA", StringComparison.Ordinal))
        {
            return rates.Doviz;
        }

        return rates.Sarrafiye;
    }

    private static void RenumberRows(List<LiveRateItem> items)
    {
        for (var index = 0; index < items.Count; index++)
        {
            items[index].IsAlternateRow = index % 2 == 1;
        }
    }

    private static string NormalizeName(string value) => value
        .Replace("İ", "I", StringComparison.Ordinal)
        .Replace("ı", "i", StringComparison.Ordinal)
        .Replace("Ş", "S", StringComparison.Ordinal)
        .Replace("ş", "s", StringComparison.Ordinal)
        .Replace("Ğ", "G", StringComparison.Ordinal)
        .Replace("ğ", "g", StringComparison.Ordinal)
        .Replace("Ü", "U", StringComparison.Ordinal)
        .Replace("ü", "u", StringComparison.Ordinal)
        .Replace("Ö", "O", StringComparison.Ordinal)
        .Replace("ö", "o", StringComparison.Ordinal)
        .Replace("Ç", "C", StringComparison.Ordinal)
        .Replace("ç", "c", StringComparison.Ordinal)
        .Trim()
        .ToUpperInvariant();

    private static string FormatScreenPrice(decimal value)
    {
        var rounded = decimal.Round(value, 2, MidpointRounding.AwayFromZero);
        return rounded == decimal.Truncate(rounded)
            ? ((long)rounded).ToString("N0", TurkishCulture)
            : rounded.ToString("N2", TurkishCulture);
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
