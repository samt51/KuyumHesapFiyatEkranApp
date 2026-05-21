using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using KuyumHesapFiyatEkran.Core.Features.LiveRates.Queries.GetLiveRates;
using MediatR;

namespace KuyumHesapFiyatEkran.UI.ViewModels;

public sealed class MainPageViewModel : INotifyPropertyChanged
{
    private readonly IMediator mediator;
    private bool isBusy;
    private bool isInitialLoading = true;
    private string lastUpdate = string.Empty;
    private string? errorMessage;
    private string selectedCategory = "Sarrafiye";
    private CancellationTokenSource? tickerSwitchCancellationTokenSource;

    public MainPageViewModel(IMediator mediator)
    {
        this.mediator = mediator;
        RefreshCommand = new Command(async () => await LoadAsync(CancellationToken.None, updateTicker: true, showBusy: true), () => !IsBusy);
        SelectCategoryCommand = new Command<string>(category => _ = SelectCategoryAsync(category));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ICommand RefreshCommand { get; }

    public ICommand SelectCategoryCommand { get; }

    public ObservableCollection<LiveRateItem> Sarrafiye { get; } = new();

    public ObservableCollection<LiveRateItem> Maden { get; } = new();

    public ObservableCollection<LiveRateItem> Doviz { get; } = new();

    public ObservableCollection<LiveRateItem> DisplayRates { get; } = new();

    public ObservableCollection<LiveRateItem> TickerRates { get; } = new();

    private string tickerText = string.Empty;

    public string TickerText
    {
        get => tickerText;
        private set => SetProperty(ref tickerText, value);
    }

    public bool IsBusy
    {
        get => isBusy;
        private set
        {
            if (SetProperty(ref isBusy, value))
            {
                ((Command)RefreshCommand).ChangeCanExecute();
            }
        }
    }

    public bool IsInitialLoading
    {
        get => isInitialLoading;
        private set
        {
            if (SetProperty(ref isInitialLoading, value))
            {
                OnPropertyChanged(nameof(IsTickerVisible));
            }
        }
    }

    public bool IsTickerVisible => !IsInitialLoading;

    public string LastUpdate
    {
        get => lastUpdate;
        private set => SetProperty(ref lastUpdate, value);
    }

    public string? ErrorMessage
    {
        get => errorMessage;
        private set => SetProperty(ref errorMessage, value);
    }

    public string SelectedCategory
    {
        get => selectedCategory;
        private set
        {
            if (SetProperty(ref selectedCategory, value))
            {
                OnPropertyChanged(nameof(IsSarrafiyeSelected));
                OnPropertyChanged(nameof(IsMadenSelected));
                OnPropertyChanged(nameof(IsDovizSelected));
                OnPropertyChanged(nameof(SelectedCategoryTitle));
                RefreshDisplayRates();
            }
        }
    }

    public bool IsSarrafiyeSelected => SelectedCategory == "Sarrafiye";

    public bool IsMadenSelected => SelectedCategory == "Maden";

    public bool IsDovizSelected => SelectedCategory == "Doviz";

    public bool IsSarrafiyeEnabled => Sarrafiye.Count > 0;

    public bool IsMadenEnabled => Maden.Count > 0;

    public bool IsDovizEnabled => Doviz.Count > 0;

    public string SelectedCategoryTitle => SelectedCategory switch
    {
        "Maden" => "MADEN",
        "Doviz" => "D\u00d6V\u0130Z",
        "Sarrafiye" => "SARRAF\u0130YE",
        _ => "KURLAR"
    };

    public async Task LoadAsync(CancellationToken cancellationToken, bool updateTicker, bool showBusy)
    {
        if (showBusy && IsBusy)
        {
            return;
        }

        try
        {
            if (showBusy)
            {
                IsBusy = true;
            }

            ErrorMessage = null;

            var response = await mediator.Send(new GetLiveRatesQuery(), cancellationToken);

            if (!response.IsSuccess || response.Data is null)
            {
                ErrorMessage = response.Errors?.Count > 0
                    ? string.Join(Environment.NewLine, response.Errors)
                    : "Canli kur bilgileri alinamadi.";
                return;
            }

            LastUpdate = response.Data.LastUpdate;
            SyncItems(Sarrafiye, response.Data.Sarrafiye ?? Enumerable.Empty<LiveRateItem>());
            SyncItems(Maden, response.Data.Maden ?? Enumerable.Empty<LiveRateItem>());
            SyncItems(Doviz, response.Data.Doviz ?? Enumerable.Empty<LiveRateItem>());
            RefreshCategoryAvailability();
            EnsureSelectedCategoryAvailable();
            RefreshDisplayRates();
            if (updateTicker || string.IsNullOrWhiteSpace(TickerText))
            {
                RefreshTickerText();
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            if (showBusy)
            {
                IsBusy = false;
            }

            IsInitialLoading = false;
        }
    }

    private static void SyncItems(ObservableCollection<LiveRateItem> target, IEnumerable<LiveRateItem> items)
    {
        var source = items.ToList();

        for (var index = 0; index < source.Count; index++)
        {
            source[index].IsAlternateRow = index % 2 == 1;

            if (index >= target.Count)
            {
                target.Add(source[index]);
                continue;
            }

            if (!AreSame(target[index], source[index]))
            {
                target[index] = source[index];
            }
        }

        while (target.Count > source.Count)
        {
            target.RemoveAt(target.Count - 1);
        }
    }

    private async Task SelectCategoryAsync(string? category)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            return;
        }

        if (!IsCategoryAvailable(category))
        {
            return;
        }

        SelectedCategory = category;
        tickerSwitchCancellationTokenSource?.Cancel();
        tickerSwitchCancellationTokenSource?.Dispose();
        tickerSwitchCancellationTokenSource = new CancellationTokenSource();

        try
        {
            await Task.Delay(120, tickerSwitchCancellationTokenSource.Token);
            RefreshTickerText();
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void RefreshDisplayRates()
    {
        SyncItems(DisplayRates, GetSelectedRates());
    }

    private void RefreshTickerText()
    {
        SyncItems(TickerRates, GetSelectedRates());
        TickerText = string.Join("    |    ", GetSelectedRates()
            .Select(item => $"{item.Name}  ALI\u015e: {item.Buy} {item.ChangePercent}  SATI\u015e: {item.Sell}"));
    }

    private IEnumerable<LiveRateItem> GetSelectedRates()
    {
        return SelectedCategory switch
        {
            "Maden" => Maden,
            "Doviz" => Doviz,
            "Sarrafiye" => Sarrafiye,
            _ => Enumerable.Empty<LiveRateItem>()
        };
    }

    private void EnsureSelectedCategoryAvailable()
    {
        if (IsCategoryAvailable(SelectedCategory))
        {
            return;
        }

        SelectedCategory = GetFirstAvailableCategory();
    }

    private string GetFirstAvailableCategory()
    {
        if (IsSarrafiyeEnabled)
        {
            return "Sarrafiye";
        }

        if (IsMadenEnabled)
        {
            return "Maden";
        }

        return IsDovizEnabled ? "Doviz" : string.Empty;
    }

    private bool IsCategoryAvailable(string category)
    {
        return category switch
        {
            "Sarrafiye" => IsSarrafiyeEnabled,
            "Maden" => IsMadenEnabled,
            "Doviz" => IsDovizEnabled,
            _ => false
        };
    }

    private void RefreshCategoryAvailability()
    {
        OnPropertyChanged(nameof(IsSarrafiyeEnabled));
        OnPropertyChanged(nameof(IsMadenEnabled));
        OnPropertyChanged(nameof(IsDovizEnabled));
    }

    private static bool AreSame(LiveRateItem left, LiveRateItem right)
    {
        return left.Name == right.Name
            && left.Code == right.Code
            && left.Buy == right.Buy
            && left.Sell == right.Sell
            && left.Description == right.Description
            && left.ChangePercent == right.ChangePercent
            && left.IsUp == right.IsUp;
    }

    private bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value))
        {
            return false;
        }

        backingStore = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
