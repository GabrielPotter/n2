using Common;
using System.Text.Json;

namespace SystemService;

public static class Services
{
    public static IServiceCollection AddSystemServices(this IServiceCollection services)
    {
        services.AddSingleton<SystemDatabase>();
        services.AddSingleton<SystemRealmService>();
        services.AddSingleton<TenantService>();
        return services;
    }
}

public sealed class SystemRealmService
{
    private readonly ILogger<SystemRealmService> _logger;

    public SystemRealmService(
        ILogger<SystemRealmService> logger)
    {
        _logger = logger;
    }

    public Task<Result<InternalStatusResponse>> GetStatusAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("System status requested.");
        return Task.FromResult(Result<InternalStatusResponse>.Success(new InternalStatusResponse("system", RuntimeStatus.CreateDetails())));
    }
}

public sealed class TenantService
{
    private static readonly HashSet<string> ValidStatuses = new(StringComparer.Ordinal)
    {
        "active",
        "inactive",
        "deleted"
    };

    private readonly SystemDatabase _database;
    private readonly ILogger<TenantService> _logger;

    public TenantService(SystemDatabase database, ILogger<TenantService> logger)
    {
        _database = database;
        _logger = logger;
    }

    public Task<Result<IReadOnlyList<TenantResponse>>> GetTenantsAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Tenant list requested.");
        return _database.GetTenantsAsync(cancellationToken);
    }

    public Task<Result<TenantResponse>> GetTenantAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Tenant requested. TenantId: {TenantId}", tenantId);
        return _database.GetTenantByIdAsync(tenantId, cancellationToken);
    }

    public Task<Result<TenantLookupResponse>> GetTenantByNameAsync(string tenantName, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Tenant requested by name. TenantName: {TenantName}", tenantName);
        return _database.GetTenantByNameAsync(tenantName, cancellationToken);
    }

    public async Task<Result<TenantResponse>> CreateTenantAsync(
        TenantCreateRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = ValidateCreateOrUpdateRequest(request.TenantName, request.TenantStatus, request.Properties);

        if (validationError is not null)
        {
            return Result<TenantResponse>.Failure(validationError);
        }

        var tenantId = Guid.CreateVersion7();
        var tenantName = request.TenantName!.Trim();
        var tenantStatus = request.TenantStatus!.Trim();
        var properties = NormalizeProperties(request.Properties);

        _logger.LogInformation("Tenant creation requested. TenantId: {TenantId}", tenantId);

        return await _database.CreateTenantAsync(tenantId, tenantName, tenantStatus, properties, cancellationToken);
    }

    public async Task<Result<TenantResponse>> UpdateTenantAsync(
        Guid tenantId,
        TenantUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = ValidateCreateOrUpdateRequest(request.TenantName, request.TenantStatus, request.Properties);

        if (validationError is not null)
        {
            return Result<TenantResponse>.Failure(validationError);
        }

        _logger.LogInformation("Tenant replace requested. TenantId: {TenantId}", tenantId);

        return await _database.UpdateTenantAsync(
            tenantId,
            request.TenantName!.Trim(),
            request.TenantStatus!.Trim(),
            NormalizeProperties(request.Properties),
            cancellationToken);
    }

    public async Task<Result<TenantResponse>> PatchTenantAsync(
        Guid tenantId,
        TenantPatchRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = ValidatePatchRequest(request);

        if (validationError is not null)
        {
            return Result<TenantResponse>.Failure(validationError);
        }

        _logger.LogInformation("Tenant patch requested. TenantId: {TenantId}", tenantId);

        return await _database.PatchTenantAsync(
            tenantId,
            string.IsNullOrWhiteSpace(request.TenantName) ? null : request.TenantName.Trim(),
            string.IsNullOrWhiteSpace(request.TenantStatus) ? null : request.TenantStatus.Trim(),
            request.Properties.HasValue ? NormalizeProperties(request.Properties) : null,
            cancellationToken);
    }

    public Task<Result<bool>> DeleteTenantAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Tenant delete requested. TenantId: {TenantId}", tenantId);
        return _database.DeleteTenantAsync(tenantId, cancellationToken);
    }

    public Result<CurrentUserResponse> GetCurrentUser(UserContext? userContext)
    {
        if (userContext is null)
        {
            return Result<CurrentUserResponse>.Failure(Error.Unauthorized());
        }

        return Result<CurrentUserResponse>.Success(
            new CurrentUserResponse(
                userContext.UserId,
                userContext.Username,
                userContext.Email,
                userContext.Realm,
                userContext.TenantId,
                userContext.Roles));
    }

    private static Error? ValidateCreateOrUpdateRequest(string? name, string? status, JsonElement? properties)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Error.Validation("TenantName is required.");
        }

        if (string.IsNullOrWhiteSpace(status))
        {
            return Error.Validation("TenantStatus is required.");
        }

        if (!ValidStatuses.Contains(status.Trim()))
        {
            return Error.Validation("Status must be one of: active, inactive, deleted.");
        }

        if (properties.HasValue && properties.Value.ValueKind != JsonValueKind.Object)
        {
            return Error.Validation("Properties must be a JSON object.");
        }

        return null;
    }

    private static Error? ValidatePatchRequest(TenantPatchRequest request)
    {
        if (request.TenantName is null && request.TenantStatus is null && !request.Properties.HasValue)
        {
            return Error.Validation("At least one field must be provided.");
        }

        if (request.TenantStatus is not null && (string.IsNullOrWhiteSpace(request.TenantStatus) || !ValidStatuses.Contains(request.TenantStatus.Trim())))
        {
            return Error.Validation("Status must be one of: active, inactive, deleted.");
        }

        if (request.TenantName is not null && string.IsNullOrWhiteSpace(request.TenantName))
        {
            return Error.Validation("TenantName is required.");
        }

        if (request.Properties.HasValue && request.Properties.Value.ValueKind != JsonValueKind.Object)
        {
            return Error.Validation("Properties must be a JSON object.");
        }

        return null;
    }

    private static JsonElement NormalizeProperties(JsonElement? properties)
    {
        return properties.HasValue
            ? properties.Value.Clone()
            : JsonDocument.Parse("{}").RootElement.Clone();
    }
}
