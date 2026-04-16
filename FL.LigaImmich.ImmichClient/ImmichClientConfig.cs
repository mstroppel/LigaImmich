using System;

namespace FL.LigaImmich.ImmichClient;

public record ImmichClientConfig
{
    public required Uri BaseUrl { get; init; }
    public required string ApiKey { get; init; }
}
