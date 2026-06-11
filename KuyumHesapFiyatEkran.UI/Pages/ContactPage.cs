using KuyumHesapFiyatEkran.UI.Services;
using Microsoft.Maui.Controls.Shapes;

namespace KuyumHesapFiyatEkran.UI.Pages;

public sealed class ContactPage : ContentPage
{
    private readonly ActionTile callAction;
    private readonly ActionTile whatsAppAction;
    private readonly ActionTile directionsAction;
    private readonly ActionTile websiteAction;

    public ContactPage()
    {
        Title = "İletişim";
        BackgroundColor = Color.FromArgb("#FFFFFF");

        callAction = CreateActionTile("Ara", "phone_icon.svg", async () => await OpenOrWarn(string.IsNullOrWhiteSpace(BusinessInfo.Phone) ? null : $"tel:{OnlyDigits(BusinessInfo.Phone)}", "Telefon bilgisi henuz tanimli degil."));
        whatsAppAction = CreateActionTile("WhatsApp'tan Yaz", "whatsapp_icon.svg", async () => await OpenOrWarn(string.IsNullOrWhiteSpace(BusinessInfo.WhatsApp) ? null : $"https://wa.me/{OnlyDigits(BusinessInfo.WhatsApp)}", "WhatsApp bilgisi henuz tanimli degil."));
        directionsAction = CreateActionTile("Yol Tarifi Al", "directions_icon.svg", async () => await OpenOrWarn(string.IsNullOrWhiteSpace(BusinessInfo.Address) ? null : $"https://maps.apple.com/?q={Uri.EscapeDataString(BusinessInfo.Address)}", "Adres bilgisi henuz tanimli degil."));
        websiteAction = CreateActionTile("Web Sitesini Aç", "web_icon.svg", async () => await OpenOrWarn(BusinessInfo.Website, "Web sitesi acilamadi."));

        Content = CreateContactLayout();
        ApplyTheme();
        AppThemeManager.ThemeChanged += OnThemeChanged;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ApplyTheme();
    }

    private void OnThemeChanged(object? sender, AslanThemeMode mode)
    {
        MainThread.BeginInvokeOnMainThread(ApplyTheme);
    }

    private void ApplyTheme()
    {
        ThemePageStyler.Apply(this);
        StyleActionTiles();
    }

