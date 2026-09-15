using System.Globalization;

namespace KuyumHesapFiyatEkran.Core.Features.LiveRates;

public static class RateChangeFormatter
{
    public const string MissingDisplay = "—";
    public const string SellChangeAccessibility = "Satış fiyatının yüzde değişimi";

    private static readonly CultureInfo TurkishCulture = CultureInfo.GetCultureInfo("tr-TR");

    public static string Format(decimal? sellChangePercent, string? changePercentText)
    {
        if (sellChangePercent.HasValue)
        {
            return FormatNumeric(sellChangePercent.Value);
        }

        return string.IsNullOrWhiteSpace(changePercentText)
            ? MissingDisplay
            : changePercentText.Trim();
    }

    public static int GetDirection(decimal? sellChangePercent, string? changePercentText)
    {
        if (sellChangePercent.HasValue)
        {
            return SignOf(RoundForDisplay(sellChangePercent.Value));
        }

        var parsed = TryParsePercentText(changePercentText);
        return parsed.HasValue ? SignOf(RoundForDisplay(parsed.Value)) : 0;
    }

    public static string FormatNumeric(decimal value)
    {
        var rounded = RoundForDisplay(value);
        var magnitude = Math.Abs(rounded).ToString("0.00", TurkishCulture);

        if (rounded > 0)
        {
            return $"+%{magnitude}";
        }

        if (rounded < 0)
        {
            return $"\u2212%{magnitude}";
        }

        return "%0,00";
    }

    public static decimal? TryParsePercentText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim()
            .Replace("%", string.Empty, StringComparison.Ordinal)
            .Replace("\u2212", "-", StringComparison.Ordinal)
            .Trim()
            .TrimStart('+');

        if (decimal.TryParse(trimmed, NumberStyles.Number | NumberStyles.AllowLeadingSign, TurkishCulture, out var turkishNumber))
        {
            return turkishNumber;
        }

        if (decimal.TryParse(trimmed, NumberStyles.Number | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var invariantNumber))
        {
            return invariantNumber;
        }

        return null;
    }

    private static decimal RoundForDisplay(decimal value) =>
        decimal.Round(value, 2, MidpointRounding.AwayFromZero);

    private static int SignOf(decimal value) => value > 0 ? 1 : value < 0 ? -1 : 0;
}
