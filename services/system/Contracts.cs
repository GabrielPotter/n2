using System.Text.Json;

namespace SystemService;

public sealed record HealthResponse(string Service, string Status);

public sealed record InternalStatusResponse(
    string Service,
    IReadOnlyDictionary<string, string?> Details);

public sealed class TenantCreateRequest
{
    public string? TenantName { get; init; }

    public string? TenantStatus { get; init; }

    public JsonElement? Properties { get; init; }
}

public sealed class TenantUpdateRequest
{
    public string? TenantName { get; init; }

    public string? TenantStatus { get; init; }

    public JsonElement? Properties { get; init; }
}

public sealed class TenantPatchRequest
{
    public string? TenantName { get; init; }

    public string? TenantStatus { get; init; }

    public JsonElement? Properties { get; init; }
}

public sealed record TenantResponse(
    string TenantId,
    string TenantName,
    string TenantStatus,
    JsonElement Properties,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? DeletedAt);

public sealed record TenantLookupResponse(
    string TenantId,
    string TenantName);

public sealed record CurrentUserResponse(
    string Subject,
    string? Username,
    string? Email,
    string Realm,
    string? TenantId,
    IReadOnlyList<string> Roles);
