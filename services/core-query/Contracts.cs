namespace CoreQuery;

public sealed record HealthResponse(string Service, string Status);

public sealed record InternalStatusResponse(
    string Service,
    string Status,
    string DatabaseStatus,
    DateTimeOffset CheckedAtUtc);

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
