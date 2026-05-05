namespace KuyumHesapFiyatEkran.UI
{
    public partial class App : Application
    {
        public static event EventHandler? AppSleeping;

        public static event EventHandler? AppResumed;

        public App(AppShell appShell)
        {
            InitializeComponent();

            MainPage = new SplashPage();
            _ = ShowMainPageAsync(appShell);
        }

        private async Task ShowMainPageAsync(AppShell appShell)
        {
            await Task.Delay(1500);
            MainThread.BeginInvokeOnMainThread(() => MainPage = appShell);
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
