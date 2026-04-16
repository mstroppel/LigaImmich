using Microsoft.Extensions.DependencyInjection;

namespace FL.LigaImmich.Scheduling;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddScheduledTask<T>(this IServiceCollection services)
        where T : class, IScheduledTask
    {
        services.AddScoped<T>();
        services.AddSingleton(new ScheduledTaskRegistration(
            typeof(T),
            sp => sp.GetRequiredService<T>()));
        return services;
    }
}
