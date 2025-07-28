using Microsoft.Identity.Client;

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Tapio.StripeConnector.ApiClient;

public partial class StripeConnectorApiClient
{
    private readonly StripeConnectorApiClientConfig _Config;
    private readonly IConfidentialClientApplication _ConfidentialClientApplication;

    public StripeConnectorApiClient(HttpClient httpClient, StripeConnectorApiClientConfig config, IConfidentialClientApplication confidentialClientApplication)
        : this(config.BaseUrl.ToString(), httpClient)
    {
        _Config = config ?? throw new ArgumentNullException(nameof(config));
        _ConfidentialClientApplication = confidentialClientApplication ?? throw new ArgumentNullException(nameof(confidentialClientApplication));
        BaseUrl = _Config.BaseUrl.ToString();

        ReadResponseAsString = true;
    }

    static partial void UpdateJsonSerializerSettings(JsonSerializerOptions settings)
    {
        settings.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    }

    private async Task PrepareRequestAsync(HttpClient client, HttpRequestMessage request, string url, CancellationToken cancellationToken)
    {
        _ = client;
        _ = url;
        await InjectBearerTokenAsync(request, cancellationToken);
    }

    private async Task PrepareRequestAsync(HttpClient client, HttpRequestMessage request, StringBuilder urlBuilder, CancellationToken cancellationToken)
    {
        _ = client;
        _ = urlBuilder;
        await InjectBearerTokenAsync(request, cancellationToken);
    }

    private static Task ProcessResponseAsync(HttpClient client, HttpResponseMessage message, CancellationToken cancellationToken)
    {
        _ = client;
        _ = message;
        _ = cancellationToken;
        return Task.CompletedTask;
    }

    private async Task InjectBearerTokenAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        var tokenBuilder = _ConfidentialClientApplication.AcquireTokenForClient([$"{_Config.ResourceUrl}/.default"]);
        var token = await tokenBuilder.ExecuteAsync(cancellationToken);

        request.Headers.Authorization = new AuthenticationHeaderValue(
            scheme: "Bearer",
            parameter: token.AccessToken);
    }
}
