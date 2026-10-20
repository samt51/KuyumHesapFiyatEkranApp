namespace KuyumHesapFiyatEkran.UI.Brands.MazlumKuyumculuk.Pages;

public partial class MazlumLoginChooserPage : ContentPage
{
    private readonly IServiceProvider _services;

    public MazlumLoginChooserPage(IServiceProvider services)
    {
        InitializeComponent();
        _services = services;
    }

    private async void OnUserLoginTapped(
        object sender,
        TappedEventArgs e)
    {
        await OpenLoginAsync(false);
    }

    private async void OnAdminLoginTapped(
        object sender,
        TappedEventArgs e)
    {
        await OpenLoginAsync(true);
    }

    private async Task OpenLoginAsync(bool isAdmin)
    {
        var page = _services.GetService<MazlumLoginPage>();

        if (page is null)
        {
            await DisplayAlert(
                "Hata",
                "Giriş ekranı açılamadı.",
                "Tamam");

            return;
        }

        page.Configure(isAdmin);

        await Navigation.PushAsync(page);
    }

    private async void OnBackTapped(
        object sender,
        TappedEventArgs e)
    {
        await Navigation.PopAsync();
    }
}
