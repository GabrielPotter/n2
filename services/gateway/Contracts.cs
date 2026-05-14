namespace Gateway;

public sealed record HealthResponse(string Service, string Status);

public sealed record GatewayStatusResponse(
    string Service,
    string Status,
    string PublicBaseUrl,
    GatewayRoutesResponse Routes);

public sealed record GatewayRoutesResponse(
    string CatalogStatus,
    string EditorStatus,
    string QueryStatus,
    string CatalogCategories,
    string CatalogTypes,
    string QueryObjects,
    string CreateObject);

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
