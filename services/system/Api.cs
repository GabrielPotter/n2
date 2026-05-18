using Common;

namespace SystemService;

public static class Api
{
    public static IEndpointRouteBuilder MapSystemApi(this IEndpointRouteBuilder app)
    {
        app.MapGet("/health", async Task<HealthResponse> () =>
        {
            await Task.CompletedTask;
            return new HealthResponse("system", "ok");
        });

        app.MapGet("/internal/status", GetInternalStatusAsync);
        app.MapGet("/api/v1/tenats/by-name/{tenantName}", GetTenantByNameAsync);
        app.MapGet("/api/v1/me", GetCurrentUser);
        app.MapGet("/api/v1/tenants", GetTenantsAsync);
        app.MapGet("/api/v1/tenants/{tenantId}", GetTenantAsync);
        app.MapPost("/api/v1/tenants", CreateTenantAsync);
        app.MapPut("/api/v1/tenants/{tenantId}", UpdateTenantAsync);
        app.MapPatch("/api/v1/tenants/{tenantId}", PatchTenantAsync);
        app.MapDelete("/api/v1/tenants/{tenantId}", DeleteTenantAsync);

        return app;
    }

    private static async Task<IResult> GetInternalStatusAsync(
        SystemRealmService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetStatusAsync(cancellationToken);
        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.Problem(result.Error!.Message, statusCode: StatusCodes.Status503ServiceUnavailable);
    }

    private static IResult GetCurrentUser(
        TenantService service,
        IUserContextAccessor userContextAccessor)
    {
        var result = service.GetCurrentUser(userContextAccessor.GetCurrent());
        return ToResult(result);
    }

    private static async Task<IResult> GetTenantsAsync(
        TenantService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetTenantsAsync(cancellationToken);
        return ToResult(result);
    }

    private static async Task<IResult> GetTenantAsync(
        string tenantId,
        TenantService service,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(tenantId, out var parsedTenantId))
        {
            return TypedResults.BadRequest(new { error = Error.Validation("TenantId must be a valid UUID.") });
        }

        var result = await service.GetTenantAsync(parsedTenantId, cancellationToken);
        return ToResult(result);
    }

    private static async Task<IResult> GetTenantByNameAsync(
        string tenantName,
        TenantService service,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(tenantName))
        {
            return TypedResults.BadRequest(new { error = Error.Validation("TenantName is required.") });
        }

        var result = await service.GetTenantByNameAsync(tenantName.Trim(), cancellationToken);
        return ToResult(result);
    }

    private static async Task<IResult> CreateTenantAsync(
        TenantCreateRequest request,
        TenantService service,
        CancellationToken cancellationToken)
    {
        var result = await service.CreateTenantAsync(request, cancellationToken);
        return ToResult(result, StatusCodes.Status201Created);
    }

    private static async Task<IResult> UpdateTenantAsync(
        string tenantId,
        TenantUpdateRequest request,
        TenantService service,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(tenantId, out var parsedTenantId))
        {
            return TypedResults.BadRequest(new { error = Error.Validation("TenantId must be a valid UUID.") });
        }

        var result = await service.UpdateTenantAsync(parsedTenantId, request, cancellationToken);
        return ToResult(result);
    }

    private static async Task<IResult> PatchTenantAsync(
        string tenantId,
        TenantPatchRequest request,
        TenantService service,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(tenantId, out var parsedTenantId))
        {
            return TypedResults.BadRequest(new { error = Error.Validation("TenantId must be a valid UUID.") });
        }

        var result = await service.PatchTenantAsync(parsedTenantId, request, cancellationToken);
        return ToResult(result);
    }

    private static async Task<IResult> DeleteTenantAsync(
        string tenantId,
        TenantService service,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(tenantId, out var parsedTenantId))
        {
            return TypedResults.BadRequest(new { error = Error.Validation("TenantId must be a valid UUID.") });
        }

        var result = await service.DeleteTenantAsync(parsedTenantId, cancellationToken);

        if (result.IsSuccess)
        {
            return TypedResults.NoContent();
        }

        return ToErrorResult(result.Error!);
    }

    private static IResult ToResult<T>(Result<T> result, int successStatusCode = StatusCodes.Status200OK)
    {
        if (result.IsSuccess)
        {
            return successStatusCode == StatusCodes.Status201Created
                ? TypedResults.Created(string.Empty, result.Value)
                : TypedResults.Ok(result.Value);
        }

        return ToErrorResult(result.Error!);
    }

    private static IResult ToErrorResult(Error error)
    {
        return error.Code switch
        {
            "validation_error" => TypedResults.BadRequest(new { error }),
            "authorization_error" => TypedResults.Unauthorized(),
            "forbidden_error" => TypedResults.Json(new { error }, statusCode: StatusCodes.Status403Forbidden),
            "not_found" => TypedResults.Json(new { error }, statusCode: StatusCodes.Status404NotFound),
            "conflict" => TypedResults.Json(new { error }, statusCode: StatusCodes.Status409Conflict),
            _ => TypedResults.Problem(error.Message, statusCode: StatusCodes.Status503ServiceUnavailable)
        };
    }
}
