using System;

namespace Tapio.StripeConnector.ApiClient;

public record StripeConnectorApiClientConfig
{
    public required Uri Authority { get; init; }
    public required Uri ResourceUrl { get; init; }
    public Guid ClientId { get; init; }
    public required string ClientSecret { get; init; }
    public required Uri BaseUrl { get; init; }
}
