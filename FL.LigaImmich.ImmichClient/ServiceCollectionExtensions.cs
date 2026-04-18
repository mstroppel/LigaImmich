using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FL.LigaImmich.ImmichClient;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddImmichClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ImmichClientConfig>()
            .Bind(configuration.GetSection(ImmichClientConfig.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHttpClient<IImmichClient, ImmichClient>((sp, httpClient) =>
        {
            var config = sp.GetRequiredService<IOptions<ImmichClientConfig>>().Value;
            httpClient.BaseAddress = config.BaseUrl;
            httpClient.DefaultRequestHeaders.Add("x-api-key", config.ApiKey);
        })
        .AddStandardResilienceHandler();

        return services;
    }
}
