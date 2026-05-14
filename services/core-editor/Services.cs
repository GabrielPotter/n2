using Common;

namespace CoreEditor;

public static class Services
{
    public static IServiceCollection AddCoreEditorServices(this IServiceCollection services)
    {
        services.AddSingleton<CoreEditorDatabase>();
        services.AddSingleton<CoreEditorService>();
        return services;
    }
}

public sealed class CoreEditorService
{
    private readonly CoreEditorDatabase _database;
    private readonly IUserContextAccessor _userContextAccessor;
    private readonly ILogger<CoreEditorService> _logger;

    public CoreEditorService(
        CoreEditorDatabase database,
        IUserContextAccessor userContextAccessor,
        ILogger<CoreEditorService> logger)
    {
        _database = database;
        _userContextAccessor = userContextAccessor;
        _logger = logger;
    }

    public Task<Result<InternalStatusResponse>> GetStatusAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Core editor status requested.");
        return _database.GetStatusAsync(cancellationToken);
    }

    public async Task<Result<CreateObjectResponse>> CreateObjectAsync(
        CreateObjectRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result<CreateObjectResponse>.Failure(Error.Validation("Name is required."));
        }

        if (string.IsNullOrWhiteSpace(request.CategoryId))
        {
            return Result<CreateObjectResponse>.Failure(Error.Validation("CategoryId is required."));
        }

        if (string.IsNullOrWhiteSpace(request.TypeId))
        {
            return Result<CreateObjectResponse>.Failure(Error.Validation("TypeId is required."));
        }

        _logger.LogInformation(
            "Object creation requested. CategoryId: {CategoryId}, TypeId: {TypeId}",
            request.CategoryId,
            request.TypeId);

        var userContext = _userContextAccessor.GetCurrent();

        if (userContext is null || !Guid.TryParse(userContext.TenantId, out var tenantId))
        {
            return Result<CreateObjectResponse>.Failure(new Error("authorization_error", "A valid tenant context is required."));
        }

        var createResult = await _database.CreateObjectAsync(request, tenantId, cancellationToken);

        if (createResult.IsFailure)
        {
            _logger.LogWarning("Object creation failed. ErrorCode: {ErrorCode}", createResult.Error?.Code);
            return Result<CreateObjectResponse>.Failure(createResult.Error!);
        }

        _logger.LogInformation(
            "Object created. ObjectId: {ObjectId}, CategoryId: {CategoryId}, TypeId: {TypeId}",
            createResult.Value!.Id,
            createResult.Value.CategoryId,
            createResult.Value.TypeId);

        return Result<CreateObjectResponse>.Success(new CreateObjectResponse("core-editor", createResult.Value!));
    }
}
