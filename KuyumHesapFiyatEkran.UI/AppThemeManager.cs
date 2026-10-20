using Microsoft.Maui.Storage;
using KuyumHesapFiyatEkran.UI.Services;

namespace KuyumHesapFiyatEkran.UI
{
    public enum AslanThemeMode
    {
        Light,
        Dark
    }

    public enum ThemeSelection
    {
        Auto,
        Light,
        Dark
    }

    public static class AppThemeManager
    {
        private const string PreferenceKey = "AslanThemeModeV2";
        private const string SelectionPreferenceKey = "AppThemeSelectionV2";

        public static event EventHandler<AslanThemeMode>? ThemeChanged;

        private static readonly IReadOnlyDictionary<string, Color> AslanLightColors = new Dictionary<string, Color>
        {
            ["AppBackground"] = Color.FromArgb("#FFFFFF"),
            ["AppCardBackground"] = Color.FromArgb("#FFFFFF"),
            ["AppPanelBackground"] = Color.FromArgb("#FFFFFF"),
            ["AppAlternateRowBackground"] = Color.FromArgb("#FAF6EA"),
            ["AppBorder"] = Color.FromArgb("#D4AF37"),
            ["AppAccent"] = Color.FromArgb("#D4AF37"),
            ["AppTextPrimary"] = Color.FromArgb("#2F2A1D"),
            ["AppTextMuted"] = Color.FromArgb("#9A7417"),
            ["AppTextInactive"] = Color.FromArgb("#A8872B"),
            ["AppBrandText"] = Color.FromArgb("#231F20"),
            ["AppHeaderText"] = Color.FromArgb("#111111"),
            ["AppPositive"] = Color.FromArgb("#1FA463"),
            ["AppNegative"] = Color.FromArgb("#D84555"),
            ["AppTickerBackground"] = Color.FromArgb("#FFFFFF"),
            ["AppTickerCardBackground"] = Color.FromArgb("#FFF9E8"),
            ["AppTickerText"] = Color.FromArgb("#2F2A1D"),
            ["AppLoadingBackground"] = Color.FromArgb("#FFFFFF")
        };

        private static readonly IReadOnlyDictionary<string, Color> AslanDarkColors = new Dictionary<string, Color>
        {
            ["AppBackground"] = Color.FromArgb("#050505"),
            ["AppCardBackground"] = Color.FromArgb("#101010"),
            ["AppPanelBackground"] = Color.FromArgb("#050505"),
            ["AppAlternateRowBackground"] = Color.FromArgb("#130E08"),
            ["AppBorder"] = Color.FromArgb("#7E6421"),
            ["AppAccent"] = Color.FromArgb("#D4AF37"),
            ["AppTextPrimary"] = Color.FromArgb("#F1D16A"),
            ["AppTextMuted"] = Color.FromArgb("#A8872B"),
            ["AppTextInactive"] = Color.FromArgb("#8B6D20"),
            ["AppBrandText"] = Color.FromArgb("#FFFFFF"),
            ["AppHeaderText"] = Color.FromArgb("#111111"),
            ["AppPositive"] = Color.FromArgb("#26BF72"),
            ["AppNegative"] = Color.FromArgb("#E34F61"),
            ["AppTickerBackground"] = Color.FromArgb("#050505"),
            ["AppTickerCardBackground"] = Color.FromArgb("#101010"),
            ["AppTickerText"] = Color.FromArgb("#F1D16A"),
            ["AppLoadingBackground"] = Color.FromArgb("#000000")
        };

        private static readonly IReadOnlyDictionary<string, Color> SalihLightColors = new Dictionary<string, Color>
        {
            ["AppBackground"] = Color.FromArgb("#FAF6EA"),
            ["AppCardBackground"] = Color.FromArgb("#FFFFFF"),
            ["AppPanelBackground"] = Color.FromArgb("#F6EEDC"),
            ["AppAlternateRowBackground"] = Color.FromArgb("#F9F4E6"),
            ["AppBorder"] = Color.FromArgb("#C5A028"),
            ["AppAccent"] = Color.FromArgb("#9A7417"),
            ["AppTextPrimary"] = Color.FromArgb("#2F2A1D"),
            ["AppTextMuted"] = Color.FromArgb("#A8872B"),
            ["AppTextInactive"] = Color.FromArgb("#8B6D20"),
            ["AppBrandText"] = Color.FromArgb("#231F20"),
            ["AppHeaderText"] = Color.FromArgb("#FAF6EA"),
            ["AppPositive"] = Color.FromArgb("#1FA463"),
            ["AppNegative"] = Color.FromArgb("#D84555"),
            ["AppTickerBackground"] = Color.FromArgb("#FAF6EA"),
            ["AppTickerCardBackground"] = Color.FromArgb("#FFFFFF"),
            ["AppTickerText"] = Color.FromArgb("#2F2A1D"),
            ["AppLoadingBackground"] = Color.FromArgb("#FAF6EA")
        };

