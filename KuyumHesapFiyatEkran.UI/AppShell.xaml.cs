using KuyumHesapFiyatEkran.UI.Pages;

namespace KuyumHesapFiyatEkran.UI
{
    public partial class AppShell : Shell
    {
        public AppShell(
            MainPage mainPage,
            GoldCalculatorPage goldCalculatorPage,
            FavoritesPage favoritesPage,
            ContactPage contactPage,
            AboutPage aboutPage)
        {
            InitializeComponent();
            HomeContent.Content = mainPage;
            GoldCalculatorContent.Content = goldCalculatorPage;
            FavoritesContent.Content = favoritesPage;
            ContactContent.Content = contactPage;
            AboutContent.Content = aboutPage;
        }
    }
}