    private ScrollView CreateContactLayout()
    {
        var stack = new VerticalStackLayout
        {
            Padding = new Thickness(18, 18, 18, 26),
            Spacing = 16
        };

        var logoCard = new Border
        {
            WidthRequest = 66,
            HeightRequest = 66,
            Padding = 6,
            Stroke = Color.FromArgb("#D4AF37"),
            BackgroundColor = Color.FromArgb("#050505"),
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Content = new Image { Source = "aslan_logo.png", Aspect = Aspect.AspectFit }
        };
        var heroText = CreateHeroText();
        var heroGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star)
            },
            ColumnSpacing = 14
        };
        heroGrid.Add(logoCard);
        heroGrid.SetColumn(heroText, 1);
        heroGrid.Add(heroText);

        stack.Add(new Border
        {
            Padding = 16,
            Stroke = Color.FromArgb("#D4AF37"),
            BackgroundColor = Color.FromArgb("#101010"),
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Content = heroGrid
        });

        stack.Add(new Border
        {
            Padding = 16,
            Stroke = Color.FromArgb("#D4AF37"),
            BackgroundColor = Color.FromArgb("#FFFFFF"),
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Content = new VerticalStackLayout
            {
                Spacing = 12,
                Children =
                {
                    new Label { Text = "Hızlı iletişim", FontSize = 18, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#2F2A1D") },
                    new Label { Text = "Mağazaya ulaşmak, mesaj göndermek veya yol tarifi almak için aşağıdaki seçenekleri kullanabilirsiniz.", FontSize = 13, TextColor = Color.FromArgb("#9A7417") },
                    whatsAppAction.View,
                    CreateTwoColumnActions(callAction.View, directionsAction.View),
                    websiteAction.View
                }
            }
        });

        stack.Add(CreateInfo("Adres", BusinessInfo.Address));
        stack.Add(CreateInfo("Çalışma saatleri", BusinessInfo.WorkingHours));
        stack.Add(CreateInfo("Web sitesi", BusinessInfo.Website));

        return new ScrollView { Content = stack };
    }

    private static VerticalStackLayout CreateHeroText()
    {
        return new VerticalStackLayout
        {
            Spacing = 5,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                new Label { Text = BusinessInfo.CompanyName, FontSize = 22, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#FFFFFF") },
                new Label { Text = "İçerenköy Şubesi", FontSize = 14, TextColor = Color.FromArgb("#D4AF37") },
                new Label { Text = "Altın ve döviz işlemleriniz için hızlı iletişim", FontSize = 12, TextColor = Color.FromArgb("#A8872B") }
            }
        };
    }

    private static Grid CreateTwoColumnActions(View left, View right)
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star)
            },
            ColumnSpacing = 10
        };

        grid.Add(left);
        grid.SetColumn(right, 1);
        grid.Add(right);
        return grid;
    }

    private ActionTile CreateActionTile(string text, string icon, Func<Task> action)
    {
        var image = new Image
        {
            Source = icon,
            WidthRequest = 22,
            HeightRequest = 22,
            Aspect = Aspect.AspectFit,
            VerticalOptions = LayoutOptions.Center
        };
        var label = new Label
        {
            Text = text,
            FontSize = 15,
            FontAttributes = FontAttributes.Bold,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center,
            VerticalOptions = LayoutOptions.Center
        };
        var content = new HorizontalStackLayout
        {
            Spacing = 10,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Children = { image, label }
        };
        var tile = new Border
        {
            Padding = new Thickness(12, 0),
            HeightRequest = 56,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Content = content
        };

        tile.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(async () => await action())
        });

        return new ActionTile(tile, label, image);
    }

    private void StyleActionTiles()
    {
        StyleTile(whatsAppAction, Color.FromArgb("#25D366"), Colors.White, Color.FromArgb("#1DA851"));
        StyleTile(callAction, AppThemeManager.GetColor("AppAccent", "#D4AF37"), AppThemeManager.GetColor("AppHeaderText", "#111111"), AppThemeManager.GetColor("AppBorder", "#7E6421"));
        StyleTile(directionsAction, AppThemeManager.GetColor("AppCardBackground", "#101010"), AppThemeManager.GetColor("AppAccent", "#D4AF37"), AppThemeManager.GetColor("AppBorder", "#7E6421"));
        StyleTile(websiteAction, AppThemeManager.GetColor("AppCardBackground", "#101010"), AppThemeManager.GetColor("AppAccent", "#D4AF37"), AppThemeManager.GetColor("AppBorder", "#7E6421"));
    }

    private static void StyleTile(ActionTile tile, Color background, Color text, Color border)
    {
        tile.View.BackgroundColor = background;
        tile.View.Stroke = border;
        tile.Label.TextColor = text;
    }

    private async Task OpenOrWarn(string? uri, string message)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(uri) || !await Launcher.Default.TryOpenAsync(uri))
            {
                await DisplayAlert("Bilgi", message, "Tamam");
            }
        }
        catch
        {
            await DisplayAlert("Bilgi", message, "Tamam");
        }
    }

    private static Border CreateInfo(string title, string value)
    {
        return new Border
        {
            Padding = 15,
            Stroke = Color.FromArgb("#E8D596"),
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Content = new VerticalStackLayout
            {
                Spacing = 5,
                Children =
                {
                    new Label { Text = title, FontSize = 12, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#9A7417") },
                    new Label { Text = value, FontSize = 16, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#2F2A1D"), LineBreakMode = LineBreakMode.WordWrap }
                }
            }
        };
    }

    private static string OnlyDigits(string value)
    {
        return new string(value.Where(char.IsDigit).ToArray());
    }

    private sealed record ActionTile(Border View, Label Label, Image Icon);
}
