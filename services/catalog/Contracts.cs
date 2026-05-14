namespace Catalog;

public sealed record HealthResponse(string Service, string Status);

public sealed record InternalStatusResponse(
    string Service,
    string Status,
    string DatabaseStatus,
    DateTimeOffset CheckedAtUtc);

public sealed record CategoryResponse(string CategoryId, string ObjectKind, string Name);

public sealed record CatalogCategoriesResponse(
    string Service,
    IReadOnlyList<CategoryResponse> Categories);

public sealed record TypeResponse(string TypeId, string CategoryId, string Name);

public sealed record CatalogTypesResponse(
    string Service,
    IReadOnlyList<TypeResponse> Types);
