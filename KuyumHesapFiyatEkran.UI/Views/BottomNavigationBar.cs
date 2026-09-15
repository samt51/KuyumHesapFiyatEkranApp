using Microsoft.Maui.Controls.Shapes;

namespace KuyumHesapFiyatEkran.UI.Views
{
    public sealed class BottomNavigationBar : Border
    {
        private static readonly (LucideIconKind Icon, string Text, string Route)[] Tabs = new[]
        {
            (LucideIconKind.House, "Ana Sayfa", "///Home"),
            (LucideIconKind.Star, "Fiyatlar", "///MainPage"),
            (LucideIconKind.Calculator, "Hesaplama", "///GoldCalculator"),
            (LucideIconKind.Phone, "İletişim", "///Contact"),
            (LucideIconKind.Info, "Hakkımızda", "///About")
        };

        public BottomNavigationBar(int activeIndex)
        {
            HeightRequest = 66;
            Margin = new Thickness(0);
            Padding = new Thickness(4, 4, 4, 8);
            StrokeShape = new RoundRectangle { CornerRadius = 0 };
            StrokeThickness = 1;

            var dark = AppThemeManager.GetEffectiveTheme() == AslanThemeMode.Dark;
            Stroke = dark ? Color.FromArgb("#213746") : Color.FromArgb("#ECE4D6");
            BackgroundColor = dark ? Color.FromArgb("#0C1D2B") : Colors.White;

            var grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Star)
                }
            };

            for (int i = 0; i < Tabs.Length; i++)
            {
                var tab = Tabs[i];
                var route = tab.Route;
                var isSelected = (i == activeIndex);

                var iconColor = isSelected ? Color.FromArgb("#D4A72C") : dark ? Color.FromArgb("#B9C1CA") : Color.FromArgb("#535353");
                var icon = new LucideIcon(tab.Icon, 22, iconColor);

                var textLabel = new Label
                {
                    Text = tab.Text,
                    FontSize = 9,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    FontAutoScalingEnabled = false
                };

                if (isSelected)
                {
                    textLabel.SetDynamicResource(Label.TextColorProperty, "AppAccent");
                    textLabel.FontAttributes = FontAttributes.Bold;
                }
                else
                {
                    textLabel.TextColor = dark ? Color.FromArgb("#B9C1CA") : Color.FromArgb("#535353");
                }

                var tabStack = new VerticalStackLayout
                {
                    Spacing = 0,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                    Children = { icon, textLabel }
                };

                Grid.SetColumn(tabStack, i);
                grid.Add(tabStack);

                // Add tap navigation
                var tapGesture = new TapGestureRecognizer();
                tapGesture.Tapped += async (s, e) =>
                {
                    try
                    {
                        if (Shell.Current is not null)
                        {
                            await Shell.Current.GoToAsync(route);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Navigation error: {ex}");
                    }
                };

                tapGesture.Tapped += async (_, _) =>
                {
                    await tabStack.ScaleTo(0.92, 60, Easing.CubicOut);
                    await tabStack.ScaleTo(1, 90, Easing.CubicIn);
                };
                tabStack.GestureRecognizers.Add(tapGesture);
            }

            Content = grid;
        }
    }
}
