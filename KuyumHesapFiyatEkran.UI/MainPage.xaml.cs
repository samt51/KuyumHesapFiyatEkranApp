using System.ComponentModel;
using KuyumHesapFiyatEkran.Core.Features.LiveRates.Queries.GetLiveRates;
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
        private IDispatcherTimer? tickerTimer;
        private IDispatcherTimer? liveRatesTimer;
        private CancellationTokenSource? refreshCancellationTokenSource;
        private bool isRefreshInProgress;
        private bool isTickerDragging;
        private double tickerDragStartX;
        private bool disposed;

        public MainPage(MainPageViewModel viewModel)
        {
            InitializeComponent();
            this.viewModel = viewModel;
            BindingContext = viewModel;
            viewModel.PropertyChanged += OnViewModelPropertyChanged;
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
                ? Color.FromArgb("#E34F61")
                : Color.FromArgb("#26BF72");

            var layout = new HorizontalStackLayout
            {
                Spacing = 4,
                VerticalOptions = LayoutOptions.Center
            };

            layout.Add(CreateTickerLabel(item.Name, Colors.White));
            layout.Add(CreateTickerLabel("ALIŞ:", Colors.White));
            layout.Add(CreateTickerLabel(item.Buy.ToString("N2"), Colors.White));
            layout.Add(CreateTickerLabel(item.ChangePercent, percentColor));
            layout.Add(CreateTickerLabel("SATIŞ:", Colors.White));
            layout.Add(CreateTickerLabel(item.Sell.ToString("N2"), Colors.White));
            layout.Add(CreateTickerLabel("|", Colors.White));

            return layout;
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