        private static readonly IReadOnlyDictionary<string, Color> SalihDarkColors = new Dictionary<string, Color>
        {
            ["AppBackground"] = Color.FromArgb("#070707"),
            ["AppCardBackground"] = Color.FromArgb("#121212"),
            ["AppPanelBackground"] = Color.FromArgb("#0D0D0D"),
            ["AppAlternateRowBackground"] = Color.FromArgb("#141414"),
            ["AppBorder"] = Color.FromArgb("#3A2C0E"),
            ["AppAccent"] = Color.FromArgb("#D4AF37"),
            ["AppTextPrimary"] = Color.FromArgb("#FFFFFF"),
            ["AppTextMuted"] = Color.FromArgb("#8A8A8A"),
            ["AppTextInactive"] = Color.FromArgb("#FFFFFF"),
            ["AppBrandText"] = Color.FromArgb("#D4AF37"),
            ["AppHeaderText"] = Color.FromArgb("#FFFFFF"),
            ["AppPositive"] = Color.FromArgb("#31C278"),
            ["AppNegative"] = Color.FromArgb("#FF4F5E"),
            ["AppTickerBackground"] = Color.FromArgb("#070707"),
            ["AppTickerCardBackground"] = Color.FromArgb("#121212"),
            ["AppTickerText"] = Color.FromArgb("#D4AF37"),
            ["AppLoadingBackground"] = Color.FromArgb("#070707")
        };
        
private static readonly IReadOnlyDictionary<string, Color> MazlumLightColors =
    new Dictionary<string, Color>
    {
        ["AppBackground"] = Color.FromArgb("#FAF9F5"),
        ["AppCardBackground"] = Color.FromArgb("#F7F4E9"),
        ["AppPanelBackground"] = Color.FromArgb("#FFFFFF"),
        ["AppAlternateRowBackground"] = Color.FromArgb("#F4F1E6"),
        ["AppBorder"] = Color.FromArgb("#DDD8C8"),

        ["AppAccent"] = Color.FromArgb("#B99A20"),

        ["AppTextPrimary"] = Color.FromArgb("#171717"),
        ["AppTextMuted"] = Color.FromArgb("#686868"),
        ["AppTextInactive"] = Color.FromArgb("#8A8A8A"),

        ["AppBrandText"] = Color.FromArgb("#A88916"),
        ["AppHeaderText"] = Color.FromArgb("#171717"),

        ["AppPositive"] = Color.FromArgb("#20A66A"),
        ["AppNegative"] = Color.FromArgb("#D64A4A"),

        ["AppTickerBackground"] = Color.FromArgb("#FAF9F5"),
        ["AppTickerCardBackground"] = Color.FromArgb("#F7F4E9"),
        ["AppTickerText"] = Color.FromArgb("#171717"),

        ["AppLoadingBackground"] = Color.FromArgb("#FAF9F5")
    };

private static readonly IReadOnlyDictionary<string, Color> MazlumDarkColors =
    new Dictionary<string, Color>
    {
        ["AppBackground"] = Color.FromArgb("#101113"),
        ["AppCardBackground"] = Color.FromArgb("#191A16"),
        ["AppPanelBackground"] = Color.FromArgb("#121417"),
        ["AppAlternateRowBackground"] = Color.FromArgb("#171819"),
        ["AppBorder"] = Color.FromArgb("#343426"),

        ["AppAccent"] = Color.FromArgb("#D4B56A"),

        ["AppTextPrimary"] = Color.FromArgb("#F0F0F0"),
        ["AppTextMuted"] = Color.FromArgb("#92979F"),
        ["AppTextInactive"] = Color.FromArgb("#777C84"),

        ["AppBrandText"] = Color.FromArgb("#D4B56A"),
        ["AppHeaderText"] = Color.FromArgb("#F0F0F0"),

        ["AppPositive"] = Color.FromArgb("#3DCF8E"),
        ["AppNegative"] = Color.FromArgb("#E65C61"),

        ["AppTickerBackground"] = Color.FromArgb("#101113"),
        ["AppTickerCardBackground"] = Color.FromArgb("#191A16"),
        ["AppTickerText"] = Color.FromArgb("#D4B56A"),

        ["AppLoadingBackground"] = Color.FromArgb("#101113")
    };

