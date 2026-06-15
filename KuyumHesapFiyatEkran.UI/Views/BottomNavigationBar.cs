using Microsoft.Maui.Controls.Shapes;

namespace KuyumHesapFiyatEkran.UI.Views
{
    public sealed class BottomNavigationBar : Border
    {
        private static readonly (string Symbol, string Text, string Route)[] Tabs = new[]
        {
            ("⌂", "Anasayfa", "///MainPage"),
            ("☆", "Favoriler", "///Favorites"),
            ("⌁", "Hesaplama", "///GoldCalculator"),
            ("☎", "İletişim", "///Contact"),
            ("ℹ", "Hakkımızda", "///About")
        };

        public BottomNavigationBar(int activeIndex)
        {
            HeightRequest = 56;
            Margin = new Thickness(12, 0, 12, 8);
            Padding = new Thickness(4, 2);
            StrokeShape = new RoundRectangle { CornerRadius = 14 };

            // Bind border styling to dynamic theme resources
            SetDynamicResource(StrokeProperty, "AppBorder");
            SetDynamicResource(BackgroundColorProperty, "AppCardBackground");

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

                var iconLabel = new Label
                {
                    Text = tab.Symbol,
                    FontSize = i == 0 ? 17 : (i == 4 ? 17 : 18),
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    FontAutoScalingEnabled = false
                };

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
                    iconLabel.SetDynamicResource(Label.TextColorProperty, "AppAccent");
                    textLabel.SetDynamicResource(Label.TextColorProperty, "AppAccent");
                    iconLabel.FontAttributes = FontAttributes.Bold;
                    textLabel.FontAttributes = FontAttributes.Bold;
                }
                else
                {
                    iconLabel.SetDynamicResource(Label.TextColorProperty, "AppTextMuted");
                    textLabel.SetDynamicResource(Label.TextColorProperty, "AppTextMuted");
                }

                var tabStack = new VerticalStackLayout
                {
                    Spacing = 0,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                    Children = { iconLabel, textLabel }
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

                tabStack.GestureRecognizers.Add(tapGesture);
            }

            Content = grid;
        }
    }
}
