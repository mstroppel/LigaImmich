using FL.LigaImmich.ImmichClient;
using FL.LigaImmich.Scheduling;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace FL.LigaImmich.Tests;

public class ConfigurationValidationTests
{
    [Fact]
    public void ImmichClientConfig_Missing_ApiKey_Fails_Validation()
    {
        var settings = new Dictionary<string, string?>
        {
            ["Immich:BaseUrl"] = "https://immich.example.com/api",
        };

        var host = BuildHost(settings);

        var ex = Assert.ThrowsAny<OptionsValidationException>(host.Start);
        Assert.Contains(nameof(ImmichClientConfig.ApiKey), ex.Message);
    }

    [Fact]
    public void ImmichClientConfig_Missing_BaseUrl_Fails_Validation()
    {
        var settings = new Dictionary<string, string?>
        {
            ["Immich:ApiKey"] = "secret",
        };

        var host = BuildHost(settings);

        var ex = Assert.ThrowsAny<OptionsValidationException>(host.Start);
        Assert.Contains(nameof(ImmichClientConfig.BaseUrl), ex.Message);
    }

    [Fact]
    public void ImmichClientConfig_Valid_Settings_Bind()
    {
        var settings = new Dictionary<string, string?>
        {
            ["Immich:BaseUrl"] = "https://immich.example.com/api",
            ["Immich:ApiKey"] = "secret",
        };

        using var host = BuildHost(settings);
        host.Start();

        var options = host.Services.GetRequiredService<IOptions<ImmichClientConfig>>().Value;
        Assert.Equal(new Uri("https://immich.example.com/api"), options.BaseUrl);
        Assert.Equal("secret", options.ApiKey);
    }

    [Fact]
    public void SchedulerOptions_Bind_TaskSchedule_FromConfig()
    {
        var settings = new Dictionary<string, string?>
        {
            ["Immich:BaseUrl"] = "https://immich.example.com/api",
            ["Immich:ApiKey"] = "secret",
            ["Scheduler:TimeZone"] = "UTC",
            ["Scheduler:Tasks:SyncAlbums:Cron"] = "0 */5 * * * *",
            ["Scheduler:Tasks:SyncAlbums:Enabled"] = "true",
        };

        using var host = BuildHost(settings);
        host.Start();

        var scheduler = host.Services.GetRequiredService<IOptions<SchedulerOptions>>().Value;
        Assert.Equal("UTC", scheduler.TimeZone);
        Assert.True(scheduler.Tasks.TryGetValue("SyncAlbums", out var sync));
        Assert.Equal("0 */5 * * * *", sync!.Cron);
        Assert.True(sync.Enabled);
    }

    private static IHost BuildHost(IDictionary<string, string?> settings)
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Configuration.AddInMemoryCollection(settings);

        builder.Services.AddOptions<SchedulerOptions>()
            .Bind(builder.Configuration.GetSection(SchedulerOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        builder.Services.AddImmichClient(builder.Configuration);

        return builder.Build();
    }
}
