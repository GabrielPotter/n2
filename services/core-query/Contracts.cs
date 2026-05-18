namespace CoreQuery;

public sealed record HealthResponse(string Service, string Status);

public sealed record InternalStatusResponse(
    string Service,
    IReadOnlyDictionary<string, string?> Details);

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
