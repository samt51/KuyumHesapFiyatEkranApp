using KuyumHesapFiyatEkran.UI.Views;
using Microsoft.Maui.Controls.Shapes;

namespace KuyumHesapFiyatEkran.UI.Services;

public static class SalihUi
{
    public static bool IsDark => AppThemeManager.GetEffectiveTheme() == AslanThemeMode.Dark;
    public static Color Ink => Color.FromArgb(IsDark ? "#F7F7F5" : "#171717");
    public static Color Muted => Color.FromArgb(IsDark ? "#AEB8C2" : "#6B6B6B");
    public static readonly Color Gold = Color.FromArgb("#D5A52B");
    public static readonly Color SecondaryGold = Color.FromArgb("#B88916");
    public static Color Line => Color.FromArgb(IsDark ? "#284050" : "#ECE4D6");
    public static Color Cream => Color.FromArgb(IsDark ? "#071522" : "#FAF7EF");
    public static Color Surface => Color.FromArgb(IsDark ? "#0C1D2B" : "#FFFFFF");
    public static Color SurfaceElevated => Color.FromArgb(IsDark ? "#102536" : "#FFFFFF");
    public static Color SurfaceSoft => Color.FromArgb(IsDark ? "#132A3A" : "#F0EDE6");
    public static Color Divider => Color.FromArgb(IsDark ? "#213746" : "#EEEAE3");
    public static Color Navy => Color.FromArgb("#071522");

    public static Label Text(string text, double size = 13, bool bold = false, Color? color = null) => new()
    {
        Text = text, FontSize = size, FontAttributes = bold ? FontAttributes.Bold : FontAttributes.None,
        TextColor = color ?? Ink, FontAutoScalingEnabled = false, VerticalTextAlignment = TextAlignment.Center
    };

    public static Grid Header(string title, string? right = null)
    {
        var grid = new Grid { HeightRequest = 58, Padding = new Thickness(8, 0), ColumnDefinitions = { new ColumnDefinition(new GridLength(42)), new ColumnDefinition(GridLength.Star), new ColumnDefinition(new GridLength(42)) } };
        var back = new Grid { WidthRequest = 42, HeightRequest = 58, BackgroundColor = Colors.Transparent, Children = { new LucideIcon(LucideIconKind.ChevronLeft, 24, Ink) } };
        var backTap = new TapGestureRecognizer();
        backTap.Tapped += async (_, _) => await GoBackAsync();
        back.GestureRecognizers.Add(backTap);
        grid.Add(back);
        var heading = Text(title, 18, true); heading.HorizontalTextAlignment = TextAlignment.Center; Grid.SetColumn(heading, 1); grid.Add(heading);
        if (!string.IsNullOrWhiteSpace(right)) { var action = Text(right, 11); action.HorizontalTextAlignment = TextAlignment.Center; Grid.SetColumn(action, 2); grid.Add(action); }
        return grid;
    }

    public static async Task GoBackAsync()
    {
        try
        {
            if (Shell.Current?.Navigation.ModalStack.Count > 0)
            {
                await Shell.Current.Navigation.PopModalAsync();
                return;
            }
        }
        catch
        {
        }

        if (Shell.Current is null)
        {
            return;
        }

        try
        {
            await Shell.Current.GoToAsync("///Home");
        }
        catch
        {
            await Shell.Current.GoToAsync("//Home");
        }
    }

    public static Border Card(View content, Color? background = null, float radius = 12, Thickness? padding = null) => new()
    {
        Content = content, Padding = padding ?? new Thickness(12), BackgroundColor = background is null || background == Colors.White ? Surface : background,
        Stroke = Line, StrokeThickness = 1, StrokeShape = new RoundRectangle { CornerRadius = radius }
    };

    public static View Page(View content, int activeTab)
    {
        var grid = new Grid { BackgroundColor = Cream, RowDefinitions = { new RowDefinition(GridLength.Star), new RowDefinition(GridLength.Auto) } };
        grid.Add(content); var nav = new BottomNavigationBar(activeTab); Grid.SetRow(nav, 1); grid.Add(nav); return grid;
    }

    public static Brush GoldGradient() => new LinearGradientBrush
    {
        StartPoint = new Point(0, 0), EndPoint = new Point(1, 0),
        GradientStops = { new GradientStop(Color.FromArgb("#F4CA4C"), 0), new GradientStop(Color.FromArgb("#C79018"), 1) }
    };
}
