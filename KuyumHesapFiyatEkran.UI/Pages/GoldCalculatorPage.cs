using KuyumHesapFiyatEkran.Core.Features.LiveRates.Queries.GetLiveRates;
using KuyumHesapFiyatEkran.UI.Services;
using Microsoft.Maui.Controls.Shapes;

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
        goldPicker.ItemDisplayBinding = new Binding(nameof(LiveRateItem.Name));
        StyleInput(goldPicker);
        StyleInput(amountEntry);

        goldPicker.SelectedIndexChanged += (_, _) => Calculate();
        amountEntry.TextChanged += (_, _) => Calculate();
        amountEntry.Completed += (_, _) => amountEntry.Unfocus();
        sellModeButton.Clicked += (_, _) => SelectPriceType(0);
        buyModeButton.Clicked += (_, _) => SelectPriceType(1);

        SelectPriceType(0);
        Content = CreateGoldLayout();
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
            messageLabel.Text = "Altın fiyatları henuz alinamadi. Canli Fiyatlar ekranindan fiyatlar yuklendikten sonra tekrar deneyin.";
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
            messageLabel.Text = "Bu kalem icin fiyat bilgisi henuz alinamadi.";
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

        root.Add(new Border
        {
            Padding = 16,
            Stroke = Color.FromArgb("#8D6F1F"),
            BackgroundColor = Color.FromArgb("#101010"),
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Content = new Grid
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
                        Children =
                        {
                            new Label { Text = "Altın Hesaplama", FontSize = 24, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#FFFFFF") },
                            new Label { Text = "Canli fiyatla hizli TL hesaplama", FontSize = 13, TextColor = Color.FromArgb("#D4AF37") }
                        }
                    },
                    new Border
                    {
                        Padding = 10,
                        BackgroundColor = Color.FromArgb("#050505"),
                        Stroke = Color.FromArgb("#7E6421"),
                        StrokeShape = new RoundRectangle { CornerRadius = 8 },
                        Content = new Label { Text = "₺", FontSize = 24, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#D4AF37") }
                    }
                }
            }
        });

        var iconBorder = (Border)((Grid)((Border)root.Children[0]).Content).Children[1];
        Grid.SetColumn(iconBorder, 1);

        var formStack = new VerticalStackLayout { Spacing = 12 };
        formStack.Add(CreateField("Altın türü", goldPicker));
        formStack.Add(CreateField("Miktar", amountEntry));
        formStack.Add(new Label { Text = "Hesap fiyatı", FontSize = 12, TextColor = Color.FromArgb("#A8872B") });
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
            TextColor = Color.FromArgb("#050505"),
            BackgroundColor = Color.FromArgb("#D4AF37"),
            CornerRadius = 8,
            Padding = new Thickness(14, 12),
            Command = new Command(() =>
            {
                amountEntry.Unfocus();
                Calculate();
            })
        };
        formStack.Add(calculateButton);

        root.Add(new Border
        {
            Padding = 16,
            Stroke = Color.FromArgb("#8D6F1F"),
            BackgroundColor = Color.FromArgb("#0B0B0B"),
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Content = formStack
        });

        root.Add(new Border
        {
            Padding = 16,
            Stroke = Color.FromArgb("#D4AF37"),
            BackgroundColor = Color.FromArgb("#15110A"),
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Content = new VerticalStackLayout
            {
                Spacing = 8,
                Children =
                {
                    unitPriceLabel,
                    resultLabel,
                    messageLabel
                }
            }
        });

        return new ScrollView { BackgroundColor = Color.FromArgb("#050505"), Content = root };
    }

    private static Border CreateField(string title, View input)
    {
        return new Border
        {
            Padding = new Thickness(12, 8),
            Stroke = Color.FromArgb("#7E6421"),
            BackgroundColor = Color.FromArgb("#101010"),
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Content = new VerticalStackLayout
            {
                Spacing = 4,
                Children =
                {
                    new Label { Text = title, FontSize = 12, TextColor = Color.FromArgb("#A8872B") },
                    input
                }
            }
        };
    }

    private static Border CreatePriceTile(string title, Label valueLabel)
    {
        valueLabel.FontSize = 14;
        valueLabel.FontAttributes = FontAttributes.Bold;

        return new Border
        {
            Padding = 12,
            Stroke = Color.FromArgb("#3A2C0E"),
            BackgroundColor = Color.FromArgb("#101010"),
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Content = new VerticalStackLayout
            {
                Spacing = 5,
                Children =
                {
                    new Label { Text = title, FontSize = 12, TextColor = Color.FromArgb("#A8872B") },
                    valueLabel
                }
            }
        };
    }

    private static void StyleInput(InputView input)
    {
        input.BackgroundColor = Colors.Transparent;
        input.TextColor = AppThemeManager.GetColor("AppTextPrimary", "#F1D16A");
        input.PlaceholderColor = AppThemeManager.GetColor("AppTextMuted", "#A8872B");
        input.FontSize = 16;
    }

    private static void StyleInput(Picker input)
    {
        input.BackgroundColor = Colors.Transparent;
        input.TextColor = AppThemeManager.GetColor("AppTextPrimary", "#F1D16A");
        input.TitleColor = AppThemeManager.GetColor("AppTextMuted", "#A8872B");
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
        stack.Add(new Label { Text = title, FontSize = 24, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#111111") });
        stack.Add(new Label { Text = subtitle, FontSize = 14, TextColor = Color.FromArgb("#9A7417") });

        var card = new Border
        {
            Padding = 16,
            Stroke = Color.FromArgb("#D4AF37"),
            BackgroundColor = Color.FromArgb("#FFFFFF"),
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Content = new VerticalStackLayout { Spacing = 12 }
        };

        foreach (var child in children)
        {
            ((VerticalStackLayout)card.Content).Add(child);
        }

        stack.Add(card);
        return new ScrollView { Content = stack };
    }

    private static Label CreateValueLabel(string text) => new() { Text = text, FontSize = 15, TextColor = Color.FromArgb("#F1D16A") };

    private static Label CreateResultLabel() => new() { Text = "Sonuç: -", FontSize = 26, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#FFFFFF") };
}
