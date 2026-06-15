using KuyumHesapFiyatEkran.Core.Features.LiveRates.Queries.GetLiveRates;
using KuyumHesapFiyatEkran.UI.Services;
using Microsoft.Maui.Controls.Shapes;
using KuyumHesapFiyatEkran.UI.Views;

namespace KuyumHesapFiyatEkran.UI.Pages;

public sealed class GoldCalculatorPage : ContentPage
{
    private readonly LiveRatesStore liveRatesStore;
    private readonly Picker goldPicker = new() { Title = "Altin turu secin" };
    private readonly Entry amountEntry = new() { Placeholder = "Miktar / gram", Keyboard = Keyboard.Numeric, ReturnType = ReturnType.Done };
    private readonly Button sellModeButton = new() { Text = "Satış", Padding = new Thickness(10, 8), CornerRadius = 8 };
    private readonly Button buyModeButton = new() { Text = "Alış", Padding = new Thickness(10, 8), CornerRadius = 8 };
    private readonly Label buyPriceLabel = CreateValueLabel("Alış: -");
    private readonly Label sellPriceLabel = CreateValueLabel("Satış: -");
    private readonly Label unitPriceLabel = CreateValueLabel("Birim fiyat: -");
    private readonly Label resultLabel = CreateResultLabel();
    private readonly Label messageLabel = new() { TextColor = Color.FromArgb("#D84555"), IsVisible = false };
    private readonly List<LiveRateItem> goldRates = new();
    private int selectedPriceType;

    public GoldCalculatorPage(LiveRatesStore liveRatesStore)
    {
        this.liveRatesStore = liveRatesStore;
        Title = "Altın Hesaplama";
        SetDynamicResource(BackgroundColorProperty, "AppBackground");
        goldPicker.ItemDisplayBinding = new Binding(nameof(LiveRateItem.Name));
        StyleInput(goldPicker);
        StyleInput(amountEntry);

        goldPicker.SelectedIndexChanged += (_, _) => Calculate();
        amountEntry.TextChanged += (_, _) => Calculate();
        amountEntry.Completed += (_, _) => amountEntry.Unfocus();
        sellModeButton.Clicked += (_, _) => SelectPriceType(0);
        buyModeButton.Clicked += (_, _) => SelectPriceType(1);

        SelectPriceType(0);
        
        var goldContent = CreateGoldLayout();
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
            pageGrid.Add(goldContent);

            var bottomBar = new BottomNavigationBar(2);
            Grid.SetRow(bottomBar, 1);
            pageGrid.Add(bottomBar);

            Content = pageGrid;
        }
        else
        {
            Content = goldContent;
        }

