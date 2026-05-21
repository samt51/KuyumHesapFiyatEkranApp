namespace KuyumHesapFiyatEkran.UI
{
    public partial class App : Application
    {
        public static event EventHandler? AppSleeping;

        public static event EventHandler? AppResumed;

        public App(AppShell appShell)
        {
            InitializeComponent();
            AppThemeManager.Apply(AppThemeManager.GetSavedTheme(), Resources, save: false);
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
            AppResumed?.Invoke(this, EventArgs.Empty);
        }
    }
}
