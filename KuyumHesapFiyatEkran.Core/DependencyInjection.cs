using KuyumHesapFiyatEkran.Core.Abstractions.Http;
using KuyumHesapFiyatEkran.Core.Configuration;
using KuyumHesapFiyatEkran.Core.Services.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace KuyumHesapFiyatEkran.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ApiSettings>(configuration.GetSection(ApiSettings.SectionName));

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
        });

        services.AddHttpClient<IBackendApiClient, BackendApiClient>((serviceProvider, client) =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<ApiSettings>>().Value;

            if (string.IsNullOrWhiteSpace(settings.BaseUrl))
            {
                throw new InvalidOperationException($"{ApiSettings.SectionName}:BaseUrl appsettings icinde tanimli olmali.");
            }

            client.BaseAddress = new Uri(settings.BaseUrl, UriKind.Absolute);
        });

        return services;
    }
}
