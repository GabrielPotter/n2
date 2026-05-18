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
    private readonly SystemDatabase _database;

    public SystemRealmService(
        ILogger<SystemRealmService> logger,
        SystemDatabase database)
    {
        _logger = logger;
        _database = database;
    }

    public Task<Result<InternalStatusResponse>> GetStatusAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("System status requested.");

        return _database.GetStatusAsync(cancellationToken);
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

    public async Task<Result<TenantResponse>> CreateTenantAsync(
        TenantCreateRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = ValidateCreateOrUpdateRequest(request.Name, request.Status, request.Properties);

        if (validationError is not null)
        {
            return Result<TenantResponse>.Failure(validationError);
        }

        var tenantId = Guid.CreateVersion7();
        var name = request.Name!.Trim();
        var status = request.Status!.Trim();
        var properties = NormalizeProperties(request.Properties);

        _logger.LogInformation("Tenant creation requested. TenantId: {TenantId}", tenantId);

        return await _database.CreateTenantAsync(tenantId, name, status, properties, cancellationToken);
    }

    public async Task<Result<TenantResponse>> UpdateTenantAsync(
        Guid tenantId,
        TenantUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = ValidateCreateOrUpdateRequest(request.Name, request.Status, request.Properties);

        if (validationError is not null)
        {
            return Result<TenantResponse>.Failure(validationError);
        }

        _logger.LogInformation("Tenant replace requested. TenantId: {TenantId}", tenantId);

        return await _database.UpdateTenantAsync(
            tenantId,
            request.Name!.Trim(),
            request.Status!.Trim(),
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
            string.IsNullOrWhiteSpace(request.Name) ? null : request.Name.Trim(),
            string.IsNullOrWhiteSpace(request.Status) ? null : request.Status.Trim(),
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
            return Error.Validation("Name is required.");
        }

        if (string.IsNullOrWhiteSpace(status))
        {
            return Error.Validation("Status is required.");
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
        if (request.Name is null && request.Status is null && !request.Properties.HasValue)
        {
            return Error.Validation("At least one field must be provided.");
        }

        if (request.Status is not null && (string.IsNullOrWhiteSpace(request.Status) || !ValidStatuses.Contains(request.Status.Trim())))
        {
            return Error.Validation("Status must be one of: active, inactive, deleted.");
        }

        if (request.Name is not null && string.IsNullOrWhiteSpace(request.Name))
        {
            return Error.Validation("Name is required.");
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
