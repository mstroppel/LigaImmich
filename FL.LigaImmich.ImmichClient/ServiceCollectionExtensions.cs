using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;

using System;

namespace Tapio.StripeConnector.ApiClient;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddStripeConnectorApiClient(this IServiceCollection services, StripeConnectorApiClientConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(config.BaseUrl);
        ArgumentNullException.ThrowIfNull(config.Authority);
        ArgumentNullException.ThrowIfNull(config.ResourceUrl);

        var confidentialClient = ConfidentialClientApplicationBuilder
            .Create(config.ClientId.ToString())
            .WithAuthority(config.Authority)
            .WithClientSecret(config.ClientSecret)
            .Build();

        return services.AddHttpClient<IStripeConnectorApiClient, StripeConnectorApiClient>(
            httpClient => new StripeConnectorApiClient(httpClient, config, confidentialClient))
                .AddStandardResilienceHandler()
                .Services;
    }
}
