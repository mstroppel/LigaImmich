using System;
using System.ComponentModel.DataAnnotations;

namespace FL.LigaImmich.ImmichClient;

public sealed class ImmichClientConfig
{
    public const string SectionName = "Immich";

    [Required]
    public Uri? BaseUrl { get; set; }

    [Required]
    [MinLength(1)]
    public string? ApiKey { get; set; }
}
