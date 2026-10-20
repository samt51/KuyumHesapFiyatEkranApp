using KuyumHesapFiyatEkran.UI.Services.Firebase;

namespace KuyumHesapFiyatEkran.UI.Brands.MazlumKuyumculuk.Pages;

public partial class MazlumBranchesPage : ContentPage
{
    private const string AdminEmail = "admin@mazlumkuyumculuk.com";

    private readonly IMazlumFirebaseService firebaseService;
    private readonly IMazlumFirebaseAuthService authService;

    private List<MazlumFirebaseBranch> branches = new();

    public MazlumBranchesPage(
        IMazlumFirebaseService firebaseService,
        IMazlumFirebaseAuthService authService)
    {
        InitializeComponent();

        this.firebaseService = firebaseService;
        this.authService = authService;
    }

    private bool IsAdmin =>
        authService.IsAuthenticated &&
        string.Equals(
            authService.UserEmail?.Trim(),
            AdminEmail,
            StringComparison.OrdinalIgnoreCase);

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        NavigationPage.SetHasNavigationBar(this, false);
        Shell.SetNavBarIsVisible(this, false);

        await authService.RestoreSessionAsync();

        AddButton.IsVisible = IsAdmin;

        UpdateSidebarAuthState();

        await LoadBranchesAsync();
    }

    private async Task LoadBranchesAsync()
    {
        try
        {
            SetLoading(true);
            HideError();

            branches = await firebaseService.GetBranchesAsync();

            RenderBranches();
        }
        catch (Exception ex)
        {
            ShowError($"Şubeler alınamadı: {ex.Message}");
        }
        finally
        {
            SetLoading(false);
        }
    }

    private void RenderBranches()
    {
        BranchesContainer.Children.Clear();

        EmptyLabel.IsVisible =
            branches.Count == 0 &&
            !BranchForm.IsVisible;

        EmptyLabel.Text = IsAdmin
            ? "Henüz şube yok. + ile ekleyebilirsiniz."
            : "Şube bilgisi yakında eklenecek.";

        foreach (var branch in branches)
        {
            BranchesContainer.Children.Add(
                CreateBranchCard(branch));
        }
    }

    private View CreateBranchCard(MazlumFirebaseBranch branch)
    {
        var content = new VerticalStackLayout
        {
            Spacing = 7
        };

        if (!string.IsNullOrWhiteSpace(branch.ImageUrl))
        {
            try
            {
                content.Children.Add(new Image
                {
                    Source = ImageSource.FromUri(
                        new Uri(branch.ImageUrl)),
                    HeightRequest = 170,
                    Aspect = Aspect.AspectFill
                });
            }
            catch
            {
                // data URL desteğini sonraki adımda ekleyeceğiz.
            }
        }

        content.Children.Add(new Label
        {
            Text = branch.Name,
            TextColor = Colors.White,
            FontSize = 17,
            FontAttributes = FontAttributes.Bold
        });

        if (!string.IsNullOrWhiteSpace(branch.Phone))
        {
            var phone = new Label
            {
                Text = branch.Phone,
                TextColor = Color.FromArgb("#C9AE20"),
                FontSize = 14
            };

            var phoneTap = new TapGestureRecognizer();
            phoneTap.Tapped += async (_, _) =>
            {
                try
                {
                    await Launcher.Default.OpenAsync(
                        new Uri($"tel:{NormalizePhone(branch.Phone)}"));
                }
                catch
                {
                    // cihaz desteklemiyorsa sessiz geç
                }
            };

            phone.GestureRecognizers.Add(phoneTap);
            content.Children.Add(phone);
        }

        if (!string.IsNullOrWhiteSpace(branch.Address))
        {
            content.Children.Add(new Label
            {
                Text = branch.Address,
                TextColor = Color.FromArgb("#A4A4A4"),
                FontSize = 13,
                LineBreakMode = LineBreakMode.WordWrap
            });
        }

        if (IsAdmin)
        {
            var buttons = new HorizontalStackLayout
            {
                Spacing = 12,
                Margin = new Thickness(0, 6, 0, 0)
            };

            var edit = new Button
            {
                Text = "Düzenle",
                TextColor = Color.FromArgb("#C9AE20"),
                BackgroundColor = Colors.Transparent,
                FontSize = 13,
                Padding = new Thickness(0)
            };

            edit.Clicked += (_, _) => StartEdit(branch);

            var delete = new Button
            {
                Text = "Sil",
                TextColor = Color.FromArgb("#E57373"),
                BackgroundColor = Colors.Transparent,
                FontSize = 13,
                Padding = new Thickness(0)
            };

            delete.Clicked += async (_, _) =>
            {
                await DisplayAlert(
                    "Şube",
                    "Firebase silme bağlantısını sonraki adımda bağlayacağız.",
                    "Tamam");
            };

            buttons.Children.Add(edit);
            buttons.Children.Add(delete);

            content.Children.Add(buttons);
        }

        return new Border
        {
            BackgroundColor = Color.FromArgb("#151614"),
            Stroke = Color.FromArgb("#35321D"),
            StrokeThickness = 1,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
            {
                CornerRadius = 13
            },
            Padding = 12,
            Content = content
        };
    }

    private void OnAddTapped(object sender, TappedEventArgs e)
    {
        if (!IsAdmin)
            return;

        NameEntry.Text = string.Empty;
        PhoneEntry.Text = string.Empty;
        AddressEditor.Text = string.Empty;

        ClearImage();

        BranchForm.IsVisible = true;
        EmptyLabel.IsVisible = false;
    }

    private void StartEdit(MazlumFirebaseBranch branch)
    {
        if (!IsAdmin)
            return;

        NameEntry.Text = branch.Name;
        PhoneEntry.Text = branch.Phone;
        AddressEditor.Text = branch.Address;

        BranchForm.IsVisible = true;
        EmptyLabel.IsVisible = false;

        // imageUrl/data URL gösterimini sonraki adımda tamamlayacağız.
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (!IsAdmin)
            return;

        if (string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            ShowError("Şube adı gerekli.");
            return;
        }

        await DisplayAlert(
            "Hazır",
            "Şube ekranı hazır. Firebase kayıt bağlantısını sonraki adımda bağlayacağız.",
            "Tamam");
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        BranchForm.IsVisible = false;
        HideError();
        RenderBranches();
    }

    private async void OnTakePhotoTapped(
        object sender,
        TappedEventArgs e)
    {
        await DisplayAlert(
            "Kamera",
            "Kamera ve 16:10 kırpma sonraki adımda bağlanacak.",
            "Tamam");
    }

    private async void OnGalleryTapped(
        object sender,
        TappedEventArgs e)
    {
        await DisplayAlert(
            "Galeri",
            "Galeri ve 16:10 kırpma sonraki adımda bağlanacak.",
            "Tamam");
    }

    private void OnRemoveImageTapped(
        object sender,
        TappedEventArgs e)
    {
        ClearImage();
    }

    private void ClearImage()
    {
        BranchImage.Source = null;
        BranchImage.IsVisible = false;

        ImagePlaceholder.IsVisible = true;
        RemoveImageButton.IsVisible = false;
    }

    private async void OnBackTapped(
        object sender,
        TappedEventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnMenuTapped(
        object sender,
        TappedEventArgs e)
    {
        await OpenSidebarAsync();
    }

    private async Task OpenSidebarAsync()
    {
        UpdateSidebarAuthState();

        SidebarOverlay.IsVisible = true;

        SidebarPanel.TranslationX = -340;

        await SidebarPanel.TranslateTo(
            0,
            0,
            220,
            Easing.CubicOut);
    }

    private async Task CloseSidebarAsync()
    {
        await SidebarPanel.TranslateTo(
            -340,
            0,
            180,
            Easing.CubicIn);

        SidebarOverlay.IsVisible = false;
    }

    private async void OnSidebarCloseClicked(
        object sender,
        EventArgs e)
    {
        await CloseSidebarAsync();
    }

    private async void OnSidebarBackdropTapped(
        object sender,
        TappedEventArgs e)
    {
        await CloseSidebarAsync();
    }

    private void OnSidebarPanelTapped(
        object sender,
        TappedEventArgs e)
    {
        // Panel içerisindeki tıklamalar overlay'i kapatmasın.
    }

    private void OnBranchCurrentPageTapped(
        object sender,
        TappedEventArgs e)
    {
        // Zaten Şubelerimiz sayfasındayız.
    }

    private async void OnBranchCalculationTapped(
        object sender,
        TappedEventArgs e)
    {
        await CloseSidebarAsync();

        var page =
            Handler?.MauiContext?.Services
                .GetService<MazlumCalculationPage>();

        if (page is not null)
        {
            await Navigation.PushAsync(page);
        }
    }

    private void UpdateSidebarAuthState()
    {
        var authenticated =
            authService.IsAuthenticated &&
            !string.IsNullOrWhiteSpace(
                authService.UserEmail);

        var email =
            authService.UserEmail?.Trim();

        BranchAuthUserLabel.Text =
            authenticated
                ? email
                : "Giriş Yap";

        LogoutMenuRow.IsVisible =
            authenticated;

        var isAdmin =
            authenticated &&
            string.Equals(
                email,
                "admin@mazlumkuyumculuk.com",
                StringComparison.OrdinalIgnoreCase);

        NotificationMenuRow.IsVisible =
            isAdmin;
    }

    private async void OnBranchAuthMenuTapped(
        object sender,
        TappedEventArgs e)
    {
        await CloseSidebarAsync();

        if (authService.IsAuthenticated)
        {
            return;
        }

        var page =
            Handler?.MauiContext?.Services
                .GetService<MazlumLoginChooserPage>();

        if (page is not null)
        {
            await Navigation.PushAsync(page);
        }
    }

    private async void OnLogoutTapped(
        object sender,
        TappedEventArgs e)
    {
        await CloseSidebarAsync();

        try
        {
            await authService.SignOutAsync();
            UpdateSidebarAuthState();

            if (Navigation.NavigationStack.Count > 1)
                await Navigation.PopToRootAsync();
        }
        catch
        {
            await DisplayAlert(
                "Hata",
                "Çıkış yapılamadı. Tekrar deneyin.",
                "Tamam");
        }
    }

    private void SetLoading(bool loading)
    {
        LoadingIndicator.IsVisible = loading;
        LoadingIndicator.IsRunning = loading;
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }

    private void HideError()
    {
        ErrorLabel.Text = string.Empty;
        ErrorLabel.IsVisible = false;
    }

    private static string NormalizePhone(string phone)
    {
        var digits = new string(
            phone.Where(char.IsDigit).ToArray());

        if (digits.StartsWith("90") && digits.Length >= 12)
            return $"+{digits}";

        if (digits.StartsWith("0") && digits.Length == 11)
            return $"+90{digits[1..]}";

        if (digits.Length == 10 && digits.StartsWith("5"))
            return $"+90{digits}";

        return $"+{digits}";
    }

    private async void OnAboutMenuTapped(
        object sender,
        TappedEventArgs e)
    {
        if (SidebarOverlay.IsVisible)
            await CloseSidebarAsync();

        var page =
            Handler?.MauiContext?.Services
                .GetService<MazlumAboutPage>();

        if (page is null)
        {
            await DisplayAlert(
                "Hata",
                "Hakkımızda sayfası açılamadı.",
                "Tamam");

            return;
        }

        await Navigation.PushAsync(page);
    }


    private async void OnLegalMenuTapped(
        object sender,
        TappedEventArgs e)
    {
        if (SidebarOverlay.IsVisible)
            await CloseSidebarAsync();

        var page =
            Handler?.MauiContext?.Services
                .GetService<MazlumLegalPage>();

        if (page is null)
        {
            await DisplayAlert(
                "Hata",
                "Yasal Uyarı sayfası açılamadı.",
                "Tamam");

            return;
        }

        await Navigation.PushAsync(page);
    }



    private async void OnCalculationMenuTapped(
        object? sender,
        TappedEventArgs e)
    {{
        await CloseSidebarAsync();

        var page =
            Handler?.MauiContext?.Services
                .GetService<MazlumCalculationPage>();

        if (page is not null)
            await Navigation.PushAsync(page);
    }}


    private async void OnBranchesMenuTapped(
        object? sender,
        TappedEventArgs e)
    {{
        await CloseSidebarAsync();
    }}


    private async void OnNotificationMenuTapped(
        object? sender,
        TappedEventArgs e)
    {{
        await CloseSidebarAsync();

        var isAdmin =
            authService.IsAuthenticated &&
            string.Equals(
                authService.UserEmail?.Trim(),
                AdminEmail,
                StringComparison.OrdinalIgnoreCase);

        if (!isAdmin)
            return;
        var page =
            Handler?.MauiContext?.Services
                .GetService<MazlumNotifyPage>();

        if (page is not null)
            await Navigation.PushAsync(page);
    }}


    private async void OnSettingsMenuTapped(
        object? sender,
        TappedEventArgs e)
    {{
        await CloseSidebarAsync();

        var page =
            Handler?.MauiContext?.Services
                .GetService<MazlumSettingsPage>();

        if (page is not null)
            await Navigation.PushAsync(page);
    }}

}
