using System.Text.Json;

namespace Gateway;

public sealed record HealthResponse(string Service, string Status);

public sealed record GatewayStatusResponse(
    string Service,
    string Status,
    string PublicBaseUrl,
    GatewayRoutesResponse Routes);

public sealed record GatewayRoutesResponse(
    string GatewayStatus,
    string CatalogStatus,
    string EditorStatus,
    string QueryStatus,
    string SystemStatus,
    string CurrentUser,
    string CatalogCategories,
    string CatalogTypes,
    string QueryObjects,
    string CreateObject,
    string SystemTenants);

public sealed record InternalStatusResponse(
    string Service,
    string Status,
    string DatabaseStatus,
    DateTimeOffset CheckedAtUtc);

public sealed record CatalogCategoryResponse(string CategoryId, string ObjectKind, string Name);

public sealed record CatalogCategoriesResponse(
    string Service,
    IReadOnlyList<CatalogCategoryResponse> Categories);

public sealed record CatalogTypeResponse(string TypeId, string CategoryId, string Name);

public sealed record CatalogTypesResponse(
    string Service,
    IReadOnlyList<CatalogTypeResponse> Types);

public sealed record QueryObjectResponse(
    string Id,
    string Name,
    string ObjectKind,
    string CategoryId,
    string CategoryName,
    string TypeId,
    string TypeName,
    string Status);

public sealed record QueryObjectsResponse(
    string Service,
    IReadOnlyList<QueryObjectResponse> Objects);

public sealed record CreateObjectRequest(string Name, string CategoryId, string TypeId);

public sealed record EditorObjectResponse(
    string Id,
    string Name,
    string ObjectKind,
    string CategoryId,
    string CategoryName,
    string TypeId,
    string TypeName,
    string Status);

public sealed record CreateObjectResponse(string Service, EditorObjectResponse Object);

public sealed class TenantCreateRequest
{
    public string? Name { get; init; }

    public string? Status { get; init; }

    public JsonElement? Properties { get; init; }
}

public sealed class TenantUpdateRequest
{
    public string? Name { get; init; }

    public string? Status { get; init; }

    public JsonElement? Properties { get; init; }
}

public sealed class TenantPatchRequest
{
    public string? Name { get; init; }

    public string? Status { get; init; }

    public JsonElement? Properties { get; init; }
}

public sealed record TenantResponse(
    string Id,
    string Name,
    string Status,
    JsonElement Properties,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? DeletedAt);

public sealed record CurrentUserResponse(
    string Subject,
    string? Username,
    string? Email,
    string Realm,
    string? TenantId,
    IReadOnlyList<string> Roles);
