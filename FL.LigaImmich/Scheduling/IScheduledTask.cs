namespace FL.LigaImmich.Scheduling;

public interface IScheduledTask
{
    string Name { get; }
    Task ExecuteAsync(CancellationToken cancellationToken);
}