    public static AslanThemeMode GetSavedTheme()
        {
            var value = Preferences.Get(PreferenceKey, AslanThemeMode.Light.ToString());
            return Enum.TryParse<AslanThemeMode>(value, out var mode) ? mode : AslanThemeMode.Light;
        }

        public static ThemeSelection GetThemeSelection()
        {
            var value = Preferences.Get(SelectionPreferenceKey, ThemeSelection.Auto.ToString());
            return Enum.TryParse<ThemeSelection>(value, out var selection) ? selection : ThemeSelection.Auto;
        }

        public static AslanThemeMode GetEffectiveTheme(ThemeSelection? selection = null, DateTime? localTime = null)
        {
            return (selection ?? GetThemeSelection()) switch
            {
                ThemeSelection.Light => AslanThemeMode.Light,
                ThemeSelection.Dark => AslanThemeMode.Dark,
                _ => (localTime ?? DateTime.Now).Hour is >= 6 and < 18 ? AslanThemeMode.Light : AslanThemeMode.Dark
            };
        }

        public static void SelectTheme(ThemeSelection selection, ResourceDictionary? pageResources = null)
        {
            Preferences.Set(SelectionPreferenceKey, selection.ToString());
            Apply(GetEffectiveTheme(selection), pageResources, false);
        }

        public static void ApplySelectedTheme(ResourceDictionary? pageResources = null)
        {
            Apply(GetEffectiveTheme(), pageResources, false);
        }

        public static void Apply(AslanThemeMode mode, ResourceDictionary? pageResources = null, bool save = true)
        {
            if (save)
            {
                Preferences.Set(PreferenceKey, mode.ToString());
                Preferences.Set(SelectionPreferenceKey, mode == AslanThemeMode.Light ? ThemeSelection.Light.ToString() : ThemeSelection.Dark.ToString());
            }

            var colors = GetColors(mode);
            if (Application.Current is not null)
            {
                Application.Current.UserAppTheme = mode == AslanThemeMode.Dark ? AppTheme.Dark : AppTheme.Light;
            }
            ApplyTo(Application.Current?.Resources, colors);
            ApplyTo(pageResources, colors);
            ThemeChanged?.Invoke(null, mode);
        }

        public static Color GetColor(string key, string fallback)
        {
            if (Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Color color)
            {
                return color;
            }

            var colors = GetColors(GetSavedTheme());
            return colors.TryGetValue(key, out var themeColor) ? themeColor : Color.FromArgb(fallback);
        }

        private static IReadOnlyDictionary<string, Color> GetColors(AslanThemeMode mode)
        {
            var brandKey = BusinessInfo.Current.Key;

            bool isSalih = string.Equals(
                brandKey,
                "SalihCetinkaya",
                StringComparison.OrdinalIgnoreCase);

            bool isMazlum = string.Equals(
                brandKey,
                "Mazlum",
                StringComparison.OrdinalIgnoreCase);

            if (isMazlum)
            {
                return mode == AslanThemeMode.Dark
                    ? MazlumDarkColors
                    : MazlumLightColors;
            }

            if (isSalih)
            {
                return mode == AslanThemeMode.Dark
                    ? SalihDarkColors
                    : SalihLightColors;
            }

            return mode == AslanThemeMode.Dark
                ? AslanDarkColors
                : AslanLightColors;
        }

        private static void ApplyTo(ResourceDictionary? resources, IReadOnlyDictionary<string, Color> colors)
        {
            if (resources is null)
            {
                return;
            }

            foreach (var color in colors)
            {
                if (resources.ContainsKey(color.Key))
                {
                    resources[color.Key] = color.Value;
                }
                else
                {
                    resources.Add(color.Key, color.Value);
                }
            }
        }
    }
}
