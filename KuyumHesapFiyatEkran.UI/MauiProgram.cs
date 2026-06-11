using System.Reflection;
using KuyumHesapFiyatEkran.Core;
using KuyumHesapFiyatEkran.UI.Pages;
using KuyumHesapFiyatEkran.UI.Services;
using KuyumHesapFiyatEkran.UI.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KuyumHesapFiyatEkran.UI
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            AddAppSettings(builder.Configuration);
            builder.Services.AddCoreServices(builder.Configuration);
            builder.Services.AddSingleton<LiveRatesStore>();
            builder.Services.AddSingleton<AppShell>();
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<GoldCalculatorPage>();
            builder.Services.AddTransient<FavoritesPage>();
            builder.Services.AddTransient<ContactPage>();
            builder.Services.AddTransient<AboutPage>();
            builder.Services.AddTransient<MainPageViewModel>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

        private static void AddAppSettings(ConfigurationManager configuration)
        {
            var assembly = Assembly.GetExecutingAssembly();

            AddJsonResource(configuration, assembly, "KuyumHesapFiyatEkran.UI.appsettings.json", required: true);
            AddJsonResource(configuration, assembly, "KuyumHesapFiyatEkran.UI.appsettings.Production.json", required: false);
        }

        private static void AddJsonResource(ConfigurationManager configuration, Assembly assembly, string resourceName, bool required)
        {
            var stream = assembly.GetManifestResourceStream(resourceName);

            if (stream is null)
            {
                if (required)
                {
                    throw new InvalidOperationException($"{resourceName} embedded resource olarak bulunamadi.");
                }

                return;
            }

            configuration.AddJsonStream(stream);
        }
    }
}
