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
    private readonly Label buyUnitLabel = CreateValueLabel("-");
    private readonly Label sellUnitLabel = CreateValueLabel("-");
    private readonly Label unitPriceLabel = CreateValueLabel("Birim fiyat: -");
    private readonly Label resultLabel = CreateResultLabel();
    private readonly Label messageLabel = new() { TextColor = Color.FromArgb("#D84555"), IsVisible = false };
    private readonly Label selectedGoldLabel = new() { Text = "Altın türü seçin", FontSize = 16, FontAttributes = FontAttributes.Bold, TextColor = SalihUi.Ink, FontAutoScalingEnabled = false };
    private readonly Label lastUpdateLabel = new() { Text = "-", FontSize = 13, FontAttributes = FontAttributes.Bold, TextColor = SalihUi.Ink, FontAutoScalingEnabled = false, HorizontalTextAlignment = TextAlignment.End };
    private readonly Label selectorTitleLabel = SalihUi.Text("Ürün Türü", 12, false, SalihUi.SecondaryGold);
    private readonly ActivityIndicator categoryLoading = new() { IsRunning = true, Color = SalihUi.Gold, HeightRequest = 44 };
    private readonly ContentView categoryHost = new() { HeightRequest = 50, HorizontalOptions = LayoutOptions.Fill };
    private readonly List<LiveRateItem> goldRates = new();
    private readonly List<RateCategory> categories = new();
    private string? selectedCategoryKey;
    private string? selectedProductKey;
    private int selectedPriceType;

    public GoldCalculatorPage(LiveRatesStore liveRatesStore)
    {
        this.liveRatesStore = liveRatesStore;
        Title = "Altın Hesaplama";
        SetDynamicResource(BackgroundColorProperty, "AppBackground");
        goldPicker.ItemDisplayBinding = new Binding(nameof(LiveRateItem.Name));
        StyleInput(goldPicker);
        StyleInput(amountEntry);

        goldPicker.SelectedIndexChanged += (_, _) => { UpdateSelectedGoldText(); Calculate(); };
        amountEntry.TextChanged += (_, _) => Calculate();
        amountEntry.Completed += (_, _) => amountEntry.Unfocus();
        sellModeButton.Clicked += (_, _) => SelectPriceType(0);
        buyModeButton.Clicked += (_, _) => SelectPriceType(1);

        SelectPriceType(0);
        
        if (string.Equals(BusinessInfo.Current.Key, "SalihCetinkaya", StringComparison.OrdinalIgnoreCase))
        {
            Shell.SetNavBarIsVisible(this, false);
            Content = SalihUi.Page(CreateSalihCalculator(), 2);
        }
        else
        {
            Content = CreateGoldLayout();
        }

        ApplyTheme();
        AppThemeManager.ThemeChanged += OnThemeChanged;
    }

    private View CreateSalihCalculator()
    {
        BackgroundColor = SalihUi.Cream;
        selectedGoldLabel.TextColor = SalihUi.Ink; lastUpdateLabel.TextColor = SalihUi.Ink; selectorTitleLabel.TextColor = SalihUi.SecondaryGold;
        var stack = new VerticalStackLayout { Padding = new Thickness(20, 0, 20, 24), Spacing = 16 };
        stack.Add(CreateCalculatorHeader());
        categoryHost.Content = categoryLoading;
        stack.Add(categoryHost);

        stack.Add(CreateGoldSelector());
        if (string.IsNullOrWhiteSpace(amountEntry.Text)) amountEntry.Text = "1";
        stack.Add(CreateAmountCard());

        var prices = new Grid { ColumnSpacing = 10, ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) } };
        prices.Add(LightPrice("Alış Tutarı", buyPriceLabel)); var sell = LightPrice("Satış Tutarı", sellPriceLabel); Grid.SetColumn(sell, 1); prices.Add(sell); stack.Add(prices);
        var calculate = new Button { Text = "Hesapla", HeightRequest = 56, FontSize = 17, FontAttributes = FontAttributes.Bold, TextColor = Colors.White, Background = SalihUi.GoldGradient(), CornerRadius = 13, Command = new Command(() => { amountEntry.Unfocus(); Calculate(); }) }; stack.Add(calculate);
        stack.Add(new BoxView { HeightRequest = 1, Color = SalihUi.Divider, Margin = new Thickness(0, 12, 0, 8) });
        stack.Add(SalihUi.Text("Güncel Fiyat Bilgisi", 18, true));
        stack.Add(LightInfo("Alış Fiyatı", buyUnitLabel));
        stack.Add(LightInfo("Satış Fiyatı", sellUnitLabel));
        stack.Add(LightInfo("Son Güncelleme", lastUpdateLabel));
        stack.Add(messageLabel);
        return new ScrollView { BackgroundColor = SalihUi.Cream, Content = stack };
    }

    private View CreateGoldSelector()
    {
        var row = new Grid { ColumnDefinitions = { new ColumnDefinition(new GridLength(42)), new ColumnDefinition(GridLength.Star), new ColumnDefinition(new GridLength(28)) } };
        row.Add(new LucideIcon(LucideIconKind.CircleDollarSign, 28, SalihUi.Gold)); Grid.SetColumn(selectedGoldLabel, 1); row.Add(selectedGoldLabel);
        var chevron = new LucideIcon(LucideIconKind.ChevronRight, 20, SalihUi.Gold); Grid.SetColumn(chevron, 2); row.Add(chevron);
        var card = SalihUi.Card(new VerticalStackLayout { Spacing = 8, Children = { selectorTitleLabel, row } }, radius: 16, padding: new Thickness(16, 12));
        var tap = new TapGestureRecognizer(); tap.Tapped += async (_, _) => await ShowProductSelectorAsync(); card.GestureRecognizers.Add(tap); return card;
    }

    private View CreateCalculatorHeader()
    {
        var header = SalihUi.Header("Altın Hesaplama");
        var refresh = new Grid { Children = { new LucideIcon(LucideIconKind.RefreshCw, 22, SalihUi.Ink) } }; Grid.SetColumn(refresh, 2);
        var tap = new TapGestureRecognizer(); tap.Tapped += async (_, _) => await RefreshRatesAsync(); refresh.GestureRecognizers.Add(tap); header.Add(refresh); return header;
    }

    private View CreateAmountCard()
    {
        amountEntry.BackgroundColor = Colors.Transparent; amountEntry.TextColor = SalihUi.Ink; amountEntry.PlaceholderColor = SalihUi.Muted; amountEntry.FontSize = 21; amountEntry.FontAttributes = FontAttributes.Bold; amountEntry.Margin = 0; amountEntry.HeightRequest = 38;
        var row = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) } }; row.Add(amountEntry);
        var unit = SalihUi.Text("Adet", 13, false, SalihUi.Muted); Grid.SetColumn(unit, 1); row.Add(unit);
        return SalihUi.Card(new VerticalStackLayout { Spacing = 2, Children = { SalihUi.Text("Miktar", 12, false, SalihUi.SecondaryGold), row } }, radius: 16, padding: new Thickness(16, 10));
    }

    private static Border LightPrice(string title, Label value)
    {
        value.TextColor = SalihUi.Ink; value.FontSize = 19; value.FontAttributes = FontAttributes.Bold; value.LineBreakMode = LineBreakMode.NoWrap;
        return SalihUi.Card(new VerticalStackLayout { Spacing = 10, Children = { SalihUi.Text(title, 13, false, SalihUi.SecondaryGold), value } }, SalihUi.SurfaceElevated, 15, new Thickness(14, 14));
    }

    private static View LightInfo(string title, Label source)
    {
        var grid = new Grid { Padding = new Thickness(0, 4), ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) } }; grid.Add(SalihUi.Text(title, 13, false, SalihUi.Muted));
        var copy = SalihUi.Text(source.Text, 13, true, SalihUi.Ink); copy.SetBinding(Label.TextProperty, new Binding(nameof(Label.Text), source: source)); copy.HorizontalTextAlignment = TextAlignment.End; Grid.SetColumn(copy, 1); grid.Add(copy); return grid;
    }

    private static Label Center(string text, Color color, bool bold = false) { var l = SalihUi.Text(text, 13, bold, color); l.HorizontalTextAlignment = TextAlignment.Center; return l; }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await liveRatesStore.EnsureLoadedAsync(CancellationToken.None);
        LoadCategories();
        lastUpdateLabel.Text = string.IsNullOrWhiteSpace(liveRatesStore.CurrentRates?.LastUpdate) ? "-" : liveRatesStore.CurrentRates.LastUpdate;
        Calculate();
        ApplyTheme();
    }

    private void Calculate()
    {
        messageLabel.IsVisible = false;
        var rate = goldPicker.SelectedItem as LiveRateItem;
        var priceText = selectedPriceType == 1 ? rate?.Buy : rate?.Sell;
        var unitPrice = LiveRatesStore.ParsePrice(priceText);
        var salih = string.Equals(BusinessInfo.Current.Key, "SalihCetinkaya", StringComparison.OrdinalIgnoreCase);
        var validAmount = decimal.TryParse(amountEntry.Text?.Replace(',', '.'), System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var amount) && amount > 0;
        buyPriceLabel.Text = salih
            ? FormatSalihPrice(rate?.Buy, validAmount ? amount : 1)
            : string.IsNullOrWhiteSpace(rate?.Buy) ? "Alış: -" : $"Alış: {rate.Buy}";
        sellPriceLabel.Text = salih
            ? FormatSalihPrice(rate?.Sell, validAmount ? amount : 1)
            : string.IsNullOrWhiteSpace(rate?.Sell) ? "Satış: -" : $"Satış: {rate.Sell}";
        buyUnitLabel.Text = FormatSalihPrice(rate?.Buy, 1);
        sellUnitLabel.Text = FormatSalihPrice(rate?.Sell, 1);
        unitPriceLabel.Text = unitPrice > 0 ? $"Birim fiyat: {LiveRatesStore.FormatCurrency(unitPrice)}" : "Birim fiyat: -";

        if (goldRates.Count == 0)
        {
            resultLabel.Text = "Sonuç: -";
            messageLabel.Text = "Altın fiyatları henüz alınamadı. Canlı Fiyatlar ekranından fiyatlar yüklendikten sonra tekrar deneyin.";
            messageLabel.IsVisible = true;
            return;
        }

        if (!validAmount)
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

    private void LoadCategories()
    {
        var currentRates = liveRatesStore.CurrentRates;
        categories.Clear();
        if (currentRates is not null)
        {
            AddCategory("Sarrafiye", "Altın", currentRates.Sarrafiye);
            AddCategory("Maden", "Gümüş", currentRates.Maden);
            AddCategory("Doviz", "Döviz", currentRates.Doviz);
        }
        selectedCategoryKey = categories.Any(x => x.Key == selectedCategoryKey) ? selectedCategoryKey : categories.FirstOrDefault()?.Key;
        BuildCategoryTabs(); LoadSelectedCategory();
    }

    private void AddCategory(string key, string title, IEnumerable<LiveRateItem> items)
    {
        var usable = items.Where(HasUsablePrice).ToList(); if (usable.Count > 0) categories.Add(new RateCategory(key, title, usable));
    }

    private void LoadSelectedCategory()
    {
        var category = categories.FirstOrDefault(x => x.Key == selectedCategoryKey);
        var previousKey = selectedProductKey;
        goldRates.Clear(); if (category is not null) goldRates.AddRange(category.Items);
        goldPicker.ItemsSource = null;
        goldPicker.ItemsSource = goldRates;
        var selectedIndex = previousKey is null ? -1 : goldRates.FindIndex(x => LiveRatesStore.GetRateKey(x) == previousKey);
        goldPicker.SelectedIndex = selectedIndex >= 0 ? selectedIndex : goldRates.Count > 0 ? 0 : -1;
        selectedProductKey = goldPicker.SelectedItem is LiveRateItem item ? LiveRatesStore.GetRateKey(item) : null;
        selectorTitleLabel.Text = category?.Key switch { "Sarrafiye" => "Altın Türü", "Doviz" => "Döviz Türü", _ => "Ürün Türü" };
        if (category is null) { selectedGoldLabel.Text = liveRatesStore.LastError is null ? "Bu kategori için ürün bulunamadı." : "Fiyat bilgileri alınamadı."; }
        UpdateSelectedGoldText();
    }

    private void UpdateSelectedGoldText()
    {
        if (goldPicker.SelectedItem is LiveRateItem item) { selectedProductKey = LiveRatesStore.GetRateKey(item); selectedGoldLabel.Text = DisplayName(item.Name); }
    }

    private static string DisplayName(string name) => System.Globalization.CultureInfo.GetCultureInfo("tr-TR").TextInfo.ToTitleCase(name.ToLowerInvariant());

    private static string FormatSalihPrice(string? price, decimal amount)
    {
        if (string.IsNullOrWhiteSpace(price))
        {
            return "-";
        }

        var value = LiveRatesStore.ParsePrice(price);
        if (value <= 0)
        {
            return price.Contains("TL", StringComparison.OrdinalIgnoreCase) ? price : $"{price} TL";
        }

        return LiveRatesStore.FormatCurrency(value * amount);
    }

    private void BuildCategoryTabs()
    {
        categoryLoading.IsRunning = false; categoryLoading.IsVisible = false;
        if (categories.Count == 0)
        {
            categoryHost.HorizontalOptions = LayoutOptions.Fill;
            categoryHost.Content = Center(liveRatesStore.LastError is null ? "Ürün bulunamadı" : "Fiyatlar alınamadı", SalihUi.Muted);
            return;
        }

        if (categories.Count == 1)
        {
            categoryHost.HorizontalOptions = LayoutOptions.Start;
            categoryHost.Content = CreateCategoryPill(categories[0], 124, true);
            return;
        }

        categoryHost.HorizontalOptions = LayoutOptions.Fill;
        if (categories.Count == 2)
        {
            var grid = new Grid { Padding = 3, ColumnSpacing = 0, ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) } };
            for (var i = 0; i < 2; i++) { var pill = CreateCategoryPill(categories[i], -1, categories[i].Key == selectedCategoryKey); Grid.SetColumn(pill, i); grid.Add(pill); }
            categoryHost.Content = new Border { BackgroundColor = SalihUi.SurfaceSoft, Stroke = SalihUi.Line, StrokeThickness = 1, StrokeShape = new RoundRectangle { CornerRadius = 25 }, Content = grid };
            return;
        }

        var tabs = new HorizontalStackLayout { Spacing = 7, Padding = new Thickness(3) };
        foreach (var category in categories) tabs.Add(CreateCategoryPill(category, 96, category.Key == selectedCategoryKey));
        categoryHost.Content = new ScrollView { Orientation = ScrollOrientation.Horizontal, HorizontalScrollBarVisibility = ScrollBarVisibility.Never, Content = tabs };
    }

    private Border CreateCategoryPill(RateCategory category, double minimumWidth, bool selected)
    {
        var pill = new Border { HeightRequest = 46, MinimumWidthRequest = minimumWidth, Padding = new Thickness(20, 0), Background = selected ? SalihUi.GoldGradient() : new SolidColorBrush(SalihUi.SurfaceSoft), Stroke = SalihUi.Line, StrokeThickness = selected ? 0 : 1, StrokeShape = new RoundRectangle { CornerRadius = 23 }, Content = Center(category.Title, selected ? Colors.White : SalihUi.Muted, selected) };
        var key = category.Key; var tap = new TapGestureRecognizer(); tap.Tapped += (_, _) => SelectCategory(key); pill.GestureRecognizers.Add(tap); return pill;
    }

    private void SelectCategory(string key)
    {
        if (selectedCategoryKey == key) return; selectedCategoryKey = key; selectedProductKey = null; BuildCategoryTabs(); LoadSelectedCategory(); Calculate();
    }

    private async Task ShowProductSelectorAsync()
    {
        if (goldRates.Count == 0) { messageLabel.Text = liveRatesStore.LastError is null ? "Bu kategori için ürün bulunamadı." : "Fiyat bilgileri alınamadı. Tekrar deneyin."; messageLabel.IsVisible = true; return; }
        var category = categories.First(x => x.Key == selectedCategoryKey);
        await Navigation.PushModalAsync(new ProductSelectionPage(category.Title, goldRates, selectedProductKey, item =>
        {
            var index = goldRates.FindIndex(x => LiveRatesStore.GetRateKey(x) == LiveRatesStore.GetRateKey(item));
            if (index >= 0) goldPicker.SelectedIndex = index;
        }));
    }

    private async Task RefreshRatesAsync()
    {
        categoryLoading.IsVisible = true; categoryLoading.IsRunning = true; categoryHost.HorizontalOptions = LayoutOptions.Fill; categoryHost.Content = categoryLoading; messageLabel.IsVisible = false;
        await liveRatesStore.LoadAsync(CancellationToken.None); LoadCategories();
        lastUpdateLabel.Text = string.IsNullOrWhiteSpace(liveRatesStore.CurrentRates?.LastUpdate) ? "-" : liveRatesStore.CurrentRates.LastUpdate; Calculate();
    }

    private static bool HasUsablePrice(LiveRateItem item)
    {
        return LiveRatesStore.ParsePrice(item.Buy) > 0 || LiveRatesStore.ParsePrice(item.Sell) > 0;
    }

    private sealed record RateCategory(string Key, string Title, List<LiveRateItem> Items);

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
        if (string.Equals(BusinessInfo.Current.Key, "SalihCetinkaya", StringComparison.OrdinalIgnoreCase))
        {
            Content = null;
            BackgroundColor = SalihUi.Cream;
            Content = SalihUi.Page(CreateSalihCalculator(), 2);
            if (categories.Count > 0) BuildCategoryTabs();
            buyPriceLabel.TextColor = SalihUi.Ink;
            sellPriceLabel.TextColor = SalihUi.Ink;
            messageLabel.TextColor = Color.FromArgb("#D84555");
            return;
        }
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
