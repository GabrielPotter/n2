using Common;
using Microsoft.Extensions.Options;

namespace Gateway;

public static class Services
{
    public static IServiceCollection AddGatewayServices(this IServiceCollection services)
    {
        services.AddSingleton<GatewayService>();
        services.AddSingleton<ITenantIdResolver, GatewayTenantIdResolver>();
        return services;
    }
}

public sealed class GatewayService
{
    private readonly GatewayClient _client;
    private readonly ILogger<GatewayService> _logger;

    public GatewayService(
        GatewayClient client,
        ILogger<GatewayService> logger,
        IOptions<GatewaySettings> settings)
    {
        _client = client;
        _logger = logger;
        _ = settings.Value;
    }

    public Task<Result<InternalStatusResponse>> GetInternalStatusAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Gateway status requested.");
        return Task.FromResult(Result<InternalStatusResponse>.Success(new InternalStatusResponse("gateway", RuntimeStatus.CreateDetails())));
    }

    public async Task<Result<GatewayStatusListResponse>> GetStatusAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Gateway aggregated status requested.");

        var catalogTask = _client.GetCatalogStatusAsync(cancellationToken);
        var editorTask = _client.GetEditorStatusAsync(cancellationToken);
        var queryTask = _client.GetQueryStatusAsync(cancellationToken);
        var systemTask = _client.GetSystemStatusAsync(cancellationToken);

        await Task.WhenAll(catalogTask, editorTask, queryTask, systemTask);

        var services = new List<InternalStatusResponse>
        {
            new("gateway", RuntimeStatus.CreateDetails())
        };

        var upstreamResults = new[]
        {
            await catalogTask,
            await editorTask,
            await queryTask,
            await systemTask
        };

        foreach (var result in upstreamResults)
        {
            if (result.IsFailure)
            {
                return Result<GatewayStatusListResponse>.Failure(result.Error!);
            }

            services.Add(result.Value!);
        }

        return Result<GatewayStatusListResponse>.Success(new GatewayStatusListResponse(services));
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

public sealed class GatewayTenantIdResolver : ITenantIdResolver
{
    private readonly GatewayClient _client;
    private readonly ILogger<GatewayTenantIdResolver> _logger;

    public GatewayTenantIdResolver(GatewayClient client, ILogger<GatewayTenantIdResolver> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<string?> ResolveTenantIdAsync(string tenantName, CancellationToken cancellationToken)
    {
        var result = await _client.GetSystemTenantByNameAsync(tenantName, cancellationToken);

        if (result.IsSuccess)
        {
            return result.Value!.TenantId;
        }

        _logger.LogWarning(
            "Tenant ID resolution failed. TenantName: {TenantName}, ErrorCode: {ErrorCode}",
            tenantName,
            result.Error?.Code);

        return null;
    }
}
