using KuyumHesapFiyatEkran.UI.Pages;
using KuyumHesapFiyatEkran.UI.Services;

namespace KuyumHesapFiyatEkran.UI
{
    public partial class AppShell : Shell
    {
        public AppShell(
            MainPage mainPage,
            GoldCalculatorPage goldCalculatorPage,
            FavoritesPage favoritesPage,
            ContactPage contactPage,
            AboutPage aboutPage,
            BrandSettings brandSettings)
        {
            InitializeComponent();
            ApplyBranding(brandSettings);
            HomeContent.Content = mainPage;
            GoldCalculatorContent.Content = goldCalculatorPage;
            FavoritesContent.Content = favoritesPage;
            ContactContent.Content = contactPage;
            AboutContent.Content = aboutPage;
        }

        private void ApplyBranding(BrandSettings brandSettings)
        {
            Title = brandSettings.DisplayName;
            ShellRoot.Title = brandSettings.DisplayName;
            FlyoutBrandNameLabel.Text = brandSettings.DisplayName.ToUpperInvariant();
            FlyoutSubtitleLabel.Text = brandSettings.HeaderSubtitle;
            FlyoutLogoFallback.Text = brandSettings.LogoMonogram;

            var hasLogo = !string.IsNullOrWhiteSpace(brandSettings.LogoImage);
            FlyoutLogoImage.IsVisible = hasLogo;
            FlyoutLogoFallback.IsVisible = !hasLogo;

            if (hasLogo)
            {
                FlyoutLogoImage.Source = brandSettings.LogoImage;
            }
        }
    }
}
