using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Common;
using Microsoft.Extensions.Options;

namespace Gateway;

public sealed class GatewayClientsSettings
{
    public const string SectionName = "InternalServices";

    public string CatalogUrl { get; init; } = "http://localhost:5201";

    public string CoreEditorUrl { get; init; } = "http://localhost:5202";

    public string CoreQueryUrl { get; init; } = "http://localhost:5203";

    public string SystemUrl { get; init; } = "http://localhost:5204";
}

public static class Clients
{
    public static IServiceCollection AddGatewayClients(this IServiceCollection services)
    {
        services
            .AddHttpClient("gateway-upstream")
            .AddHttpMessageHandler<CorrelationIdHandler>()
            .AddHttpMessageHandler<UserContextForwardingHandler>();

        services.AddSingleton<GatewayClient>();
        return services;
    }
}

public sealed class GatewayClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GatewayClient> _logger;
    private readonly GatewayClientsSettings _settings;

    public GatewayClient(
        IHttpClientFactory httpClientFactory,
        ILogger<GatewayClient> logger,
        IOptions<GatewayClientsSettings> settings)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _settings = settings.Value;
    }

    public Task<Result<InternalStatusResponse>> GetCatalogStatusAsync(CancellationToken cancellationToken)
    {
        return GetStatusAsync(_settings.CatalogUrl, cancellationToken);
    }

    public Task<Result<InternalStatusResponse>> GetEditorStatusAsync(CancellationToken cancellationToken)
    {
        return GetStatusAsync(_settings.CoreEditorUrl, cancellationToken);
    }

    public Task<Result<InternalStatusResponse>> GetQueryStatusAsync(CancellationToken cancellationToken)
    {
        return GetStatusAsync(_settings.CoreQueryUrl, cancellationToken);
    }

    public Task<Result<InternalStatusResponse>> GetSystemStatusAsync(CancellationToken cancellationToken)
    {
        return GetStatusAsync(_settings.SystemUrl, cancellationToken);
    }

    public Task<Result<CatalogCategoriesResponse>> GetCatalogCategoriesAsync(CancellationToken cancellationToken)
    {
        return GetAsync<CatalogCategoriesResponse>(_settings.CatalogUrl, "/api/catalog/categories", cancellationToken);
    }

    public Task<Result<CatalogTypesResponse>> GetCatalogTypesAsync(CancellationToken cancellationToken)
    {
        return GetAsync<CatalogTypesResponse>(_settings.CatalogUrl, "/api/catalog/types", cancellationToken);
    }

    public Task<Result<QueryObjectsResponse>> GetQueryObjectsAsync(CancellationToken cancellationToken)
    {
        return GetAsync<QueryObjectsResponse>(_settings.CoreQueryUrl, "/api/query/objects", cancellationToken);
    }

    public Task<ProxyResponse> GetSystemCurrentUserAsync(CancellationToken cancellationToken)
    {
        return SendAsync(HttpMethod.Get, _settings.SystemUrl, "/api/v1/me", cancellationToken);
    }

    public Task<ProxyResponse> GetSystemTenantsAsync(CancellationToken cancellationToken)
    {
        return SendAsync(HttpMethod.Get, _settings.SystemUrl, "/api/v1/tenants", cancellationToken);
    }

    public Task<ProxyResponse> GetSystemTenantAsync(string tenantId, CancellationToken cancellationToken)
    {
        return SendAsync(HttpMethod.Get, _settings.SystemUrl, $"/api/v1/tenants/{tenantId}", cancellationToken);
    }

    public Task<Result<TenantLookupResponse>> GetSystemTenantByNameAsync(string tenantName, CancellationToken cancellationToken)
    {
        var encodedTenantName = Uri.EscapeDataString(tenantName);
        return GetAsync<TenantLookupResponse>(_settings.SystemUrl, $"/api/v1/tenats/by-name/{encodedTenantName}", cancellationToken);
    }

    public Task<ProxyResponse> CreateSystemTenantAsync(TenantCreateRequest request, CancellationToken cancellationToken)
    {
        return SendAsync(HttpMethod.Post, _settings.SystemUrl, "/api/v1/tenants", cancellationToken, request);
    }

    public Task<ProxyResponse> UpdateSystemTenantAsync(string tenantId, TenantUpdateRequest request, CancellationToken cancellationToken)
    {
        return SendAsync(HttpMethod.Put, _settings.SystemUrl, $"/api/v1/tenants/{tenantId}", cancellationToken, request);
    }

    public Task<ProxyResponse> PatchSystemTenantAsync(string tenantId, TenantPatchRequest request, CancellationToken cancellationToken)
    {
        return SendAsync(new HttpMethod("PATCH"), _settings.SystemUrl, $"/api/v1/tenants/{tenantId}", cancellationToken, request);
    }

    public Task<ProxyResponse> DeleteSystemTenantAsync(string tenantId, CancellationToken cancellationToken)
    {
        return SendAsync(HttpMethod.Delete, _settings.SystemUrl, $"/api/v1/tenants/{tenantId}", cancellationToken);
    }

    public Task<Result<CreateObjectResponse>> CreateObjectAsync(
        CreateObjectRequest request,
        CancellationToken cancellationToken)
    {
        return PostAsync<CreateObjectRequest, CreateObjectResponse>(
            _settings.CoreEditorUrl,
            "/api/editor/object",
            request,
            cancellationToken);
    }

    private async Task<Result<InternalStatusResponse>> GetStatusAsync(string baseUrl, CancellationToken cancellationToken)
    {
        return await GetAsync<InternalStatusResponse>(baseUrl, "/internal/status", cancellationToken);
    }

    private async Task<Result<TResponse>> GetAsync<TResponse>(
        string baseUrl,
        string path,
        CancellationToken cancellationToken)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("gateway-upstream");
            var response = await client.GetAsync($"{baseUrl.TrimEnd('/')}{path}", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Upstream GET failed. BaseUrl: {BaseUrl}, Path: {Path}, StatusCode: {StatusCode}",
                    baseUrl,
                    path,
                    (int)response.StatusCode);

                return Result<TResponse>.Failure(
                    new Error("upstream_error", $"Upstream status check failed with {(int)response.StatusCode}."));
            }

            var payload = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken);

            return payload is null
                ? Result<TResponse>.Failure(new Error("upstream_error", "Upstream response was empty."))
                : Result<TResponse>.Success(payload);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Upstream GET failed. BaseUrl: {BaseUrl}, Path: {Path}", baseUrl, path);
            return Result<TResponse>.Failure(new Error("upstream_error", exception.Message));
        }
    }

    private async Task<Result<TResponse>> PostAsync<TRequest, TResponse>(
        string baseUrl,
        string path,
        TRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("gateway-upstream");
            var response = await client.PostAsJsonAsync($"{baseUrl.TrimEnd('/')}{path}", request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning(
                    "Upstream POST failed. BaseUrl: {BaseUrl}, Path: {Path}, StatusCode: {StatusCode}",
                    baseUrl,
                    path,
                    (int)response.StatusCode);

                return Result<TResponse>.Failure(
                    new Error("upstream_error", string.IsNullOrWhiteSpace(message) ? $"Upstream request failed with {(int)response.StatusCode}." : message));
            }

            var payload = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken);

            return payload is null
                ? Result<TResponse>.Failure(new Error("upstream_error", "Upstream response was empty."))
                : Result<TResponse>.Success(payload);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Upstream POST failed. BaseUrl: {BaseUrl}, Path: {Path}", baseUrl, path);
            return Result<TResponse>.Failure(new Error("upstream_error", exception.Message));
        }
    }

    private Task<ProxyResponse> SendAsync(
        HttpMethod method,
        string baseUrl,
        string path,
        CancellationToken cancellationToken)
    {
        return SendAsync<object>(method, baseUrl, path, cancellationToken);
    }

    private async Task<ProxyResponse> SendAsync<TRequest>(
        HttpMethod method,
        string baseUrl,
        string path,
        CancellationToken cancellationToken,
        TRequest? request = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("gateway-upstream");
            using var message = new HttpRequestMessage(method, $"{baseUrl.TrimEnd('/')}{path}");

            if (request is not null)
            {
                message.Content = JsonContent.Create(request);
            }

            using var response = await client.SendAsync(message, cancellationToken);
            var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/json";
            var body = response.Content is null
                ? string.Empty
                : await response.Content.ReadAsStringAsync(cancellationToken);

            return new ProxyResponse((int)response.StatusCode, contentType, body);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Upstream proxy request failed. BaseUrl: {BaseUrl}, Path: {Path}, Method: {Method}", baseUrl, path, method);

            return new ProxyResponse(
                StatusCodes.Status502BadGateway,
                "application/json",
                JsonSerializer.Serialize(new
                {
                    error = new
                    {
                        code = "upstream_error",
                        message = exception.Message
                    }
                }));
        }
    }
}

public sealed record ProxyResponse(int StatusCode, string ContentType, string Body)
{
    public IResult ToResult()
    {
        if (StatusCode == StatusCodes.Status204NoContent)
        {
            return TypedResults.NoContent();
        }

        return Results.Content(Body, ContentType, Encoding.UTF8, StatusCode);
    }
}
