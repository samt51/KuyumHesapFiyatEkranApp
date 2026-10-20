using KuyumHesapFiyatEkran.UI.Brands.MazlumKuyumculuk.Pages;
﻿using KuyumHesapFiyatEkran.UI.Pages;
using KuyumHesapFiyatEkran.UI.Services;

namespace KuyumHesapFiyatEkran.UI
{
    public partial class AppShell : Shell
    {
        public AppShell(
            MainPage mainPage,
            SalihHomePage salihHomePage,
            MazlumHomePage mazlumHomePage,
            GoldCalculatorPage goldCalculatorPage,
            FavoritesPage favoritesPage,
            ContactPage contactPage,
            AboutPage aboutPage,
            BrandSettings brandSettings)
        {
            InitializeComponent();
            ApplyBranding(brandSettings);
            DashboardContent.Content = salihHomePage;

            HomeContent.Content =
                string.Equals(
                    brandSettings.Key,
                    "Mazlum",
                    StringComparison.OrdinalIgnoreCase)
                    ? mazlumHomePage
                    : mainPage;

            GoldCalculatorContent.Content = goldCalculatorPage;
            FavoritesContent.Content = favoritesPage;
            ContactContent.Content = contactPage;
            AboutContent.Content = aboutPage;

            if (!string.Equals(brandSettings.Key, "SalihCetinkaya", StringComparison.OrdinalIgnoreCase))
            {
                Items.Remove(DashboardContent);
            }
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
