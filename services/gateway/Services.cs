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
                "/api/catalog/status",
                "/api/editor/status",
                "/api/query/status",
                "/api/catalog/categories",
                "/api/catalog/types",
                "/api/query/objects",
                "/api/editor/object"));

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

    public Task<Common.Result<CreateObjectResponse>> CreateObjectAsync(
        CreateObjectRequest request,
        CancellationToken cancellationToken)
    {
        return _client.CreateObjectAsync(request, cancellationToken);
    }
}
