namespace FL.LigaImmich;

internal static class EnvironmentVariableConfiguration
{
    private static readonly Dictionary<string, string> Aliases = new()
    {
        ["IMMICH_BASE_URL"] = "Immich:BaseUrl",
        ["IMMICH_API_KEY"] = "Immich:ApiKey",
        ["SCHEDULER_TIMEZONE"] = "Scheduler:TimeZone",
        ["SCHEDULER_TAG_ASSETS_BY_CLUB_CRON"] = "Scheduler:Tasks:TagAssetsByClub:Cron",
        ["SCHEDULER_TAG_ASSETS_BY_CLUB_ENABLED"] = "Scheduler:Tasks:TagAssetsByClub:Enabled",
        ["SCHEDULER_TAG_ASSETS_BY_CLUB_RUN_ON_STARTUP"] = "Scheduler:Tasks:TagAssetsByClub:RunOnStartup",
        ["SCHEDULER_TAG_ASSETS_BY_FOLDER_STRUCTURE_CRON"] = "Scheduler:Tasks:TagAssetsByFolderStructure:Cron",
        ["SCHEDULER_TAG_ASSETS_BY_FOLDER_STRUCTURE_ENABLED"] = "Scheduler:Tasks:TagAssetsByFolderStructure:Enabled",
        ["SCHEDULER_TAG_ASSETS_BY_FOLDER_STRUCTURE_RUN_ON_STARTUP"] = "Scheduler:Tasks:TagAssetsByFolderStructure:RunOnStartup",
    };

    public static IConfigurationBuilder AddLigaImmichEnvironmentVariables(this IConfigurationBuilder builder)
    {
        var mapped = new Dictionary<string, string?>();
        foreach (var (envName, configKey) in Aliases)
        {
            var value = Environment.GetEnvironmentVariable(envName);
            if (!string.IsNullOrEmpty(value))
            {
                mapped[configKey] = value;
            }
        }

        if (mapped.Count > 0)
        {
            builder.AddInMemoryCollection(mapped);
        }

        return builder;
    }
}