        ApplyTheme();
        AppThemeManager.ThemeChanged += OnThemeChanged;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await liveRatesStore.EnsureLoadedAsync(CancellationToken.None);
        LoadGoldRates();
        Calculate();
        ApplyTheme();
    }

    private void Calculate()
    {
        messageLabel.IsVisible = false;
        var rate = goldPicker.SelectedItem as LiveRateItem;
        var priceText = selectedPriceType == 1 ? rate?.Buy : rate?.Sell;
        var unitPrice = LiveRatesStore.ParsePrice(priceText);
        buyPriceLabel.Text = string.IsNullOrWhiteSpace(rate?.Buy) ? "Alış: -" : $"Alış: {rate.Buy}";
        sellPriceLabel.Text = string.IsNullOrWhiteSpace(rate?.Sell) ? "Satış: -" : $"Satış: {rate.Sell}";
        unitPriceLabel.Text = unitPrice > 0 ? $"Birim fiyat: {LiveRatesStore.FormatCurrency(unitPrice)}" : "Birim fiyat: -";

        if (goldRates.Count == 0)
        {
            resultLabel.Text = "Sonuç: -";
            messageLabel.Text = "Altın fiyatları henüz alınamadı. Canlı Fiyatlar ekranından fiyatlar yüklendikten sonra tekrar deneyin.";
            messageLabel.IsVisible = true;
            return;
        }

        if (!decimal.TryParse(amountEntry.Text?.Replace(',', '.'), System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var amount) || amount <= 0)
        {
            resultLabel.Text = "Sonuç: -";
            return;
        }

        if (unitPrice <= 0)
        {
            resultLabel.Text = "Sonuç: -";
            messageLabel.Text = "Bu kalem için fiyat bilgisi henüz alınamadı.";
            messageLabel.IsVisible = true;
            return;
        }

        resultLabel.Text = $"Sonuç: {LiveRatesStore.FormatCurrency(amount * unitPrice)}";
    }

    private void LoadGoldRates()
    {
        var currentRates = liveRatesStore.CurrentRates;
        var rates = currentRates is null
            ? Enumerable.Empty<LiveRateItem>()
            : currentRates.Sarrafiye.Concat(currentRates.Maden);

        goldRates.Clear();
        goldRates.AddRange(rates.Where(item => HasUsablePrice(item)));
        goldPicker.ItemsSource = null;
        goldPicker.ItemsSource = goldRates;

        if (goldRates.Count > 0 && goldPicker.SelectedIndex < 0)
        {
            goldPicker.SelectedIndex = 0;
        }
    }

    private static bool HasUsablePrice(LiveRateItem item)
    {
        return LiveRatesStore.ParsePrice(item.Buy) > 0 || LiveRatesStore.ParsePrice(item.Sell) > 0;
    }

    private void SelectPriceType(int priceType)
    {
        selectedPriceType = priceType;
        UpdatePriceModeButtons();
        Calculate();
    }

    private void UpdatePriceModeButtons()
    {
        StyleModeButton(sellModeButton, selectedPriceType == 0);
        StyleModeButton(buyModeButton, selectedPriceType == 1);
    }

    private void OnThemeChanged(object? sender, AslanThemeMode mode)
    {
        MainThread.BeginInvokeOnMainThread(ApplyTheme);
    }

    private void ApplyTheme()
    {
        ThemePageStyler.Apply(this);
        StyleInput(goldPicker);
        StyleInput(amountEntry);
        UpdatePriceModeButtons();
        resultLabel.TextColor = AppThemeManager.GetColor("AppBrandText", "#FFFFFF");
        unitPriceLabel.TextColor = AppThemeManager.GetColor("AppTextPrimary", "#F1D16A");
        buyPriceLabel.TextColor = AppThemeManager.GetColor("AppTextPrimary", "#F1D16A");
        sellPriceLabel.TextColor = AppThemeManager.GetColor("AppTextPrimary", "#F1D16A");
        messageLabel.TextColor = AppThemeManager.GetColor("AppNegative", "#E34F61");
    }

    private ScrollView CreateGoldLayout()
    {
        var root = new VerticalStackLayout
        {
            Padding = new Thickness(18, 18, 18, 24),
            Spacing = 16
        };

        var titleLabel = new Label { Text = "Altın Hesaplama", FontSize = 22, FontAttributes = FontAttributes.Bold };
        titleLabel.SetDynamicResource(Label.TextColorProperty, "AppBrandText");
        var subtitleLabel = new Label { Text = "Canlı fiyatla hızlı TL hesaplama", FontSize = 13 };
        subtitleLabel.SetDynamicResource(Label.TextColorProperty, "AppTextMuted");

        var currencySymbol = new Label { Text = "₺", FontSize = 24, FontAttributes = FontAttributes.Bold };
        currencySymbol.SetDynamicResource(Label.TextColorProperty, "AppAccent");

        var currencyBorder = new Border
        {
            Padding = 10,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Content = currencySymbol
        };
        currencyBorder.SetDynamicResource(Border.StrokeProperty, "AppBorder");
        currencyBorder.SetDynamicResource(Border.BackgroundColorProperty, "AppBackground");

        var headerGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            Children =
            {
                new VerticalStackLayout
                {
                    Spacing = 5,
                    Children = { titleLabel, subtitleLabel }
                },
                currencyBorder
            }
        };
        Grid.SetColumn(currencyBorder, 1);

        var headerCard = new Border
        {
            Padding = 16,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            Content = headerGrid
        };
        headerCard.SetDynamicResource(Border.StrokeProperty, "AppBorder");
        headerCard.SetDynamicResource(Border.BackgroundColorProperty, "AppCardBackground");

        root.Add(headerCard);

        var formStack = new VerticalStackLayout { Spacing = 12 };
        formStack.Add(CreateField("Altın türü", goldPicker));
        formStack.Add(CreateField("Miktar", amountEntry));
        
        var accountPriceLabel = new Label { Text = "Hesap fiyatı", FontSize = 12 };
        accountPriceLabel.SetDynamicResource(Label.TextColorProperty, "AppTextMuted");
        formStack.Add(accountPriceLabel);
        
        formStack.Add(new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star)
            },
            ColumnSpacing = 8,
            Children = { sellModeButton, buyModeButton }
        });
        Grid.SetColumn(buyModeButton, 1);

        var priceGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star)
            },
            ColumnSpacing = 10
        };
        priceGrid.Add(CreatePriceTile("Alış", buyPriceLabel));
        var sellTile = CreatePriceTile("Satış", sellPriceLabel);
        Grid.SetColumn(sellTile, 1);
        priceGrid.Add(sellTile);
        formStack.Add(priceGrid);

        var calculateButton = new Button
        {
            Text = "Hesapla",
            FontAttributes = FontAttributes.Bold,
            FontSize = 16,
            CornerRadius = 8,
            Padding = new Thickness(14, 12),
            Command = new Command(() =>
            {
                amountEntry.Unfocus();
                Calculate();
            })
        };
        calculateButton.SetDynamicResource(Button.BackgroundColorProperty, "AppAccent");
        calculateButton.SetDynamicResource(Button.TextColorProperty, "AppBackground");
        calculateButton.SetDynamicResource(Button.BorderColorProperty, "AppBorder");
        formStack.Add(calculateButton);

        var formCard = new Border
        {
            Padding = 16,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            Content = formStack
        };
        formCard.SetDynamicResource(Border.StrokeProperty, "AppBorder");
        formCard.SetDynamicResource(Border.BackgroundColorProperty, "AppCardBackground");
        root.Add(formCard);

        var resultStack = new VerticalStackLayout
        {
            Spacing = 8,
            Children =
            {
                unitPriceLabel,
                resultLabel,
                messageLabel
            }
        };

        var resultCard = new Border
        {
            Padding = 16,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            Content = resultStack
        };
        resultCard.SetDynamicResource(Border.StrokeProperty, "AppAccent");
        resultCard.SetDynamicResource(Border.BackgroundColorProperty, "AppAlternateRowBackground");
        root.Add(resultCard);

        var scrollView = new ScrollView { Content = root };
        scrollView.SetDynamicResource(VisualElement.BackgroundColorProperty, "AppBackground");
        return scrollView;
    }

    private static Border CreateField(string title, View input)
    {
        var label = new Label { Text = title, FontSize = 12 };
        label.SetDynamicResource(Label.TextColorProperty, "AppTextMuted");

        var border = new Border
        {
            Padding = new Thickness(12, 8),
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Content = new VerticalStackLayout
            {
                Spacing = 4,
                Children = { label, input }
            }
        };
        border.SetDynamicResource(Border.StrokeProperty, "AppBorder");
        border.SetDynamicResource(Border.BackgroundColorProperty, "AppBackground");
        return border;
    }

    private static Border CreatePriceTile(string title, Label valueLabel)
    {
        valueLabel.FontSize = 14;
        valueLabel.FontAttributes = FontAttributes.Bold;
        valueLabel.SetDynamicResource(Label.TextColorProperty, "AppTextPrimary");

        var label = new Label { Text = title, FontSize = 12 };
        label.SetDynamicResource(Label.TextColorProperty, "AppTextMuted");

        var border = new Border
        {
            Padding = 12,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Content = new VerticalStackLayout
            {
                Spacing = 5,
                Children = { label, valueLabel }
            }
        };
        border.SetDynamicResource(Border.StrokeProperty, "AppBorder");
        border.SetDynamicResource(Border.BackgroundColorProperty, "AppBackground");
        return border;
    }

    private static void StyleInput(InputView input)
    {
        input.BackgroundColor = Colors.Transparent;
        input.SetDynamicResource(InputView.TextColorProperty, "AppTextPrimary");
        input.SetDynamicResource(InputView.PlaceholderColorProperty, "AppTextMuted");
        input.FontSize = 16;
    }

    private static void StyleInput(Picker input)
    {
        input.BackgroundColor = Colors.Transparent;
        input.SetDynamicResource(Picker.TextColorProperty, "AppTextPrimary");
        input.SetDynamicResource(Picker.TitleColorProperty, "AppTextMuted");
        input.FontSize = 16;
    }

    private static void StyleModeButton(Button button, bool selected)
    {
        button.BackgroundColor = selected
            ? AppThemeManager.GetColor("AppAccent", "#D4AF37")
            : AppThemeManager.GetColor("AppCardBackground", "#101010");
        button.TextColor = selected
            ? AppThemeManager.GetColor("AppHeaderText", "#111111")
            : AppThemeManager.GetColor("AppAccent", "#D4AF37");
        button.BorderColor = AppThemeManager.GetColor("AppBorder", "#7E6421");
        button.BorderWidth = 1;
    }

    internal static ScrollView CreateLayout(string title, string subtitle, params View[] children)
    {
        var stack = new VerticalStackLayout { Padding = new Thickness(18, 18, 18, 28), Spacing = 14 };
        
        var titleLabel = new Label 
        { 
            Text = title.ToUpperInvariant(), 
            FontSize = 20, 
            FontAttributes = FontAttributes.Bold, 
            CharacterSpacing = 2,
            FontFamily = "serif",
            HorizontalTextAlignment = TextAlignment.Center 
        };
        titleLabel.SetDynamicResource(Label.TextColorProperty, "AppBrandText");
        stack.Add(titleLabel);

        var subtitleLabel = new Label { Text = subtitle, FontSize = 14, HorizontalTextAlignment = TextAlignment.Center };
        subtitleLabel.SetDynamicResource(Label.TextColorProperty, "AppTextMuted");
        stack.Add(subtitleLabel);

        var innerStack = new VerticalStackLayout { Spacing = 12 };
        foreach (var child in children)
        {
            innerStack.Add(child);
        }

        var card = new Border
        {
            Padding = 16,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            Content = innerStack
        };
        card.SetDynamicResource(Border.StrokeProperty, "AppBorder");
        card.SetDynamicResource(Border.BackgroundColorProperty, "AppCardBackground");

        stack.Add(card);

        var scrollView = new ScrollView { Content = stack };
        scrollView.SetDynamicResource(VisualElement.BackgroundColorProperty, "AppBackground");
        return scrollView;
    }

    private static Label CreateValueLabel(string text)
    {
        var label = new Label { Text = text, FontSize = 15 };
        label.SetDynamicResource(Label.TextColorProperty, "AppTextPrimary");
        return label;
    }

    private static Label CreateResultLabel()
    {
        var label = new Label { Text = "Sonuç: -", FontSize = 24, FontAttributes = FontAttributes.Bold };
        label.SetDynamicResource(Label.TextColorProperty, "AppBrandText");
        return label;
    }
}
