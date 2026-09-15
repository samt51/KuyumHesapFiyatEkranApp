namespace KuyumHesapFiyatEkran.UI
{
    public partial class App : Application
    {
        public static event EventHandler? AppSleeping;

        public static event EventHandler? AppResumed;

        public App(AppShell appShell)
        {
            InitializeComponent();
            AppThemeManager.ApplySelectedTheme(Resources);
            MainPage = appShell;
        }

        protected override void OnSleep()
        {
            AppSleeping?.Invoke(this, EventArgs.Empty);
            base.OnSleep();
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (AppThemeManager.GetThemeSelection() == ThemeSelection.Auto)
            {
                AppThemeManager.ApplySelectedTheme(Resources);
            }
            AppResumed?.Invoke(this, EventArgs.Empty);
        }
    }
}
