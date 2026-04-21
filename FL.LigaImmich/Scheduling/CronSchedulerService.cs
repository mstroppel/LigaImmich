using Cronos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FL.LigaImmich.Scheduling;

internal sealed class CronSchedulerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEnumerable<ScheduledTaskRegistration> _registrations;
    private readonly IOptionsMonitor<SchedulerOptions> _options;
    private readonly ILogger<CronSchedulerService> _logger;

    public CronSchedulerService(
        IServiceProvider serviceProvider,
        IEnumerable<ScheduledTaskRegistration> registrations,
        IOptionsMonitor<SchedulerOptions> options,
        ILogger<CronSchedulerService> logger)
    {
        _serviceProvider = serviceProvider;
        _registrations = registrations;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var options = _options.CurrentValue;
        var timeZone = ResolveTimeZone(options.TimeZone);

        var loops = new List<Task>();
        foreach (var registration in _registrations)
        {
            var taskName = GetTaskName(registration.TaskType);
            if (!options.Tasks.TryGetValue(taskName, out var taskOptions))
            {
                _logger.LogWarning("No schedule configured for task {TaskName}; skipping.", taskName);
                continue;
            }

            if (!taskOptions.Enabled)
            {
                _logger.LogInformation("Task {TaskName} is disabled; skipping.", taskName);
                continue;
            }

            CronExpression cron;
            try
            {
                cron = CronExpression.Parse(taskOptions.Cron, CronFormat.IncludeSeconds);
            }
            catch (CronFormatException ex)
            {
                _logger.LogError(ex, "Invalid cron expression for task {TaskName}: {Cron}", taskName, taskOptions.Cron);
                continue;
            }

            loops.Add(RunTaskLoopAsync(registration, taskName, cron, timeZone, taskOptions.RunOnStartup, stoppingToken));
        }

        if (loops.Count == 0)
        {
            _logger.LogWarning("No scheduled tasks are active; scheduler is idle.");
        }

        await Task.WhenAll(loops);
    }

    private async Task RunTaskLoopAsync(
        ScheduledTaskRegistration registration,
        string taskName,
        CronExpression cron,
        TimeZoneInfo timeZone,
        bool runOnStartup,
        CancellationToken stoppingToken)
    {
        _logger.LogInformation("Scheduling task {TaskName} with cron {Cron} in zone {TimeZone}.",
            taskName, cron.ToString(), timeZone.Id);

        if (runOnStartup)
        {
            try
            {
                await using var scope = _serviceProvider.CreateAsyncScope();
                var task = registration.Resolver(scope.ServiceProvider);
                _logger.LogInformation("Executing task {TaskName} on startup.", taskName);
                await task.ExecuteAsync(stoppingToken);
                _logger.LogInformation("Task {TaskName} startup run completed.", taskName);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Task {TaskName} threw an exception on startup.", taskName);
            }
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTimeOffset.UtcNow;
            var next = cron.GetNextOccurrence(now, timeZone);
            if (next is null)
            {
                _logger.LogWarning("Task {TaskName} has no future occurrence; loop exiting.", taskName);
                return;
            }

            var delay = next.Value - now;
            if (delay > TimeSpan.Zero)
            {
                try
                {
                    await Task.Delay(delay, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }

            try
            {
                await using var scope = _serviceProvider.CreateAsyncScope();
                var task = registration.Resolver(scope.ServiceProvider);
                _logger.LogInformation("Executing task {TaskName}.", taskName);
                await task.ExecuteAsync(stoppingToken);
                _logger.LogInformation("Task {TaskName} completed.", taskName);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Task {TaskName} threw an exception.", taskName);
            }
        }
    }

    private static string GetTaskName(Type type)
    {
        var name = type.Name;
        return name.EndsWith("Task", StringComparison.Ordinal) && name.Length > 4
            ? name[..^4]
            : name;
    }

    private static TimeZoneInfo ResolveTimeZone(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return TimeZoneInfo.Utc;
        }

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(id);
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.Utc;
        }
    }
}
