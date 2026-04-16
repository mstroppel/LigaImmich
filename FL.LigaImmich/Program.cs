using FL.LigaImmich.ImmichClient;
using FL.LigaImmich.Scheduling;
using FL.LigaImmich.Tasks;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<SchedulerOptions>(builder.Configuration.GetSection("Scheduler"));

var immichConfig = builder.Configuration.GetSection("Immich").Get<ImmichClientConfig>()
    ?? throw new InvalidOperationException("Missing 'Immich' configuration section.");
builder.Services.AddImmichClient(immichConfig);

builder.Services.AddScheduledTask<SyncAlbumsTask>();

builder.Services.AddHostedService<CronSchedulerService>();

await builder.Build().RunAsync();
