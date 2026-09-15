using KuyumHesapFiyatEkran.UI.Services;
using Microsoft.Maui.Controls.Shapes;
using KuyumHesapFiyatEkran.UI.Views;

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
        SetDynamicResource(BackgroundColorProperty, "AppBackground");

        callAction = CreateActionTile("Ara", "phone_icon.png", OpenPhoneOrWarn);
        whatsAppAction = CreateActionTile("WhatsApp'tan Yaz", "whatsapp_icon.png", async () => await OpenOrWarn(string.IsNullOrWhiteSpace(BusinessInfo.WhatsApp) ? null : $"https://wa.me/{OnlyDigits(BusinessInfo.WhatsApp)}", "WhatsApp bilgisi henüz tanımlı değil."));
        directionsAction = CreateActionTile("Yol Tarifi Al", "directions_icon.png", OpenDirectionsOrWarn);
        websiteAction = CreateActionTile("Web Sitesini Aç", "web_icon.png", async () => await OpenOrWarn(BusinessInfo.Website, "Web sitesi açılamadı."));

        if (string.Equals(BusinessInfo.Current.Key, "SalihCetinkaya", StringComparison.OrdinalIgnoreCase))
        {
            Shell.SetNavBarIsVisible(this, false);
            Content = SalihUi.Page(CreateSalihContact(), 3);
            ApplyTheme();
            AppThemeManager.ThemeChanged += OnThemeChanged;
            return;
        }

        var contactContent = CreateContactLayout();
        if (string.Equals(BusinessInfo.Current.Key, "SalihCetinkaya", StringComparison.OrdinalIgnoreCase))
        {
            var pageGrid = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition(GridLength.Star),
                    new RowDefinition(GridLength.Auto)
                }
            };
            pageGrid.Add(contactContent);

            var bottomBar = new BottomNavigationBar(3);
            Grid.SetRow(bottomBar, 1);
            pageGrid.Add(bottomBar);

            Content = pageGrid;
        }
        else
        {
            Content = contactContent;
        }

        ApplyTheme();
        AppThemeManager.ThemeChanged += OnThemeChanged;
    }

    private View CreateSalihContact()
    {
        var stack = new VerticalStackLayout { Padding = new Thickness(14, 0, 14, 18), Spacing = 10 };
        stack.Add(SalihUi.Header("İletişim"));
        var storeImage = new Image
        {
            Source = "light_storefront.png",
            Aspect = Aspect.AspectFill,
            HeightRequest = 188
        };
        var storeFrame = new Grid { HeightRequest = 188, IsClippedToBounds = true };
        storeFrame.Add(storeImage);
        stack.Add(SalihUi.Card(storeFrame, Colors.Transparent, 14, new Thickness(0)));
        stack.Add(SalihUi.Text("Bize Ulaşın", 16, true));
        stack.Add(SalihUi.Text("Bizimle iletişime geçmek için aşağıdaki kanallardan bize ulaşabilirsiniz.", 11, false, SalihUi.Muted));
        stack.Add(ContactRow(LucideIconKind.WhatsApp, "WhatsApp", "Hızlı mesaj gönderin", Color.FromArgb("#29BE57"), async () => await OpenOrWarn($"https://wa.me/{OnlyDigits(BusinessInfo.WhatsApp)}", "WhatsApp bilgisi bulunamadı.")));
        stack.Add(ContactRow(LucideIconKind.Phone, "Telefon", BusinessInfo.Phone, Color.FromArgb("#29BE57"), OpenPhoneOrWarn));
        stack.Add(ContactRow(LucideIconKind.MapPin, "Yol Tarifi Al", "Mağazamıza ulaşın", Color.FromArgb("#D5A52B"), OpenDirectionsOrWarn));
        stack.Add(ContactRow(LucideIconKind.Globe, "Web Sitemizi Ziyaret Edin", BusinessInfo.Website, Color.FromArgb("#D5A52B"), async () => await OpenOrWarn(BusinessInfo.Website, "Web sitesi açılamadı.")));
        stack.Add(new BoxView { HeightRequest = 1, Color = SalihUi.Line, Margin = new Thickness(0, 8) });
        stack.Add(SalihUi.Text("Mağaza Bilgileri", 15, true));
        stack.Add(InfoRow(LucideIconKind.MapPin, "Adres", BusinessInfo.Address));
        stack.Add(InfoRow(LucideIconKind.Clock3, "Çalışma Saatleri", "Pazartesi - Cumartesi: 09:00 - 19:30\nPazar: Kapalı"));
        return new ScrollView { Content = stack };
    }

    private static Border ContactRow(LucideIconKind icon, string title, string detail, Color trailing, Func<Task> action)
    {
        var grid = new Grid { ColumnDefinitions = { new ColumnDefinition(new GridLength(34)), new ColumnDefinition(GridLength.Star), new ColumnDefinition(new GridLength(36)) } };
        grid.Add(new LucideIcon(icon, 19, SalihUi.Gold));
        var labels = new VerticalStackLayout { Spacing = 1, Children = { SalihUi.Text(title, 12, true), SalihUi.Text(detail, 9, false, SalihUi.Muted) } }; Grid.SetColumn(labels, 1); grid.Add(labels);
        var end = new LucideIcon(icon == LucideIconKind.Globe ? LucideIconKind.ExternalLink : LucideIconKind.ChevronRight, 18, trailing); Grid.SetColumn(end, 2); grid.Add(end);
        var card = SalihUi.Card(grid, Colors.White, 9, new Thickness(10, 8)); card.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(async () => await action()) }); return card;
    }

    private static View InfoRow(LucideIconKind icon, string title, string detail)
    {
        var grid = new Grid { ColumnDefinitions = { new ColumnDefinition(new GridLength(28)), new ColumnDefinition(GridLength.Star) } };
        grid.Add(new LucideIcon(icon, 17, SalihUi.Gold)); var text = new VerticalStackLayout { Spacing = 3, Children = { SalihUi.Text(title, 11, true), SalihUi.Text(detail, 10, false, SalihUi.Muted) } }; Grid.SetColumn(text, 1); grid.Add(text); return grid;
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
        if (string.Equals(BusinessInfo.Current.Key, "SalihCetinkaya", StringComparison.OrdinalIgnoreCase))
        {
            Content = null; BackgroundColor = SalihUi.Cream; Content = SalihUi.Page(CreateSalihContact(), 3); return;
        }
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

        var brand = BusinessInfo.Current;
        var hasLogo = !string.IsNullOrWhiteSpace(brand.LogoImage);
        
        var monogramLabel = new Label
        {
            Text = brand.LogoMonogram,
            IsVisible = !hasLogo,
            FontSize = 24,
            FontAttributes = FontAttributes.Bold,
            FontAutoScalingEnabled = false,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center
        };
        monogramLabel.SetDynamicResource(Label.TextColorProperty, "AppAccent");

        var logoCard = new Border
        {
            WidthRequest = 66,
            HeightRequest = 66,
            Padding = 6,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            Content = new Grid
            {
                Children =
                {
                    new Image { Source = brand.LogoImage, Aspect = Aspect.AspectFit, IsVisible = hasLogo },
                    monogramLabel
                }
            }
        };
        logoCard.SetDynamicResource(Border.StrokeProperty, "AppBorder");
        logoCard.SetDynamicResource(Border.BackgroundColorProperty, "AppBackground");

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

        var heroCard = new Border
        {
            Padding = 16,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            Content = heroGrid
        };
        heroCard.SetDynamicResource(Border.StrokeProperty, "AppBorder");
        heroCard.SetDynamicResource(Border.BackgroundColorProperty, "AppCardBackground");
        stack.Add(heroCard);

        var quickContactTitle = new Label { Text = "Hızlı iletişim", FontSize = 18, FontAttributes = FontAttributes.Bold };
        quickContactTitle.SetDynamicResource(Label.TextColorProperty, "AppTextPrimary");

        var quickContactDesc = new Label { Text = "Mağazaya ulaşmak, mesaj göndermek veya yol tarifi almak için aşağıdaki seçenekleri kullanabilirsiniz.", FontSize = 13 };
        quickContactDesc.SetDynamicResource(Label.TextColorProperty, "AppTextMuted");

        var quickContactCard = new Border
        {
            Padding = 16,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            Content = new VerticalStackLayout
            {
                Spacing = 12,
                Children =
                {
                    quickContactTitle,
                    quickContactDesc,
                    whatsAppAction.View,
                    CreateTwoColumnActions(callAction.View, directionsAction.View),
                    websiteAction.View
                }
            }
        };
        quickContactCard.SetDynamicResource(Border.StrokeProperty, "AppBorder");
        quickContactCard.SetDynamicResource(Border.BackgroundColorProperty, "AppCardBackground");
        stack.Add(quickContactCard);

        stack.Add(CreateInfo("Adres", BusinessInfo.Address));
        stack.Add(CreateInfo("Çalışma saatleri", BusinessInfo.WorkingHours));
        stack.Add(CreateInfo("Web sitesi", BusinessInfo.Website));

        var scrollView = new ScrollView { Content = stack };
        scrollView.SetDynamicResource(VisualElement.BackgroundColorProperty, "AppBackground");
        return scrollView;
    }

    private static VerticalStackLayout CreateHeroText()
    {
        var companyName = new Label { Text = BusinessInfo.CompanyName, FontSize = 22, FontAttributes = FontAttributes.Bold };
        companyName.SetDynamicResource(Label.TextColorProperty, "AppBrandText");

        var branchName = new Label { Text = BusinessInfo.Current.BranchName, FontSize = 14 };
        branchName.SetDynamicResource(Label.TextColorProperty, "AppAccent");

        var subtitle = new Label { Text = "Altın ve döviz işlemleriniz için hızlı iletişim", FontSize = 12 };
        subtitle.SetDynamicResource(Label.TextColorProperty, "AppTextMuted");

        return new VerticalStackLayout
        {
            Spacing = 5,
            VerticalOptions = LayoutOptions.Center,
            Children = { companyName, branchName, subtitle }
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
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
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

    private async Task OpenPhoneOrWarn()
    {
        if (string.IsNullOrWhiteSpace(BusinessInfo.Phone))
        {
            await DisplayAlert("Bilgi", "Telefon bilgisi henüz tanımlı değil.", "Tamam");
            return;
        }

        var phoneUri = $"tel:{OnlyDigits(BusinessInfo.Phone)}";
        try
        {
            if (!await Launcher.Default.TryOpenAsync(phoneUri))
            {
                await DisplayAlert("Bilgi", $"Arama başlatılamadı. Telefon: {BusinessInfo.Phone}", "Tamam");
            }
        }
        catch
        {
            await DisplayAlert("Bilgi", $"Arama başlatılamadı. Telefon: {BusinessInfo.Phone}", "Tamam");
        }
    }

    private async Task OpenDirectionsOrWarn()
    {
        const string message = "Adres bilgisi henüz tanımlı değil.";

        try
        {
            foreach (var uri in GetDirectionsUris())
            {
                if (await Launcher.Default.TryOpenAsync(uri))
                {
                    return;
                }
            }

            await DisplayAlert("Bilgi", message, "Tamam");
        }
        catch
        {
            await DisplayAlert("Bilgi", message, "Tamam");
        }
    }

    private static Border CreateInfo(string title, string value)
    {
        var titleLabel = new Label { Text = title, FontSize = 12, FontAttributes = FontAttributes.Bold };
        titleLabel.SetDynamicResource(Label.TextColorProperty, "AppTextMuted");

        var valueLabel = new Label { Text = value, FontSize = 15, FontAttributes = FontAttributes.Bold, LineBreakMode = LineBreakMode.WordWrap };
        valueLabel.SetDynamicResource(Label.TextColorProperty, "AppTextPrimary");

        var border = new Border
        {
            Padding = 15,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            Content = new VerticalStackLayout
            {
                Spacing = 5,
                Children = { titleLabel, valueLabel }
            }
        };
        border.SetDynamicResource(Border.StrokeProperty, "AppBorder");
        border.SetDynamicResource(Border.BackgroundColorProperty, "AppCardBackground");
        return border;
    }

    private static string OnlyDigits(string value)
    {
        return new string(value.Where(char.IsDigit).ToArray());
    }

    private static IEnumerable<string> GetDirectionsUris()
    {
        if (string.Equals(BusinessInfo.Current.Key, "SalihCetinkaya", StringComparison.OrdinalIgnoreCase))
        {
            const string salihCetinkayaCoordinates = "36.8243844,34.6478696";

#if IOS
            yield return $"comgooglemaps://?daddr={salihCetinkayaCoordinates}&directionsmode=driving";
            yield return $"https://maps.apple.com/?daddr={salihCetinkayaCoordinates}&dirflg=d";
#elif ANDROID
            yield return $"google.navigation:q={salihCetinkayaCoordinates}";
            yield return $"geo:0,0?q={salihCetinkayaCoordinates}(Salih%20%C3%87etinkaya%20Kuyumculuk)";
#endif
        }

        if (!string.IsNullOrWhiteSpace(BusinessInfo.Address))
        {
            yield return $"https://maps.apple.com/?q={Uri.EscapeDataString(BusinessInfo.Address)}";
        }

        if (!string.IsNullOrWhiteSpace(BusinessInfo.MapUrl))
        {
            yield return BusinessInfo.MapUrl;
        }
    }

    private sealed record ActionTile(Border View, Label Label, Image Icon);
}
