using Microsoft.Maui.Controls.Shapes;

namespace KuyumHesapFiyatEkran.UI.Services;

public static class ThemePageStyler
{
    public static void Apply(ContentPage page)
    {
        if (!page.IsSet(VisualElement.BackgroundColorProperty))
        {
            page.SetDynamicResource(VisualElement.BackgroundColorProperty, "AppBackground");
        }

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
                if (!scrollView.IsSet(VisualElement.BackgroundColorProperty))
                {
                    scrollView.SetDynamicResource(VisualElement.BackgroundColorProperty, "AppBackground");
                }
                if (scrollView.Content is not null)
                {
                    ApplyTo(scrollView.Content);
                }
                break;

            case Border border:
                if (!border.IsSet(Border.StrokeProperty))
                {
                    border.SetDynamicResource(Border.StrokeProperty, "AppBorder");
                }
                if (!border.IsSet(Border.BackgroundColorProperty))
                {
                    border.SetDynamicResource(Border.BackgroundColorProperty, "AppCardBackground");
                }
                border.StrokeShape ??= new RoundRectangle { CornerRadius = 8 };
                if (border.Content is not null)
                {
                    ApplyTo(border.Content);
                }
                break;

            case Label label:
                if (!label.IsSet(Label.TextColorProperty))
                {
                    if (label.TextColor == Microsoft.Maui.Graphics.Color.FromArgb("#D84555") || label.TextColor == Microsoft.Maui.Graphics.Color.FromArgb("#E34F61"))
                    {
                        label.SetDynamicResource(Label.TextColorProperty, "AppNegative");
                    }
                    else
                    {
                        label.SetDynamicResource(
                            Label.TextColorProperty,
                            label.FontSize <= 13 ? "AppTextMuted" : "AppTextPrimary");
                    }
                }
                break;

            case Button button:
                if (!button.IsSet(VisualElement.BackgroundColorProperty))
                {
                    button.SetDynamicResource(VisualElement.BackgroundColorProperty, "AppAccent");
                }
                if (!button.IsSet(Button.TextColorProperty))
                {
                    button.SetDynamicResource(Button.TextColorProperty, "AppHeaderText");
                }
                if (!button.IsSet(Button.BorderColorProperty))
                {
                    button.SetDynamicResource(Button.BorderColorProperty, "AppBorder");
                }
                break;

            case Entry entry:
                entry.BackgroundColor = Colors.Transparent;
                if (!entry.IsSet(Entry.TextColorProperty))
                {
                    entry.SetDynamicResource(Entry.TextColorProperty, "AppTextPrimary");
                }
                if (!entry.IsSet(Entry.PlaceholderColorProperty))
                {
                    entry.SetDynamicResource(Entry.PlaceholderColorProperty, "AppTextMuted");
                }
                break;

            case Picker picker:
                picker.BackgroundColor = Colors.Transparent;
                if (!picker.IsSet(Picker.TextColorProperty))
                {
                    picker.SetDynamicResource(Picker.TextColorProperty, "AppTextPrimary");
                }
                if (!picker.IsSet(Picker.TitleColorProperty))
                {
                    picker.SetDynamicResource(Picker.TitleColorProperty, "AppTextMuted");
                }
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
}
