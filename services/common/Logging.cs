using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace Common;

public static class Logging
{
    public static WebApplicationBuilder AddCommonLogging(this WebApplicationBuilder builder, string serviceName)
    {
        var version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "0.0.0";

        builder.Host.UseSerilog((_, _, configuration) =>
        {
            configuration
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("service", serviceName)
                .Enrich.WithProperty("environment", builder.Environment.EnvironmentName)
                .Enrich.WithProperty("machine", Environment.MachineName)
                .Enrich.WithProperty("application", ProjectInfo.Name)
                .Enrich.WithProperty("version", version)
                .WriteTo.Console(new CompactJsonFormatter());
        });

        return builder;
    }

    public static IApplicationBuilder UseCommonRequestLogging(this IApplicationBuilder app)
    {
        return app.UseSerilogRequestLogging(options =>
        {
            options.EnrichDiagnosticContext = (diagnosticContext, context) =>
            {
                diagnosticContext.Set("correlationId", CorrelationId.GetOrCreate(context));
                diagnosticContext.Set("requestPath", context.Request.Path.Value ?? string.Empty);
                diagnosticContext.Set("requestMethod", context.Request.Method);
                diagnosticContext.Set("remoteIpAddress", context.Connection.RemoteIpAddress?.ToString() ?? string.Empty);
            };
        });
    }
}
