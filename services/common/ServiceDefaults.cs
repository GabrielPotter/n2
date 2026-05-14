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
