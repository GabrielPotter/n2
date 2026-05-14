namespace CoreEditor;

public sealed record HealthResponse(string Service, string Status);

public sealed record InternalStatusResponse(
    string Service,
    string Status,
    string DatabaseStatus,
    DateTimeOffset CheckedAtUtc);

public sealed record CreateObjectRequest(string Name, string CategoryId, string TypeId);

public sealed record ObjectResponse(
    string Id,
    string Name,
    string ObjectKind,
    string CategoryId,
    string CategoryName,
    string TypeId,
    string TypeName,
    string Status);

public sealed record CreateObjectResponse(string Service, ObjectResponse Object);

public sealed record ErrorResponse(string Code, string Message);
