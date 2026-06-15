using System.ComponentModel;
using KuyumHesapFiyatEkran.Core.Features.LiveRates.Queries.GetLiveRates;
using KuyumHesapFiyatEkran.UI.Services;
using KuyumHesapFiyatEkran.UI.ViewModels;
using Microsoft.Maui.Controls.Shapes;

namespace KuyumHesapFiyatEkran.UI
{
    public partial class MainPage : ContentPage, IDisposable
    {
        private const double TickerStep = 1.8;
        private const double TickerEstimatedCharacterWidth = 9.8;
        private const double TickerHorizontalPadding = 60;
        private static readonly TimeSpan TickerFrameInterval = TimeSpan.FromMilliseconds(16);
        private static readonly TimeSpan LiveRatesRefreshInterval = TimeSpan.FromSeconds(15);

        private readonly MainPageViewModel viewModel;
        private readonly BrandSettings brandSettings;
        private readonly bool isSalihBrand;
        private IDispatcherTimer? tickerTimer;
        private IDispatcherTimer? liveRatesTimer;
        private CancellationTokenSource? refreshCancellationTokenSource;
        private bool isRefreshInProgress;
        private bool isTickerDragging;
        private bool isThemeChanging;
        private bool? isLandscapeLayout;
        private double tickerDragStartX;
        private bool disposed;

        public MainPage(MainPageViewModel viewModel, BrandSettings brandSettings)
        {
            InitializeComponent();
            this.viewModel = viewModel;
            this.brandSettings = brandSettings;
            isSalihBrand = string.Equals(brandSettings.Key, "SalihCetinkaya", StringComparison.OrdinalIgnoreCase);
            ApplyBranding();
            ApplyTheme(AppThemeManager.GetSavedTheme(), save: false);
            ApplyBrandLayout();
            SalihHeaderArt.Drawable = isSalihBrand ? new SalihHeaderDrawable() : null;
            BindingContext = viewModel;
            viewModel.PropertyChanged += OnViewModelPropertyChanged;
            UpdateCategoryTabs();
            App.AppSleeping += OnAppSleeping;
            App.AppResumed += OnAppResumed;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            StartTicker();
            StartLiveRatesRefresh();
            await RefreshLiveRatesAsync(updateTicker: true, showBusy: true);
            UpdateTickerWidth();
        }

        protected override void OnDisappearing()
        {
            StopLiveActivity();
            base.OnDisappearing();
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            if (width <= 0 || height <= 0)
            {
                return;
            }

            ApplyResponsiveLayout(width > height);
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            StopLiveActivity();
            viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            App.AppSleeping -= OnAppSleeping;
            App.AppResumed -= OnAppResumed;
            refreshCancellationTokenSource?.Dispose();
        }

        private void StartTicker()
        {
            tickerTimer ??= Dispatcher.CreateTimer();
            tickerTimer.Interval = TickerFrameInterval;
            tickerTimer.Tick -= OnTickerTick;
            tickerTimer.Tick += OnTickerTick;

            if (!tickerTimer.IsRunning)
            {
                tickerTimer.Start();
            }
        }

        private void StartLiveRatesRefresh()
        {
            liveRatesTimer ??= Dispatcher.CreateTimer();
            liveRatesTimer.Interval = LiveRatesRefreshInterval;
            liveRatesTimer.Tick -= OnLiveRatesTick;
            liveRatesTimer.Tick += OnLiveRatesTick;

            if (!liveRatesTimer.IsRunning)
            {
                liveRatesTimer.Start();
            }
        }

        private void StopLiveActivity()
        {
            tickerTimer?.Stop();
            liveRatesTimer?.Stop();
            CancelCurrentRefresh();
        }

        private void OnTickerTick(object? sender, EventArgs e)
        {
            if (isSalihBrand)
            {
                ScrollSalihTicker();
                return;
            }

            if (string.IsNullOrWhiteSpace(viewModel.TickerText))
            {
                return;
            }

            var segmentWidth = TickerSegment.Width + TickerContent.Spacing;
            var viewportWidth = TickerViewport.Width;

            if (segmentWidth <= 0 || viewportWidth <= 0)
            {
                return;
            }

            if (isTickerDragging)
            {
                return;
            }

            TickerContent.TranslationX = NormalizeTickerX(TickerContent.TranslationX - TickerStep);
        }

        private void OnTickerPanUpdated(object? sender, PanUpdatedEventArgs e)
        {
            if (isSalihBrand)
            {
                switch (e.StatusType)
                {
                    case GestureStatus.Started:
                        isTickerDragging = true;
                        tickerDragStartX = NormalizeSalihTickerX(SalihTickerScrollingContent.TranslationX);
                        break;

                    case GestureStatus.Running:
                        SalihTickerScrollingContent.TranslationX = NormalizeSalihTickerX(tickerDragStartX + e.TotalX);
                        break;

                    case GestureStatus.Completed:
                    case GestureStatus.Canceled:
                        SalihTickerScrollingContent.TranslationX = NormalizeSalihTickerX(SalihTickerScrollingContent.TranslationX);
                        isTickerDragging = false;
                        break;
                }
                return;
            }

            if (string.IsNullOrWhiteSpace(viewModel.TickerText))
            {
                return;
            }

            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    isTickerDragging = true;
                    tickerDragStartX = NormalizeTickerX(TickerContent.TranslationX);
                    break;

                case GestureStatus.Running:
                    TickerContent.TranslationX = NormalizeTickerX(tickerDragStartX + e.TotalX);
                    break;

                case GestureStatus.Completed:
                case GestureStatus.Canceled:
                    TickerContent.TranslationX = NormalizeTickerX(TickerContent.TranslationX);
                    isTickerDragging = false;
                    break;
            }
        }

