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
    public void EnvironmentVariableAliases_Map_To_Dotnet_Config_Keys()
    {
        var aliases = new Dictionary<string, string>
        {
            ["IMMICH_BASE_URL"] = "https://immich.example.com/api",
            ["IMMICH_API_KEY"] = "secret",
            ["SCHEDULER_TIMEZONE"] = "UTC",
            ["SCHEDULER_TAG_ASSETS_BY_CLUB_CRON"] = "0 */5 * * * *",
            ["SCHEDULER_TAG_ASSETS_BY_CLUB_ENABLED"] = "true",
        };

        foreach (var (name, value) in aliases)
        {
            Environment.SetEnvironmentVariable(name, value);
        }

        try
        {
            var builder = Host.CreateApplicationBuilder();
            builder.Configuration.AddLigaImmichEnvironmentVariables();

            builder.Services.AddOptions<SchedulerOptions>()
                .Bind(builder.Configuration.GetSection(SchedulerOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();
            builder.Services.AddImmichClient(builder.Configuration);

            using var host = builder.Build();
            host.Start();

            var immich = host.Services.GetRequiredService<IOptions<ImmichClientConfig>>().Value;
            var scheduler = host.Services.GetRequiredService<IOptions<SchedulerOptions>>().Value;

            Assert.Equal(new Uri("https://immich.example.com/api"), immich.BaseUrl);
            Assert.Equal("secret", immich.ApiKey);
            Assert.Equal("UTC", scheduler.TimeZone);
            Assert.Equal("0 */5 * * * *", scheduler.Tasks["TagAssetsByClub"].Cron);
            Assert.True(scheduler.Tasks["TagAssetsByClub"].Enabled);
        }
        finally
        {
            foreach (var name in aliases.Keys)
            {
                Environment.SetEnvironmentVariable(name, null);
            }
        }
    }

    [Fact]
    public void SchedulerOptions_Bind_TaskSchedule_FromConfig()
    {
        var settings = new Dictionary<string, string?>
        {
            ["Immich:BaseUrl"] = "https://immich.example.com/api",
            ["Immich:ApiKey"] = "secret",
            ["Scheduler:TimeZone"] = "UTC",
            ["Scheduler:Tasks:TagAssetsByClub:Cron"] = "0 */5 * * * *",
            ["Scheduler:Tasks:TagAssetsByClub:Enabled"] = "true",
        };

        using var host = BuildHost(settings);
        host.Start();

        var scheduler = host.Services.GetRequiredService<IOptions<SchedulerOptions>>().Value;
        Assert.Equal("UTC", scheduler.TimeZone);
        Assert.True(scheduler.Tasks.TryGetValue("TagAssetsByClub", out var task));
        Assert.Equal("0 */5 * * * *", task!.Cron);
        Assert.True(task.Enabled);
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
