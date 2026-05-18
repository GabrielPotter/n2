using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace Common;

public static class Http
{
    public static IServiceCollection AddCommonHttp(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddTransient<CorrelationIdHandler>();
        services.AddTransient<UserContextForwardingHandler>();
        services.TryAddSingleton<IUserContextAccessor, UserContextAccessor>();
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

public static class RequestContextHeaders
{
    public const string UserId = "X-User-Id";
    public const string TenantId = "X-Tenant-Id";
    public const string Username = "X-Username";
    public const string Email = "X-Email";
    public const string Realm = "X-Realm";
    public const string Roles = "X-Roles";
    public const string AuthzVersion = "X-Authz-Version";
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

public sealed class UserContextForwardingHandler : DelegatingHandler
{
    private readonly IUserContextAccessor _userContextAccessor;

    public UserContextForwardingHandler(IUserContextAccessor userContextAccessor)
    {
        _userContextAccessor = userContextAccessor;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var userContext = _userContextAccessor.GetCurrent();

        if (userContext is not null)
        {
            SetHeader(request, RequestContextHeaders.UserId, userContext.UserId);
            SetHeader(request, RequestContextHeaders.TenantId, userContext.TenantId);
            SetHeader(request, RequestContextHeaders.Username, userContext.Username);
            SetHeader(request, RequestContextHeaders.Email, userContext.Email);
            SetHeader(request, RequestContextHeaders.Realm, userContext.Realm);
            SetHeader(request, RequestContextHeaders.Roles, userContext.Roles.Count == 0 ? null : string.Join(",", userContext.Roles));
            SetHeader(request, RequestContextHeaders.AuthzVersion, userContext.AuthzVersion?.ToString());
        }

        return base.SendAsync(request, cancellationToken);
    }

    private static void SetHeader(HttpRequestMessage request, string headerName, string? value)
    {
        request.Headers.Remove(headerName);

        if (!string.IsNullOrWhiteSpace(value))
        {
            request.Headers.TryAddWithoutValidation(headerName, value);
        }
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
