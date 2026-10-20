namespace KuyumHesapFiyatEkran.UI.Services.Firebase;

public sealed class MazlumFirebaseKurItem
{
    public string Code { get; set; } = string.Empty;
    public decimal Alis { get; set; }
    public decimal Satis { get; set; }
    public string Source { get; set; } = string.Empty;

    public string BuyDisplay =>
        $"{Code} · {Format(Alis)}";

    public string SellDisplay =>
        $"{Code} · {Format(Satis)}";

    private static string Format(decimal value)
    {
        return value.ToString(
            "0.###",
            System.Globalization.CultureInfo.GetCultureInfo("tr-TR"));
    }
}
