using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Common;

public static class ServiceDefaults
{
    public static IServiceCollection AddCommonServices(
        this IServiceCollection services,
        string? connectionString = null)
    {
        services.AddCommonHttp();

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            services.TryAddSingleton<IPostgreSqlConnectionFactory>(
                new PostgreSqlConnectionFactory(connectionString));
        }

        return services;
    }
}

public static class RuntimeStatus
{
    private static readonly DateTimeOffset StartedAtUtc = DateTimeOffset.UtcNow;

    public static IReadOnlyDictionary<string, string?> CreateDetails()
    {
        return new Dictionary<string, string?>
        {
            ["uptime"] = FormatUptime(DateTimeOffset.UtcNow - StartedAtUtc)
        };
    }

    private static string FormatUptime(TimeSpan uptime)
    {
        var totalHours = (int)uptime.TotalHours;
        return $"{totalHours:00}:{uptime.Minutes:00}:{uptime.Seconds:00}";
    }
}
