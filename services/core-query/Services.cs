using Common;

namespace CoreQuery;

public static class Services
{
    public static IServiceCollection AddCoreQueryServices(this IServiceCollection services)
    {
        services.AddSingleton<CoreQueryDatabase>();
        services.AddSingleton<CoreQueryService>();
        return services;
    }
}

public sealed class CoreQueryService
{
    private readonly CoreQueryDatabase _database;
    private readonly IUserContextAccessor _userContextAccessor;
    private readonly ILogger<CoreQueryService> _logger;

    public CoreQueryService(
        CoreQueryDatabase database,
        IUserContextAccessor userContextAccessor,
        ILogger<CoreQueryService> logger)
    {
        _database = database;
        _userContextAccessor = userContextAccessor;
        _logger = logger;
    }

    public Task<Result<InternalStatusResponse>> GetStatusAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Core query status requested.");
        return _database.GetStatusAsync(cancellationToken);
    }

    public async Task<Result<QueryObjectsResponse>> GetObjectsAsync(CancellationToken cancellationToken)
    {
        var userContext = _userContextAccessor.GetCurrent();

        if (userContext is null || string.IsNullOrWhiteSpace(userContext.TenantId) || !Guid.TryParse(userContext.TenantId, out var tenantId))
        {
            return Result<QueryObjectsResponse>.Failure(Error.Unauthorized("A valid tenant context is required."));
        }

        var objectsResult = await _database.GetObjectsAsync(tenantId, cancellationToken);

        if (objectsResult.IsFailure)
        {
            _logger.LogWarning("Object query failed. ErrorCode: {ErrorCode}", objectsResult.Error?.Code);
            return Result<QueryObjectsResponse>.Failure(objectsResult.Error!);
        }

        _logger.LogInformation("Objects returned. Count: {ObjectCount}", objectsResult.Value!.Count);

        return Result<QueryObjectsResponse>.Success(new QueryObjectsResponse("core-query", objectsResult.Value!));
    }
}
