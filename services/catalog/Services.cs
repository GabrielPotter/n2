using Common;

namespace Catalog;

public static class Services
{
    public static IServiceCollection AddCatalogServices(this IServiceCollection services)
    {
        services.AddSingleton<CatalogDatabase>();
        services.AddSingleton<CatalogService>();
        return services;
    }
}

public sealed class CatalogService
{
    private readonly CatalogDatabase _database;
    private readonly IUserContextAccessor _userContextAccessor;
    private readonly ILogger<CatalogService> _logger;

    public CatalogService(
        CatalogDatabase database,
        IUserContextAccessor userContextAccessor,
        ILogger<CatalogService> logger)
    {
        _database = database;
        _userContextAccessor = userContextAccessor;
        _logger = logger;
    }

    public Task<Result<InternalStatusResponse>> GetStatusAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Catalog status requested.");
        return Task.FromResult(Result<InternalStatusResponse>.Success(new InternalStatusResponse("catalog", RuntimeStatus.CreateDetails())));
    }

    public async Task<Result<CatalogCategoriesResponse>> GetCategoriesAsync(CancellationToken cancellationToken)
    {
        var userContext = _userContextAccessor.GetCurrent();

        if (userContext is null || string.IsNullOrWhiteSpace(userContext.TenantId) || !Guid.TryParse(userContext.TenantId, out var tenantId))
        {
            return Result<CatalogCategoriesResponse>.Failure(Error.Unauthorized("A valid tenant context is required."));
        }

        var categoriesResult = await _database.GetCategoriesAsync(tenantId, cancellationToken);

        if (categoriesResult.IsFailure)
        {
            _logger.LogWarning("Catalog category query failed. ErrorCode: {ErrorCode}", categoriesResult.Error?.Code);
            return Result<CatalogCategoriesResponse>.Failure(categoriesResult.Error!);
        }

        _logger.LogInformation("Catalog categories returned. Count: {CategoryCount}", categoriesResult.Value!.Count);

        return Result<CatalogCategoriesResponse>.Success(
            new CatalogCategoriesResponse("catalog", categoriesResult.Value!));
    }

    public async Task<Result<CatalogTypesResponse>> GetTypesAsync(CancellationToken cancellationToken)
    {
        var userContext = _userContextAccessor.GetCurrent();

        if (userContext is null || string.IsNullOrWhiteSpace(userContext.TenantId) || !Guid.TryParse(userContext.TenantId, out var tenantId))
        {
            return Result<CatalogTypesResponse>.Failure(Error.Unauthorized("A valid tenant context is required."));
        }

        var typesResult = await _database.GetTypesAsync(tenantId, cancellationToken);

        if (typesResult.IsFailure)
        {
            _logger.LogWarning("Catalog type query failed. ErrorCode: {ErrorCode}", typesResult.Error?.Code);
            return Result<CatalogTypesResponse>.Failure(typesResult.Error!);
        }

        _logger.LogInformation("Catalog types returned. Count: {TypeCount}", typesResult.Value!.Count);

        return Result<CatalogTypesResponse>.Success(new CatalogTypesResponse("catalog", typesResult.Value!));
    }
}
