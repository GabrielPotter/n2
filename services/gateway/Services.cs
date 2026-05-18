using Common;
using Microsoft.Extensions.Options;

namespace Gateway;

public static class Services
{
    public static IServiceCollection AddGatewayServices(this IServiceCollection services)
    {
        services.AddSingleton<GatewayService>();
        return services;
    }
}

public sealed class GatewayService
{
    private readonly GatewayClient _client;
    private readonly ILogger<GatewayService> _logger;
    private readonly GatewaySettings _settings;

    public GatewayService(
        GatewayClient client,
        ILogger<GatewayService> logger,
        IOptions<GatewaySettings> settings)
    {
        _client = client;
        _logger = logger;
        _settings = settings.Value;
    }

    public Task<GatewayStatusResponse> GetStatusAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Gateway status requested.");

        var response = new GatewayStatusResponse(
            "gateway",
            "ok",
            _settings.PublicBaseUrl,
            new GatewayRoutesResponse(
                "/api/v1/gateway/status",
                "/api/v1/catalog/status",
                "/api/v1/editor/status",
                "/api/v1/query/status",
                "/api/v1/system/status",
                "/api/v1/system/me",
                "/api/v1/catalog/categories",
                "/api/v1/catalog/types",
                "/api/v1/query/objects",
                "/api/v1/editor/object",
                "/api/v1/system/tenants"));

        return Task.FromResult(response);
    }

    public Task<Common.Result<InternalStatusResponse>> GetCatalogStatusAsync(CancellationToken cancellationToken)
    {
        return _client.GetCatalogStatusAsync(cancellationToken);
    }

    public Task<Common.Result<InternalStatusResponse>> GetEditorStatusAsync(CancellationToken cancellationToken)
    {
        return _client.GetEditorStatusAsync(cancellationToken);
    }

    public Task<Common.Result<InternalStatusResponse>> GetQueryStatusAsync(CancellationToken cancellationToken)
    {
        return _client.GetQueryStatusAsync(cancellationToken);
    }

    public Task<Common.Result<InternalStatusResponse>> GetSystemStatusAsync(CancellationToken cancellationToken)
    {
        return _client.GetSystemStatusAsync(cancellationToken);
    }

    public Task<Common.Result<CatalogCategoriesResponse>> GetCatalogCategoriesAsync(CancellationToken cancellationToken)
    {
        return _client.GetCatalogCategoriesAsync(cancellationToken);
    }

    public Task<Common.Result<QueryObjectsResponse>> GetQueryObjectsAsync(CancellationToken cancellationToken)
    {
        return _client.GetQueryObjectsAsync(cancellationToken);
    }

    public Task<Common.Result<CatalogTypesResponse>> GetCatalogTypesAsync(CancellationToken cancellationToken)
    {
        return _client.GetCatalogTypesAsync(cancellationToken);
    }

    public Task<ProxyResponse> GetSystemCurrentUserAsync(CancellationToken cancellationToken)
    {
        return _client.GetSystemCurrentUserAsync(cancellationToken);
    }

    public Task<ProxyResponse> GetSystemTenantsAsync(CancellationToken cancellationToken)
    {
        return _client.GetSystemTenantsAsync(cancellationToken);
    }

    public Task<ProxyResponse> GetSystemTenantAsync(string tenantId, CancellationToken cancellationToken)
    {
        return _client.GetSystemTenantAsync(tenantId, cancellationToken);
    }

    public Task<ProxyResponse> CreateSystemTenantAsync(TenantCreateRequest request, CancellationToken cancellationToken)
    {
        return _client.CreateSystemTenantAsync(request, cancellationToken);
    }

    public Task<ProxyResponse> UpdateSystemTenantAsync(string tenantId, TenantUpdateRequest request, CancellationToken cancellationToken)
    {
        return _client.UpdateSystemTenantAsync(tenantId, request, cancellationToken);
    }

    public Task<ProxyResponse> PatchSystemTenantAsync(string tenantId, TenantPatchRequest request, CancellationToken cancellationToken)
    {
        return _client.PatchSystemTenantAsync(tenantId, request, cancellationToken);
    }

    public Task<ProxyResponse> DeleteSystemTenantAsync(string tenantId, CancellationToken cancellationToken)
    {
        return _client.DeleteSystemTenantAsync(tenantId, cancellationToken);
    }

    public Task<Common.Result<CreateObjectResponse>> CreateObjectAsync(
        CreateObjectRequest request,
        CancellationToken cancellationToken)
    {
        return _client.CreateObjectAsync(request, cancellationToken);
    }
}
