namespace FL.LigaImmich.Scheduling;

internal sealed record ScheduledTaskRegistration(
    Type TaskType,
    Func<IServiceProvider, IScheduledTask> Resolver);
