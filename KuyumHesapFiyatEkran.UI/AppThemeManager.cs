using Microsoft.Maui.Storage;

namespace KuyumHesapFiyatEkran.UI
{
    public enum AslanThemeMode
    {
        Light,
        Dark
    }

    public static class AppThemeManager
    {
        private const string PreferenceKey = "AslanThemeModeV2";

        public static event EventHandler<AslanThemeMode>? ThemeChanged;

        private static readonly IReadOnlyDictionary<string, Color> LightColors = new Dictionary<string, Color>
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

        private static readonly IReadOnlyDictionary<string, Color> DarkColors = new Dictionary<string, Color>
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

        public static AslanThemeMode GetSavedTheme()
        {
            var value = Preferences.Get(PreferenceKey, AslanThemeMode.Dark.ToString());
            return Enum.TryParse<AslanThemeMode>(value, out var mode) ? mode : AslanThemeMode.Dark;
        }

        public static void Apply(AslanThemeMode mode, ResourceDictionary? pageResources = null, bool save = true)
        {
            if (save)
            {
                Preferences.Set(PreferenceKey, mode.ToString());
            }

            var colors = GetColors(mode);
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
            return mode == AslanThemeMode.Dark ? DarkColors : LightColors;
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