        private async void OnLiveRatesTick(object? sender, EventArgs e)
        {
            await RefreshLiveRatesAsync(updateTicker: false, showBusy: false);
        }

        private async Task RefreshLiveRatesAsync(bool updateTicker, bool showBusy)
        {
            if (isRefreshInProgress || viewModel.IsBusy)
            {
                return;
            }

            try
            {
                isRefreshInProgress = true;
                CancelCurrentRefresh();
                refreshCancellationTokenSource = new CancellationTokenSource();
                await viewModel.LoadAsync(refreshCancellationTokenSource.Token, updateTicker, showBusy);
            }
            finally
            {
                isRefreshInProgress = false;
            }
        }

        private void CancelCurrentRefresh()
        {
            if (refreshCancellationTokenSource is null)
            {
                return;
            }

            refreshCancellationTokenSource.Cancel();
            refreshCancellationTokenSource.Dispose();
            refreshCancellationTokenSource = null;
        }

        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(MainPageViewModel.SelectedCategory)
                or nameof(MainPageViewModel.IsSarrafiyeSelected)
                or nameof(MainPageViewModel.IsMadenSelected)
                or nameof(MainPageViewModel.IsDovizSelected)
                or nameof(MainPageViewModel.IsSarrafiyeEnabled)
                or nameof(MainPageViewModel.IsMadenEnabled)
                or nameof(MainPageViewModel.IsDovizEnabled))
            {
                MainThread.BeginInvokeOnMainThread(UpdateCategoryTabs);
            }

            if (e.PropertyName != nameof(MainPageViewModel.TickerText))
            {
                return;
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                BuildTickerSegments();
                UpdateTickerWidth();

                var contentWidth = TickerSegment.Width + TickerContent.Spacing;

                if (contentWidth > 0 && TickerContent.TranslationX <= -contentWidth)
                {
                    TickerContent.TranslationX += contentWidth;
                }
            });
        }

        private void BuildTickerSegments()
        {
            if (isSalihBrand)
            {
                SalihTickerSegment.Children.Clear();
                SalihTickerSegmentClone.Children.Clear();

                var allRates = GetAllSalihRates();
                foreach (var item in allRates)
                {
                    SalihTickerSegment.Children.Add(CreateSalihScrollTickerItem(item));
                    SalihTickerSegmentClone.Children.Add(CreateSalihScrollTickerItem(item));
                }

                UpdateSalihTickerIcons();

                TickerContent.IsVisible = false;
                SalihTickerBorder.IsVisible = true;
                return;
            }

            TickerContent.IsVisible = true;
            SalihTickerBorder.IsVisible = false;
            TickerSegmentContent.Children.Clear();
            TickerSegmentCloneContent.Children.Clear();
            TickerSegmentClone.IsVisible = true;
            TickerContent.Spacing = 28;

            foreach (var item in viewModel.TickerRates)
            {
                TickerSegmentContent.Children.Add(CreateTickerItem(item));
                TickerSegmentCloneContent.Children.Add(CreateTickerItem(item));
            }
        }

        private static HorizontalStackLayout CreateTickerItem(LiveRateItem item)
        {
            var percentColor = item.ChangePercent.TrimStart().StartsWith("-", StringComparison.Ordinal)
                ? GetThemeColor("AppNegative", "#E34F61")
                : GetThemeColor("AppPositive", "#26BF72");

            var layout = new HorizontalStackLayout
            {
                Spacing = 4,
                VerticalOptions = LayoutOptions.Center
            };

            var tickerTextColor = GetThemeColor("AppTickerText", "#F1D16A");

            layout.Add(CreateTickerLabel(item.Name, tickerTextColor));
            layout.Add(CreateTickerLabel("ALIŞ:", tickerTextColor));
            layout.Add(CreateTickerLabel(item.Buy, tickerTextColor));
            layout.Add(CreateTickerLabel(item.ChangePercent, percentColor));
            layout.Add(CreateTickerLabel("SATIŞ:", tickerTextColor));
            layout.Add(CreateTickerLabel(item.Sell, tickerTextColor));
            layout.Add(CreateTickerLabel("|", tickerTextColor));

            return layout;
        }

        private async void OnThemeToggleTapped(object? sender, TappedEventArgs e)
        {
            if (isThemeChanging)
            {
                return;
            }

            var nextMode = AppThemeManager.GetSavedTheme() == AslanThemeMode.Light
                ? AslanThemeMode.Dark
                : AslanThemeMode.Light;

            try
            {
                isThemeChanging = true;
                
                var toggleBorder = ThemeToggle;
                var toggleIcon = ThemeToggleIcon;

                await Task.WhenAll(
                    toggleBorder.ScaleTo(0.9, 80, Easing.CubicOut),
                    toggleIcon.RotateTo(18, 80, Easing.CubicOut),
                    RootLayout.FadeTo(0.82, 110, Easing.CubicOut));

                ApplyTheme(nextMode);

                toggleIcon.Rotation = -18;
                await Task.WhenAll(
                    toggleBorder.ScaleTo(1, 150, Easing.SpringOut),
                    toggleIcon.RotateTo(0, 170, Easing.CubicInOut),
                    RootLayout.FadeTo(1, 190, Easing.CubicIn));
            }
            finally
            {
                RootLayout.Opacity = 1;
                ThemeToggle.Scale = 1;
                ThemeToggleIcon.Rotation = 0;
                isThemeChanging = false;
            }
        }

        private void ApplyTheme(AslanThemeMode mode, bool save = true)
        {
            AppThemeManager.Apply(mode, Resources, save);
            if (isSalihBrand)
            {
                ApplySalihThemeResources(mode);
            }

            UpdateThemeSelector(mode);
            RefreshRateRows();
            BuildTickerSegments();
            UpdateTickerWidth();
            UpdateCategoryTabs();
        }

        private void UpdateCategoryTabs()
        {
            if (SarrafiyeTabIndicator is null || MadenTabIndicator is null || DovizTabIndicator is null)
            {
                return;
            }

            SarrafiyeTabIndicator.IsVisible = viewModel.IsSarrafiyeSelected;
            MadenTabIndicator.IsVisible = viewModel.IsMadenSelected;
            DovizTabIndicator.IsVisible = viewModel.IsDovizSelected;

            // Set column widths dynamically based on enablement
            SarrafiyeTabColumn.Width = viewModel.IsSarrafiyeEnabled ? GridLength.Star : new GridLength(0);
            MadenTabColumn.Width = viewModel.IsMadenEnabled ? GridLength.Star : new GridLength(0);
            DovizTabColumn.Width = viewModel.IsDovizEnabled ? GridLength.Star : new GridLength(0);

            // Set visibility of cell frames/labels based on enablement
            SarrafiyeTabFrame.IsVisible = viewModel.IsSarrafiyeEnabled;
            SarrafiyeTabCell.IsVisible = viewModel.IsSarrafiyeEnabled;
            MadenTabFrame.IsVisible = viewModel.IsMadenEnabled;
            MadenTabCell.IsVisible = viewModel.IsMadenEnabled;
            DovizTabFrame.IsVisible = viewModel.IsDovizEnabled;
            DovizTabCell.IsVisible = viewModel.IsDovizEnabled;

            if (isSalihBrand)
            {
                UpdateSalihCategoryTabs();
                return;
            }

            SarrafiyeTabLabel.TextColor = GetThemeColor(
                viewModel.IsSarrafiyeSelected ? "AppTextPrimary" : "AppTextInactive",
                viewModel.IsSarrafiyeSelected ? "#2F2A1D" : "#A8872B");
            MadenTabLabel.TextColor = GetThemeColor(
                viewModel.IsMadenSelected ? "AppTextPrimary" : "AppTextInactive",
                viewModel.IsMadenSelected ? "#2F2A1D" : "#A8872B");
            DovizTabLabel.TextColor = GetThemeColor(
                viewModel.IsDovizSelected ? "AppTextPrimary" : "AppTextInactive",
                viewModel.IsDovizSelected ? "#2F2A1D" : "#A8872B");
        }

        private void RefreshRateRows()
        {
            if (RatesCollectionView is null || viewModel is null)
            {
                return;
            }

            RatesCollectionView.ItemsSource = null;
            RatesCollectionView.ItemsSource = viewModel.DisplayRates;
        }

        private void ApplyResponsiveLayout(bool isLandscape)
        {
            if (isLandscapeLayout == isLandscape)
            {
                return;
            }

            isLandscapeLayout = isLandscape;

            if (isSalihBrand)
            {
                ApplySalihResponsiveLayout(isLandscape);
                return;
            }

            ContentGrid.Padding = isLandscape ? new Thickness(8, 4, 8, 6) : new Thickness(4, 8, 4, 18);
            HeaderCard.Padding = isLandscape ? new Thickness(10, 6, 6, 6) : new Thickness(8, 6, 6, 6);
            HeaderCard.HeightRequest = isLandscape ? 60 : 72;
            HeaderContent.RowSpacing = 0;
            HeaderTopGrid.HeightRequest = isLandscape ? 44 : 56;
            HeaderTopGrid.ColumnDefinitions[0].Width = new GridLength(isLandscape ? 32 : 34);
            HeaderTopGrid.ColumnDefinitions[1].Width = new GridLength(isLandscape ? 116 : 146);
            HeaderTopGrid.ColumnDefinitions[3].Width = new GridLength(isLandscape ? 46 : 50);
            HeaderTopGrid.ColumnSpacing = isLandscape ? 5 : 6;
            BrandLogoFrame.WidthRequest = isLandscape ? 32 : 40;
            BrandLogoFrame.HeightRequest = isLandscape ? 32 : 40;
            BrandLogoImage.WidthRequest = isLandscape ? 32 : 40;
            BrandLogoImage.HeightRequest = isLandscape ? 32 : 40;
            BrandTextStack.WidthRequest = isLandscape ? 82 : 98;
            ApplyBrandTextSizing(isLandscape);
            ThemeToggle.WidthRequest = isLandscape ? 38 : 42;
            ThemeToggle.HeightRequest = isLandscape ? 38 : 42;
            ThemeToggleIcon.FontSize = isLandscape ? 21 : 23;
            CategoryTabs.HeightRequest = isLandscape ? 30 : 34;
            TableHeader.Padding = isLandscape ? new Thickness(10, 6) : new Thickness(10, 10);
            TickerViewport.HeightRequest = isLandscape ? 38 : 54;

            Resources["RateRowHeight"] = isLandscape ? 46d : 58d;
            Resources["RateRowPadding"] = isLandscape ? new Thickness(10, 2) : new Thickness(10, 5);

            LastUpdateContainer.HorizontalOptions = LayoutOptions.End;
            LastUpdateTitleLabel.HorizontalTextAlignment = TextAlignment.End;
            LastUpdateValueLabel.HorizontalTextAlignment = TextAlignment.End;

            RefreshRateRows();
            UpdateTickerWidth();
        }

        private void ApplyBranding()
        {
            BrandAslanLabel.Text = brandSettings.PrimaryName;
            BrandKuyumculukLabel.Text = brandSettings.SecondaryName;
            SalihBrandNameLabel.Text = brandSettings.PrimaryName?.ToUpperInvariant();
            SalihBrandSubtitleLabel.Text = brandSettings.SecondaryName?.ToUpperInvariant();
            SalihLogoMonogramLabel.Text = brandSettings.LogoMonogram;
            BrandLogoFallback.Text = brandSettings.LogoMonogram;
            LoadingLogoFallback.Text = brandSettings.LogoMonogram;

            var hasLogo = !string.IsNullOrWhiteSpace(brandSettings.LogoImage);
            BrandLogoImage.IsVisible = hasLogo;
            BrandLogoFallback.IsVisible = !hasLogo;
            LoadingLogoImage.IsVisible = hasLogo;
            LoadingLogoFallback.IsVisible = !hasLogo;

            if (hasLogo)
            {
                BrandLogoImage.Source = brandSettings.LogoImage;
                LoadingLogoImage.Source = brandSettings.LogoImage;
                if (isSalihBrand)
                {
                    SalihLogoImage.Source = brandSettings.LogoImage;
                }
            }
        }

        private void ApplyBrandLayout()
        {
            if (!isSalihBrand)
            {
                SalihHeaderCard.IsVisible = false;
                HeaderCard.IsVisible = true;
                return;
            }

            SalihHeaderCard.IsVisible = false;
            HeaderCard.IsVisible = true;

            var bottomBar = new Views.BottomNavigationBar(0);
            SalihBottomNavigationContainer.Children.Clear();
            SalihBottomNavigationContainer.Children.Add(bottomBar);
            SalihBottomNavigationContainer.IsVisible = true;
            ThemeToggle.IsVisible = true;
            ContentGrid.RowSpacing = 0;
            CategoryTabs.Margin = new Thickness(12, 4, 12, 6);

            // Remove the redundant initial loading overlay for Sinan brand
            LoadingOverlay.IsVisible = false;
            LoadingOverlay.RemoveBinding(Grid.IsVisibleProperty);
            // Enable vector icons and clean up tab label texts
            SarrafiyeTabIcon.IsVisible = true;
            MadenTabIcon.IsVisible = true;
            DovizTabIcon.IsVisible = true;

            SarrafiyeTabLabel.Text = "SARRAFİYE";
            MadenTabLabel.Text = "MADEN";
            DovizTabLabel.Text = "DÖVİZ";
            SarrafiyeTabLabel.FontSize = 14;
            MadenTabLabel.FontSize = 14;
            DovizTabLabel.FontSize = 14;
            TableCategoryHeaderLabel.Text = "Ürün";
            TableBuyHeaderLabel.Text = "Alış";
            TableSellHeaderLabel.Text = "Satış";
            TableCategoryHeaderLabel.FontSize = 15;
            TableBuyHeaderLabel.FontSize = 15;
            TableSellHeaderLabel.FontSize = 15;
            RatesTableBorder.StrokeShape = new RoundRectangle { CornerRadius = 14 };
            RatesTableBorder.Margin = new Thickness(12, 0, 12, 0);
            TickerViewport.Margin = new Thickness(12, 6, 12, 4);
            TickerSegment.StrokeShape = new RoundRectangle { CornerRadius = 14 };
            TickerSegment.Padding = new Thickness(10, 8);
            TickerSegment.Background = CreateVerticalGradient("#101010", "#080808");
            SalihThemeToggle.Shadow = CreateSoftGoldShadow();
        }

        private void ApplySalihThemeResources(AslanThemeMode mode)
        {
            var isLight = mode == AslanThemeMode.Light;

            if (isLight)
            {
                Resources["AppBackground"] = Color.FromArgb("#FAF6EA");
                Resources["AppCardBackground"] = Color.FromArgb("#FFFFFF");
                Resources["AppPanelBackground"] = Color.FromArgb("#F6EEDC");
                Resources["AppAlternateRowBackground"] = Color.FromArgb("#F9F4E6");
                Resources["AppBorder"] = Color.FromArgb("#C5A028");
                Resources["AppAccent"] = Color.FromArgb("#9A7417");
                Resources["AppTextPrimary"] = Color.FromArgb("#2F2A1D");
                Resources["AppTextMuted"] = Color.FromArgb("#A8872B");
                Resources["AppTextInactive"] = Color.FromArgb("#8B6D20");
                Resources["AppBrandText"] = Color.FromArgb("#231F20");
                Resources["AppHeaderText"] = Color.FromArgb("#FAF6EA");
                Resources["AppPositive"] = Color.FromArgb("#1FA463");
                Resources["AppNegative"] = Color.FromArgb("#D84555");
                Resources["AppTickerBackground"] = Color.FromArgb("#FAF6EA");
                Resources["AppTickerCardBackground"] = Color.FromArgb("#FFFFFF");
                Resources["AppTickerText"] = Color.FromArgb("#2F2A1D");
                Resources["AppLoadingBackground"] = Color.FromArgb("#FAF6EA");
                Resources["RateNameText"] = Color.FromArgb("#2F2A1D");
                Resources["RatePriceText"] = Color.FromArgb("#2F2A1D");
                Resources["RateDivider"] = Color.FromRgba(197, 160, 40, 0.22f);
            }
            else
            {
                Resources["AppBackground"] = Color.FromArgb("#070707");
                Resources["AppCardBackground"] = Color.FromArgb("#121212");
                Resources["AppPanelBackground"] = Color.FromArgb("#0D0D0D");
                Resources["AppAlternateRowBackground"] = Color.FromArgb("#141414");
                Resources["AppBorder"] = Color.FromArgb("#3A2C0E");
                Resources["AppAccent"] = Color.FromArgb("#D4AF37");
                Resources["AppTextPrimary"] = Color.FromArgb("#FFFFFF");
                Resources["AppTextMuted"] = Color.FromArgb("#8A8A8A");
                Resources["AppTextInactive"] = Color.FromArgb("#FFFFFF");
                Resources["AppBrandText"] = Color.FromArgb("#D4AF37");
                Resources["AppHeaderText"] = Color.FromArgb("#FFFFFF");
                Resources["AppPositive"] = Color.FromArgb("#31C278");
                Resources["AppNegative"] = Color.FromArgb("#FF4F5E");
                Resources["AppTickerBackground"] = Color.FromArgb("#070707");
                Resources["AppTickerCardBackground"] = Color.FromArgb("#121212");
                Resources["AppTickerText"] = Color.FromArgb("#D4AF37");
                Resources["AppLoadingBackground"] = Color.FromArgb("#070707");
                Resources["RateNameText"] = Color.FromArgb("#FFFFFF");
                Resources["RatePriceText"] = Color.FromArgb("#D4AF37");
                Resources["RateDivider"] = Color.FromArgb("#1F180C");
            }

            RootLayout.BackgroundColor = (Color)Resources["AppBackground"];
        }

        private void UpdateSalihCategoryTabs()
        {
            var isLight = AppThemeManager.GetSavedTheme() == AslanThemeMode.Light;
            var selectedText = isLight ? Color.FromArgb("#FFFFFF") : Color.FromArgb("#101010");
            var inactiveText = isLight ? Color.FromArgb("#8B6D20") : Color.FromArgb("#8A8A8A");

            CategoryTabs.Background = isLight 
                ? CreateVerticalGradient("#FFFFFF", "#FAF6EA") 
                : CreateVerticalGradient("#151515", "#0D0D0D");
            CategoryTabs.Stroke = isLight ? Color.FromArgb("#C5A028") : Color.FromArgb("#7E6421");

            SarrafiyeTabCell.BackgroundColor = Colors.Transparent;
            MadenTabCell.BackgroundColor = Colors.Transparent;
            DovizTabCell.BackgroundColor = Colors.Transparent;
            SarrafiyeTabFrame.Background = viewModel.IsSarrafiyeSelected ? CreateGoldPillGradient() : new SolidColorBrush(Colors.Transparent);
            MadenTabFrame.Background = viewModel.IsMadenSelected ? CreateGoldPillGradient() : new SolidColorBrush(Colors.Transparent);
            DovizTabFrame.Background = viewModel.IsDovizSelected ? CreateGoldPillGradient() : new SolidColorBrush(Colors.Transparent);

            SarrafiyeTabLabel.TextColor = viewModel.IsSarrafiyeSelected ? selectedText : inactiveText;
            MadenTabLabel.TextColor = viewModel.IsMadenSelected ? selectedText : inactiveText;
            DovizTabLabel.TextColor = viewModel.IsDovizSelected ? selectedText : inactiveText;

            SarrafiyeTabIcon.Fill = viewModel.IsSarrafiyeSelected ? selectedText : inactiveText;
            MadenTabIcon.Fill = viewModel.IsMadenSelected ? selectedText : inactiveText;
            DovizTabIcon.Fill = viewModel.IsDovizSelected ? selectedText : inactiveText;

            SarrafiyeTabIndicator.IsVisible = false;
            MadenTabIndicator.IsVisible = false;
            DovizTabIndicator.IsVisible = false;

            TableCategoryHeaderLabel.Text = "Ürün";
            TableBuyHeaderLabel.Text = "Alış";
            TableSellHeaderLabel.Text = "Satış";

            // Dynamic backgrounds for TableHeader and RatesTableBorder
            TableHeader.Background = isLight 
                ? CreateVerticalGradient("#C5A028", "#9A7417") 
                : CreateVerticalGradient("#1C1710", "#0F0F0F");

            RatesTableBorder.Background = isLight 
                ? new SolidColorBrush(Color.FromArgb("#FFFFFF")) 
                : CreateVerticalGradient("#101010", "#070707");
        }

        private void ApplySalihResponsiveLayout(bool isLandscape)
        {
            ContentGrid.Padding = isLandscape ? new Thickness(8, 2, 8, 2) : new Thickness(0, 0, 0, 4);
            
            // Apply responsive layout for the active HeaderCard
            SalihHeaderCard.IsVisible = false;
            HeaderCard.IsVisible = true;
            HeaderCard.Padding = isLandscape ? new Thickness(10, 6, 6, 6) : new Thickness(8, 6, 6, 6);
            HeaderCard.HeightRequest = isLandscape ? 60 : 72;
            HeaderContent.RowSpacing = 0;
            HeaderTopGrid.HeightRequest = isLandscape ? 44 : 56;
            
            // Adjust column definitions of HeaderTopGrid dynamically:
            // Column 0: 0 (hamburger menu is removed for this brand)
            // Column 1: Auto (to fit the logo and full SİNAN ÇETİNKAYA brand name stack)
            // Column 2: * (takes the remaining space for Last Update info)
            // Column 3: 50 / 46 (shows the theme toggle button on the right side)
            HeaderTopGrid.ColumnDefinitions[0].Width = new GridLength(0);
            HeaderTopGrid.ColumnDefinitions[1].Width = GridLength.Auto;
            HeaderTopGrid.ColumnDefinitions[2].Width = GridLength.Star;
            HeaderTopGrid.ColumnDefinitions[3].Width = new GridLength(isLandscape ? 46 : 50);
            HeaderTopGrid.ColumnSpacing = isLandscape ? 5 : 6;
            
            BrandLogoFrame.WidthRequest = isLandscape ? 32 : 40;
            BrandLogoFrame.HeightRequest = isLandscape ? 32 : 40;
            BrandLogoImage.WidthRequest = isLandscape ? 32 : 40;
            BrandLogoImage.HeightRequest = isLandscape ? 32 : 40;
            
            BrandTextStack.WidthRequest = -1; // Allow the text container to expand automatically to fit the text length
            BrandTextStack.Spacing = isLandscape ? -2 : -3;
            
            // Resizing brand text programmatically to fit cleanly in portrait and landscape
            BrandAslanLabel.FontSize = isLandscape ? 14 : 16;
            BrandKuyumculukLabel.FontSize = isLandscape ? 9 : 11;

            // Responsive dimensions for the ThemeToggle button
            ThemeToggle.WidthRequest = isLandscape ? 38 : 42;
            ThemeToggle.HeightRequest = isLandscape ? 38 : 42;
            ThemeToggleIcon.FontSize = isLandscape ? 21 : 23;

            // Center the Last Update text container and labels in the middle column
            LastUpdateContainer.HorizontalOptions = LayoutOptions.Center;
            LastUpdateTitleLabel.HorizontalTextAlignment = TextAlignment.Center;
            LastUpdateValueLabel.HorizontalTextAlignment = TextAlignment.Center;

            SalihHeaderCard.HeightRequest = isLandscape ? 86 : 120;
            CategoryTabs.HeightRequest = isLandscape ? 38 : 46;
            CategoryTabs.Margin = isLandscape ? new Thickness(8, 4, 8, 4) : new Thickness(12, 4, 12, 6);
            TableHeader.Padding = isLandscape ? new Thickness(16, 6) : new Thickness(16, 8);
            TickerViewport.HeightRequest = isLandscape ? 40 : 54;
            SalihBottomNavigationContainer.HeightRequest = isLandscape ? 0 : 68;
            SalihBottomNavigationContainer.IsVisible = !isLandscape;

            Resources["RateRowHeight"] = isLandscape ? 38d : 46d;
            Resources["RateRowPadding"] = isLandscape ? new Thickness(12, 1) : new Thickness(12, 2);

            RefreshRateRows();
            UpdateTickerWidth();
            UpdateSalihCategoryTabs();
        }

        private IReadOnlyList<LiveRateItem> GetSalihTickerRates()
        {
            var allRates = viewModel.Sarrafiye
                .Concat(viewModel.Maden)
                .Concat(viewModel.Doviz)
                .ToList();

            var preferred = new[]
            {
                FindRate(allRates, "YENI ATA", "YENİ ATA", "ATA"),
                FindRate(allRates, "CEYREK (YENI)", "ÇEYREK (YENİ)", "YENİ ÇEYREK", "CEYREK", "ÇEYREK"),
                FindRate(allRates, "GRAM ALTIN", "G24 1gr", "GRAM", "G24")
            }
            .Where(item => item is not null)
            .Cast<LiveRateItem>()
            .DistinctBy(item => $"{item.Name}|{item.Code}")
            .Take(3)
            .ToList();

            if (preferred.Count >= 3)
            {
                return preferred;
            }

            foreach (var item in viewModel.TickerRates)
            {
                if (preferred.Count >= 3)
                {
                    break;
                }

                if (preferred.Any(existing => existing.Name == item.Name && existing.Code == item.Code))
                {
                    continue;
                }

                preferred.Add(item);
            }

            return preferred;
        }

        private static LiveRateItem? FindRate(IEnumerable<LiveRateItem> rates, params string[] tokens)
        {
            return rates.FirstOrDefault(rate =>
                tokens.Any(token =>
                    rate.Name.Contains(token, StringComparison.OrdinalIgnoreCase)
                    || (rate.Code?.Contains(token, StringComparison.OrdinalIgnoreCase) ?? false)));
        }

        private IReadOnlyList<LiveRateItem> GetAllSalihRates()
        {
            return viewModel.Sarrafiye
                .Concat(viewModel.Maden)
                .Concat(viewModel.Doviz)
                .DistinctBy(item => $"{item.Name}|{item.Code}")
                .ToList();
        }

        private HorizontalStackLayout CreateSalihScrollTickerItem(LiveRateItem item)
        {
            var isLight = AppThemeManager.GetSavedTheme() == AslanThemeMode.Light;
            var tickerTextColor = isLight ? Color.FromArgb("#2F2A1D") : Color.FromArgb("#FFFFFF");
            var goldColor = isLight ? Color.FromArgb("#9A7417") : Color.FromArgb("#F1D16A");
            
            var change = item.ChangePercent;
            var isDown = change.TrimStart().StartsWith("-", StringComparison.Ordinal) || !item.IsUp;
            var percentColor = isDown
                ? (isLight ? Color.FromArgb("#D84555") : Color.FromArgb("#FF4F5E"))
                : (isLight ? Color.FromArgb("#1FA463") : Color.FromArgb("#31C278"));

            var isFavorite = viewModel.IsFavorite(item);
            var displayName = isFavorite ? $"★ {item.Name.ToUpperInvariant()}" : item.Name.ToUpperInvariant();

            var layout = new HorizontalStackLayout
            {
                Spacing = 5,
                VerticalOptions = LayoutOptions.Center
            };

            // Product Name
            layout.Add(new Label
            {
                Text = displayName,
                FontAttributes = FontAttributes.Bold,
                TextColor = goldColor,
                FontSize = 8.5,
                FontAutoScalingEnabled = false,
                VerticalTextAlignment = TextAlignment.Center
            });

            // Buy Value
            layout.Add(new Label
            {
                Text = item.Buy,
                TextColor = tickerTextColor,
                FontSize = 8.5,
                FontAttributes = FontAttributes.Bold,
                FontAutoScalingEnabled = false,
                VerticalTextAlignment = TextAlignment.Center
            });

            // Slash Divider
            layout.Add(new Label
            {
                Text = "/",
                TextColor = isLight ? Color.FromArgb("#7C6010") : Color.FromArgb("#A28834"),
                FontSize = 8.5,
                FontAttributes = FontAttributes.Bold,
                FontAutoScalingEnabled = false,
                VerticalTextAlignment = TextAlignment.Center
            });

            // Sell Value
            layout.Add(new Label
            {
                Text = item.Sell,
                TextColor = tickerTextColor,
                FontSize = 8.5,
                FontAttributes = FontAttributes.Bold,
                FontAutoScalingEnabled = false,
                VerticalTextAlignment = TextAlignment.Center
            });

            // Change Percent with arrow
            var arrow = isDown ? "↓" : "↑";
            layout.Add(new Label
            {
                Text = $"{change} {arrow}",
                TextColor = percentColor,
                FontSize = 8,
                FontAttributes = FontAttributes.Bold,
                FontAutoScalingEnabled = false,
                VerticalTextAlignment = TextAlignment.Center
            });

            return layout;
        }

        private void UpdateSalihTickerIcons()
        {
            SalihTickerLeftIconContainer.Children.Clear();
            SalihTickerRightIconContainer.Children.Clear();

            var isLight = AppThemeManager.GetSavedTheme() == AslanThemeMode.Light;
            var iconColor = isLight ? Color.FromArgb("#9A7417") : Color.FromArgb("#F3C94D");

            // Left icon (signal/bar chart)
            var leftIcon = new HorizontalStackLayout
            {
                Spacing = 2.5,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            };
            leftIcon.Children.Add(new BoxView { WidthRequest = 3, HeightRequest = 7, Color = iconColor, VerticalOptions = LayoutOptions.End });
            leftIcon.Children.Add(new BoxView { WidthRequest = 3, HeightRequest = 12, Color = iconColor, VerticalOptions = LayoutOptions.End });
            leftIcon.Children.Add(new BoxView { WidthRequest = 3, HeightRequest = 17, Color = iconColor, VerticalOptions = LayoutOptions.End });
            SalihTickerLeftIconContainer.Children.Add(leftIcon);

            // Right icon (trend path)
            var rightIcon = new Grid
            {
                HeightRequest = 14,
                WidthRequest = 16,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            // Trend line
            rightIcon.Children.Add(new Microsoft.Maui.Controls.Shapes.Polyline
            {
                Stroke = iconColor,
                StrokeThickness = 2,
                StrokeLineCap = Microsoft.Maui.Controls.Shapes.PenLineCap.Round,
                StrokeLineJoin = Microsoft.Maui.Controls.Shapes.PenLineJoin.Round,
                Points = new Microsoft.Maui.Controls.PointCollection
                {
                    new Point(0, 12),
                    new Point(4.5, 6),
                    new Point(9, 9),
                    new Point(14, 2)
                }
            });

            // Arrow head
            rightIcon.Children.Add(new Microsoft.Maui.Controls.Shapes.Polyline
            {
                Stroke = iconColor,
                StrokeThickness = 2,
                StrokeLineCap = Microsoft.Maui.Controls.Shapes.PenLineCap.Round,
                StrokeLineJoin = Microsoft.Maui.Controls.Shapes.PenLineJoin.Round,
                Points = new Microsoft.Maui.Controls.PointCollection
                {
                    new Point(9.5, 2),
                    new Point(14, 2),
                    new Point(14, 6.5)
                }
            });

            SalihTickerRightIconContainer.Children.Add(rightIcon);
        }

        private void ScrollSalihTicker()
        {
            var segmentWidth = SalihTickerSegment.Width + SalihTickerScrollingContent.Spacing;
            if (segmentWidth <= 0)
            {
                return;
            }

            if (isTickerDragging)
            {
                return;
            }

            SalihTickerScrollingContent.TranslationX = NormalizeSalihTickerX(SalihTickerScrollingContent.TranslationX - TickerStep);
        }

        private double NormalizeSalihTickerX(double x)
        {
            var segmentWidth = SalihTickerSegment.Width + SalihTickerScrollingContent.Spacing;
            if (segmentWidth <= 0) return 0;
            while (x <= -segmentWidth)
            {
                x += segmentWidth;
            }
            while (x > 0)
            {
                x -= segmentWidth;
            }
            return x;
        }

        private static LinearGradientBrush CreateGoldPillGradient()
        {
            return new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
                GradientStops =
                {
                    new GradientStop(Color.FromArgb("#F3C94D"), 0),
                    new GradientStop(Color.FromArgb("#D4AF37"), 0.58f),
                    new GradientStop(Color.FromArgb("#B98620"), 1)
                }
            };
        }

        private static LinearGradientBrush CreateVerticalGradient(string top, string bottom)
        {
            return new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1),
                GradientStops =
                {
                    new GradientStop(Color.FromArgb(top), 0),
                    new GradientStop(Color.FromArgb(bottom), 1)
                }
            };
        }

        private static Shadow CreateSoftGoldShadow()
        {
            return new Shadow
            {
                Brush = new SolidColorBrush(Color.FromRgba(212, 175, 55, 0.24f)),
                Offset = new Point(0, 6),
                Radius = 12,
                Opacity = 0.65f
            };
        }

        private sealed class SalihHeaderDrawable : IDrawable
        {
            public void Draw(ICanvas canvas, RectF dirtyRect)
            {
                var width = dirtyRect.Width;
                var height = dirtyRect.Height;
                if (width <= 0 || height <= 0)
                {
                    return;
                }

                var isLight = AppThemeManager.GetSavedTheme() == AslanThemeMode.Light;

                canvas.SaveState();
                canvas.FillColor = AppThemeManager.GetColor("AppBackground", "#070707");
                canvas.FillRectangle(dirtyRect);

                // Thin bottom border line
                canvas.StrokeSize = 1.0f;
                canvas.StrokeColor = isLight ? Color.FromRgba(197, 160, 40, 0.22f) : Color.FromRgba(212, 175, 55, 0.16f);
                canvas.DrawLine(0, height - 1, width, height - 1);

                // Very subtle small diamonds on the sides
                canvas.StrokeColor = isLight ? Color.FromRgba(197, 160, 40, 0.08f) : Color.FromRgba(212, 175, 55, 0.05f);
                for (var y = 20f; y < height - 20; y += 32f)
                {
                    DrawSmallDiamond(canvas, 12, y);
                    DrawSmallDiamond(canvas, width - 12, y + 10);
                }

                canvas.RestoreState();

                static void DrawSmallDiamond(ICanvas canvas, float x, float y)
                {
                    var path = new PathF();
                    path.MoveTo(x, y - 3);
                    path.LineTo(x + 3, y);
                    path.LineTo(x, y + 3);
                    path.LineTo(x - 3, y);
                    path.Close();
                    canvas.DrawPath(path);
                }
            }
        }

        private void ApplyBrandTextSizing(bool isLandscape)
        {
            var primaryLength = brandSettings.PrimaryName.Length;

            if (primaryLength > 14)
            {
                BrandAslanLabel.FontSize = isLandscape ? 10 : 11;
                BrandKuyumculukLabel.FontSize = isLandscape ? 10 : 11;
                BrandTextStack.Spacing = 0;
                return;
            }

            if (primaryLength > 8)
            {
                BrandAslanLabel.FontSize = isLandscape ? 15 : 16;
                BrandKuyumculukLabel.FontSize = isLandscape ? 11 : 12;
                BrandTextStack.Spacing = -2;
                return;
            }

            BrandAslanLabel.FontSize = isLandscape ? 23 : 27;
            BrandKuyumculukLabel.FontSize = isLandscape ? 12 : 13;
            BrandTextStack.Spacing = -4;
        }

        private void OnFlyoutMenuButtonClicked(object? sender, EventArgs e)
        {
            if (Shell.Current is not null)
            {
                Shell.Current.FlyoutIsPresented = true;
            }
        }

        private void UpdateThemeSelector(AslanThemeMode mode)
        {
            var isLight = mode == AslanThemeMode.Light;

            if (isSalihBrand)
            {
                ThemeToggle.BackgroundColor = isLight ? Color.FromArgb("#FFFFFF") : Color.FromArgb("#121212");
                ThemeToggle.Stroke = GetThemeColor("AppBorder", "#D4AF37");
                ThemeToggle.StrokeThickness = 1;
                ThemeToggleIcon.Text = isLight ? "☀" : "☾";
                ThemeToggleIcon.TextColor = isLight ? Color.FromArgb("#F59E0B") : Color.FromArgb("#D4AF37");
                ThemeToggle.Shadow = null;
                return;
            }

            ThemeToggle.BackgroundColor = isLight ? Color.FromArgb("#FFFFFF") : Color.FromArgb("#1D2838");
            ThemeToggle.StrokeThickness = 0;
            ThemeToggle.Shadow = new Shadow
            {
                Brush = new SolidColorBrush(Color.FromArgb(isLight ? "#DDE3EA" : "#121A28")),
                Offset = new Point(0, 8),
                Radius = 16,
                Opacity = 0.45f
            };
            ThemeToggleIcon.Text = isLight ? "☀" : "☾";
            ThemeToggleIcon.TextColor = isLight ? Color.FromArgb("#F59E0B") : Color.FromArgb("#6DA8FF");
        }

        private static Color GetThemeColor(string key, string fallback)
        {
            return AppThemeManager.GetColor(key, fallback);
        }

        private static Label CreateTickerLabel(string text, Color color)
        {
            return new Label
            {
                Text = text,
                FontSize = 15,
                FontAttributes = FontAttributes.Bold,
                FontAutoScalingEnabled = false,
                TextColor = color,
                LineBreakMode = LineBreakMode.NoWrap,
                MaxLines = 1,
                VerticalTextAlignment = TextAlignment.Center,
                VerticalOptions = LayoutOptions.Center
            };
        }

        private void OnTrendLabelLoaded(object? sender, EventArgs e)
        {
            if (sender is not Label label)
            {
                return;
            }

            label.AbortAnimation("TrendBlink");
            label.Opacity = 1;

            var animation = new Animation();
            animation.Add(0, 0.5, new Animation(value => label.Opacity = value, 1, 0.35, Easing.CubicInOut));
            animation.Add(0.5, 1, new Animation(value => label.Opacity = value, 0.35, 1, Easing.CubicInOut));
            animation.Commit(label, "TrendBlink", length: 900, repeat: () => true);
        }

        private void OnTrendLabelUnloaded(object? sender, EventArgs e)
        {
            if (sender is not Label label)
            {
                return;
            }

            label.AbortAnimation("TrendBlink");
            label.Opacity = 1;
        }

        private void UpdateTickerWidth()
        {
            if (isSalihBrand)
            {
                var width = Math.Max(320, TickerViewport.Width - 32);
                SalihTickerBorder.WidthRequest = width;
                return;
            }

            if (string.IsNullOrWhiteSpace(viewModel.TickerText))
            {
                return;
            }

            TickerContent.WidthRequest = Math.Max(
                TickerViewport.Width,
                (viewModel.TickerText.Length * TickerEstimatedCharacterWidth + TickerHorizontalPadding) * 2 + TickerContent.Spacing);
        }

        private double NormalizeTickerX(double x)
        {
            var segmentWidth = TickerSegment.Width + TickerContent.Spacing;

            if (segmentWidth <= 0)
            {
                return x;
            }

            while (x <= -segmentWidth)
            {
                x += segmentWidth;
            }

            while (x > 0)
            {
                x -= segmentWidth;
            }

            return x;
        }

        private void OnAppSleeping(object? sender, EventArgs e)
        {
            StopLiveActivity();
        }

        private async void OnAppResumed(object? sender, EventArgs e)
        {
            StartTicker();
            StartLiveRatesRefresh();
            await RefreshLiveRatesAsync(updateTicker: false, showBusy: false);
        }
    }
}
