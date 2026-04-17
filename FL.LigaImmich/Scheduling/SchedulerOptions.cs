using System.ComponentModel.DataAnnotations;

namespace FL.LigaImmich.Scheduling;

public sealed class SchedulerOptions
{
    public const string SectionName = "Scheduler";

    public string? TimeZone { get; set; }
    public Dictionary<string, TaskScheduleOptions> Tasks { get; set; } = new();
}

public sealed class TaskScheduleOptions
{
    [Required]
    [MinLength(1)]
    public string Cron { get; set; } = "";
    public bool Enabled { get; set; } = true;
}
