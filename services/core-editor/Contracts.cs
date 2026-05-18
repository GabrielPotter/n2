namespace CoreEditor;

public sealed record HealthResponse(string Service, string Status);

public sealed record InternalStatusResponse(
    string Service,
    IReadOnlyDictionary<string, string?> Details);

public sealed record CreateObjectRequest(string ObjectName, string CategoryId, string TypeId);

public sealed record ObjectResponse(
    string ObjectId,
    string ObjectName,
    string ObjectKind,
    string CategoryId,
    string CategoryName,
    string TypeId,
    string TypeName,
    string ObjectStatus);

public sealed record CreateObjectResponse(string Service, ObjectResponse Object);

public sealed record ErrorResponse(string Code, string Message);
