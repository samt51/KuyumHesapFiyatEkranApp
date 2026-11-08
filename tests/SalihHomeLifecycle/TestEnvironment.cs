global using Microsoft.Maui;
global using Microsoft.Maui.Controls;
global using Microsoft.Maui.Graphics;
global using Microsoft.Maui.ApplicationModel;

// Only platform preferences and network data are replaced. The page and its
// visual tree use the production source and real MAUI controls.
namespace KuyumHesapFiyatEkran.UI
{
    public enum AslanThemeMode { Light, Dark }
    public enum ThemeSelection { Auto, Light, Dark }
    public static class AppThemeManager
    {
        public static event EventHandler<AslanThemeMode>? ThemeChanged;
        public static AslanThemeMode Mode { get; set; }
        public static AslanThemeMode GetEffectiveTheme() => Mode;
        public static ThemeSelection GetThemeSelection() => ThemeSelection.Auto;
        public static Color GetColor(string key, string fallback) => Color.FromArgb(fallback);
        public static void SelectTheme(ThemeSelection selection) => ThemeChanged?.Invoke(null, Mode);
    }
}
namespace KuyumHesapFiyatEkran.UI.Services
{
    public sealed class LiveRatesStore
    {
        public Core.Features.LiveRates.Queries.GetLiveRates.GetLiveRatesQueryResponse? CurrentRates { get; set; }
        public string? LastError => null;
        public Task EnsureLoadedAsync(CancellationToken token) => Task.CompletedTask;
        public Task LoadAsync(CancellationToken token) => Task.CompletedTask;
        public static decimal ParsePrice(string? value) => decimal.TryParse(value, out var price) ? price : 0;
        public static string FormatCurrency(decimal value) => $"{value:N2} TL";
        public static string GetRateKey(Core.Features.LiveRates.Queries.GetLiveRates.LiveRateItem item) => item.Name;
    }
    public sealed class BusinessInfo
    {
        public static BusinessInfo Current { get; } = new();
        public string Key => "SalihCetinkaya";
        public string LogoImage => "logo.png";
        public string PrimaryName => "Salih Cetinkaya";
        public string SecondaryName => "Kuyumculuk";
    }
}
