using System.ComponentModel;
using KuyumHesapFiyatEkran.Core.Features.LiveRates.Queries.GetLiveRates;
using KuyumHesapFiyatEkran.UI.Services;
using KuyumHesapFiyatEkran.UI.ViewModels;

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
        private IDispatcherTimer? tickerTimer;
        private IDispatcherTimer? liveRatesTimer;
        private CancellationTokenSource? refreshCancellationTokenSource;
        private bool isRefreshInProgress;
        private bool isTickerDragging;
        private bool isThemeChanging;
        private bool isLandscapeLayout;
        private double tickerDragStartX;
        private bool disposed;

        public MainPage(MainPageViewModel viewModel, BrandSettings brandSettings)
        {
            InitializeComponent();
            this.viewModel = viewModel;
            this.brandSettings = brandSettings;
            ApplyBranding();
            ApplyTheme(AppThemeManager.GetSavedTheme(), save: false);
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
            TickerSegmentContent.Children.Clear();
            TickerSegmentCloneContent.Children.Clear();

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
                await Task.WhenAll(
                    ThemeToggle.ScaleTo(0.9, 80, Easing.CubicOut),
                    ThemeToggleIcon.RotateTo(18, 80, Easing.CubicOut),
                    RootLayout.FadeTo(0.82, 110, Easing.CubicOut));

                ApplyTheme(nextMode);

                ThemeToggleIcon.Rotation = -18;
                await Task.WhenAll(
                    ThemeToggle.ScaleTo(1, 150, Easing.SpringOut),
                    ThemeToggleIcon.RotateTo(0, 170, Easing.CubicInOut),
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
            DovizTabColumn.Width = viewModel.IsDovizEnabled ? GridLength.Star : new GridLength(0);

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

            ContentGrid.Padding = isLandscape ? new Thickness(8, 4, 8, 6) : new Thickness(4, 8, 4, 18);
            HeaderCard.Padding = isLandscape ? new Thickness(10, 6, 6, 6) : new Thickness(8, 6, 6, 6);
            HeaderCard.HeightRequest = isLandscape ? 60 : 72;
            HeaderContent.RowSpacing = 0;
            HeaderTopGrid.HeightRequest = isLandscape ? 44 : 56;
            HeaderTopGrid.ColumnDefinitions[0].Width = new GridLength(isLandscape ? 32 : 34);
            HeaderTopGrid.ColumnDefinitions[1].Width = new GridLength(isLandscape ? 116 : 146);
            HeaderTopGrid.ColumnDefinitions[3].Width = new GridLength(isLandscape ? 46 : 50);
            HeaderTopGrid.ColumnSpacing = isLandscape ? 5 : 6;
            FlyoutMenuButton.WidthRequest = isLandscape ? 32 : 34;
            FlyoutMenuButton.HeightRequest = isLandscape ? 34 : 44;
            FlyoutMenuButton.FontSize = isLandscape ? 20 : 24;
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

            RefreshRateRows();
            UpdateTickerWidth();
        }

        private void ApplyBranding()
        {
            BrandAslanLabel.Text = brandSettings.PrimaryName;
            BrandKuyumculukLabel.Text = brandSettings.SecondaryName;
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

            ThemeToggle.BackgroundColor = isLight ? Color.FromArgb("#FFFFFF") : Color.FromArgb("#1D2838");
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
