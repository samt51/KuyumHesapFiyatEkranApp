using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KuyumHesapFiyatEkran.Core.Serialization;

public sealed class FlexibleStringJsonConverter : JsonConverter<string>
{
    private static readonly CultureInfo TurkishCulture = CultureInfo.GetCultureInfo("tr-TR");

    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                return string.Empty;
            case JsonTokenType.String:
                return reader.GetString() ?? string.Empty;
            case JsonTokenType.Number:
                if (!reader.TryGetDecimal(out var number))
                {
                    return reader.GetDouble().ToString(CultureInfo.InvariantCulture);
                }

                var rounded = decimal.Round(number, 2, MidpointRounding.AwayFromZero);
                return rounded == decimal.Truncate(rounded)
                    ? ((long)rounded).ToString("N0", TurkishCulture)
                    : rounded.ToString("N2", TurkishCulture);
            default:
                throw new JsonException("Metin degeri beklenen formatta degil.");
        }
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}

public sealed class NullableTurkishDecimalJsonConverter : JsonConverter<decimal?>
{
    private static readonly CultureInfo TurkishCulture = CultureInfo.GetCultureInfo("tr-TR");

    public override decimal? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType == JsonTokenType.Number)
        {
            if (reader.TryGetDecimal(out var number))
            {
                return number;
            }

            if (reader.TryGetDouble(out var floating))
            {
                return (decimal)floating;
            }
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var value = reader.GetString();
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

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

    public override void Write(Utf8JsonWriter writer, decimal? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteNumberValue(value.Value);
    }
}
