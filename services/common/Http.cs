using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace Common;

public static class Http
{
    public static IServiceCollection AddCommonHttp(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddTransient<CorrelationIdHandler>();
        services.AddTransient<AccessTokenForwardingHandler>();
        return services;
    }

    public static IApplicationBuilder UseCommonHttp(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CorrelationIdMiddleware>();
    }

    public static IApplicationBuilder UseCommonExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CommonExceptionMiddleware>();
    }
}

public static class CorrelationId
{
    public const string HeaderName = "X-Correlation-Id";
    public const string ItemKey = "correlation_id";

    public static string GetOrCreate(HttpContext context)
    {
        if (context.Items.TryGetValue(ItemKey, out var value) && value is string correlationId && !string.IsNullOrWhiteSpace(correlationId))
        {
            return correlationId;
        }

        correlationId = context.Request.Headers[HeaderName].FirstOrDefault() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.CreateVersion7().ToString("D");
        }

        correlationId = correlationId.Trim();
        context.Items[ItemKey] = correlationId;
        context.TraceIdentifier = correlationId;

        return correlationId;
    }
}

public sealed class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var correlationId = CorrelationId.GetOrCreate(context);

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationId.HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        using (LogContext.PushProperty("correlationId", correlationId))
        {
            await _next(context);
        }
    }
}

public sealed class CorrelationIdHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CorrelationIdHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var correlationId = _httpContextAccessor.HttpContext is null
            ? Guid.CreateVersion7().ToString("D")
            : CorrelationId.GetOrCreate(_httpContextAccessor.HttpContext);

        request.Headers.Remove(CorrelationId.HeaderName);
        request.Headers.Add(CorrelationId.HeaderName, correlationId);

        return base.SendAsync(request, cancellationToken);
    }
}

public sealed class AccessTokenForwardingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AccessTokenForwardingHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var authorizationHeader = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();

        if (!string.IsNullOrWhiteSpace(authorizationHeader))
        {
            request.Headers.Remove("Authorization");
            request.Headers.TryAddWithoutValidation("Authorization", authorizationHeader);
        }

        return base.SendAsync(request, cancellationToken);
    }
}

public sealed class CommonExceptionMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly RequestDelegate _next;
    private readonly ILogger<CommonExceptionMiddleware> _logger;

    public CommonExceptionMiddleware(RequestDelegate next, ILogger<CommonExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception for {Path}", context.Request.Path);

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            await JsonSerializer.SerializeAsync(
                context.Response.Body,
                new { error = Error.Unexpected() },
                JsonOptions,
                context.RequestAborted);
        }
    }
}
