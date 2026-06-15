namespace KuyumHesapFiyatEkran.UI.Services;

public static class BusinessInfo
{
    private static BrandSettings current = new();

    public static BrandSettings Current => current;

    public static string CompanyName => current.DisplayName;

    public static string Phone => current.Phone;

    public static string WhatsApp => current.WhatsApp;

    public static string Address => current.Address;

    public static string Website => current.Website;

    public static string WorkingHours => current.WorkingHours;

    public static string PrivacyPolicyUrl => current.PrivacyPolicyUrl;

    public static void Configure(BrandSettings settings)
    {
        current = settings;
    }
}
