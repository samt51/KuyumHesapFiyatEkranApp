using Microsoft.Maui.Controls.Shapes;

namespace KuyumHesapFiyatEkran.UI.Services;

public static class ThemePageStyler
{
    public static void Apply(ContentPage page)
    {
        page.BackgroundColor = ThemeColor("AppBackground", "#050505");

        if (page.Content is not null)
        {
            ApplyTo(page.Content);
        }
    }

    private static void ApplyTo(Element element)
    {
        switch (element)
        {
            case ScrollView scrollView:
                scrollView.BackgroundColor = ThemeColor("AppBackground", "#050505");
                if (scrollView.Content is not null)
                {
                    ApplyTo(scrollView.Content);
                }
                break;

            case Border border:
                border.Stroke = ThemeColor("AppBorder", "#D4AF37");
                border.BackgroundColor = ThemeColor("AppCardBackground", "#101010");
                border.StrokeShape ??= new RoundRectangle { CornerRadius = 8 };
                if (border.Content is not null)
                {
                    ApplyTo(border.Content);
                }
                break;

            case Label label:
                if (label.TextColor == Microsoft.Maui.Graphics.Color.FromArgb("#D84555") || label.TextColor == Microsoft.Maui.Graphics.Color.FromArgb("#E34F61"))
                {
                    label.TextColor = ThemeColor("AppNegative", "#E34F61");
                }
                else
                {
                    label.TextColor = label.FontSize <= 13
                        ? ThemeColor("AppTextMuted", "#A8872B")
                        : ThemeColor("AppTextPrimary", "#F1D16A");
                }
                break;

            case Button button:
                button.BackgroundColor = ThemeColor("AppAccent", "#D4AF37");
                button.TextColor = ThemeColor("AppHeaderText", "#111111");
                button.BorderColor = ThemeColor("AppBorder", "#7E6421");
                break;

            case Entry entry:
                entry.BackgroundColor = Colors.Transparent;
                entry.TextColor = ThemeColor("AppTextPrimary", "#F1D16A");
                entry.PlaceholderColor = ThemeColor("AppTextMuted", "#A8872B");
                break;

            case Picker picker:
                picker.BackgroundColor = Colors.Transparent;
                picker.TextColor = ThemeColor("AppTextPrimary", "#F1D16A");
                picker.TitleColor = ThemeColor("AppTextMuted", "#A8872B");
                break;

            case Layout layout:
                layout.BackgroundColor = Colors.Transparent;
                foreach (var child in layout.Children)
                {
                    if (child is Element childElement)
                    {
                        ApplyTo(childElement);
                    }
                }
                break;

            case ContentView contentView when contentView.Content is not null:
                ApplyTo(contentView.Content);
                break;
        }
    }

    private static Color ThemeColor(string key, string fallback)
    {
        return AppThemeManager.GetColor(key, fallback);
    }
}
