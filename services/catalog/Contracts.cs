namespace Catalog;

public sealed record HealthResponse(string Service, string Status);

public sealed record InternalStatusResponse(
    string Service,
    IReadOnlyDictionary<string, string?> Details);

public sealed record CategoryResponse(string CategoryId, string ObjectKind, string CategoryName);

public sealed record CatalogCategoriesResponse(
    string Service,
    IReadOnlyList<CategoryResponse> Categories);

public sealed record TypeResponse(string TypeId, string CategoryId, string TypeName);

public sealed record CatalogTypesResponse(
    string Service,
    IReadOnlyList<TypeResponse> Types);
