namespace KuyumHesapFiyatEkran.UI
{
    public partial class SplashPage : ContentPage
    {
        public SplashPage()
        {
            InitializeComponent();
            ApplyBranding();
        }

        private void ApplyBranding()
        {
            var brand = Services.BusinessInfo.Current;
            var hasLogo = !string.IsNullOrWhiteSpace(brand.LogoImage);

            SplashLogoImage.IsVisible = hasLogo;
            SplashLogoFallback.IsVisible = !hasLogo;
            SplashLogoFallback.Text = brand.LogoMonogram;

            if (hasLogo)
            {
                SplashLogoImage.Source = brand.LogoImage;
            }
        }
    }
}
