using System.Text.Json;

namespace Gateway;

public sealed record HealthResponse(string Service, string Status);

public sealed record InternalStatusResponse(
    string Service,
    IReadOnlyDictionary<string, string?> Details);

public sealed record GatewayStatusListResponse(
    IReadOnlyList<InternalStatusResponse> Services);

public sealed record CatalogCategoryResponse(string CategoryId, string ObjectKind, string CategoryName);

public sealed record CatalogCategoriesResponse(
    string Service,
    IReadOnlyList<CatalogCategoryResponse> Categories);

public sealed record CatalogTypeResponse(string TypeId, string CategoryId, string TypeName);

public sealed record CatalogTypesResponse(
    string Service,
    IReadOnlyList<CatalogTypeResponse> Types);

public sealed record QueryObjectResponse(
    string ObjectId,
    string ObjectName,
    string ObjectKind,
    string CategoryId,
    string CategoryName,
    string TypeId,
    string TypeName,
    string ObjectStatus);

public sealed record QueryObjectsResponse(
    string Service,
    IReadOnlyList<QueryObjectResponse> Objects);

public sealed record CreateObjectRequest(string ObjectName, string CategoryId, string TypeId);

public sealed record EditorObjectResponse(
    string ObjectId,
    string ObjectName,
    string ObjectKind,
    string CategoryId,
    string CategoryName,
    string TypeId,
    string TypeName,
    string ObjectStatus);

public sealed record CreateObjectResponse(string Service, EditorObjectResponse Object);

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
