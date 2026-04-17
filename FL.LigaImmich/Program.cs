using FL.LigaImmich;
using FL.LigaImmich.ImmichClient;
using FL.LigaImmich.Scheduling;
using FL.LigaImmich.Tasks;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddLigaImmichEnvironmentVariables();

builder.Services.AddOptions<SchedulerOptions>()
    .Bind(builder.Configuration.GetSection(SchedulerOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddImmichClient(builder.Configuration);

builder.Services.AddScheduledTask<SyncAlbumsTask>();

builder.Services.AddHostedService<CronSchedulerService>();

await builder.Build().RunAsync();
