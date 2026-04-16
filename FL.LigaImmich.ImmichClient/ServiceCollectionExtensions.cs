using Microsoft.Extensions.DependencyInjection;
using System;

namespace FL.LigaImmich.ImmichClient;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddImmichClient(this IServiceCollection services, ImmichClientConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        services.AddHttpClient<IImmichClient, ImmichClient>(httpClient =>
        {
            httpClient.BaseAddress = config.BaseUrl;
            httpClient.DefaultRequestHeaders.Add("x-api-key", config.ApiKey);
        })
        .AddStandardResilienceHandler();

        return services;
    }
}
