using Common;

namespace CoreQuery;

public static class Api
{
    public static IEndpointRouteBuilder MapCoreQueryApi(this IEndpointRouteBuilder app)
    {
        app.MapGet("/health", async Task<HealthResponse> () =>
        {
            await Task.CompletedTask;
            return new HealthResponse("core-query", "ok");
        });

        app.MapGet("/internal/status", GetInternalStatusAsync)
            .RequireAuthorization(AppAuthorizationPolicies.SupportAdmin);
        app.MapGet("/api/query/objects", GetObjectsAsync)
            .RequireAuthorization(AppAuthorizationPolicies.Viewer);

        return app;
    }

    private static async Task<IResult> GetInternalStatusAsync(
        CoreQueryService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetStatusAsync(cancellationToken);
        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.Problem(result.Error!.Message, statusCode: StatusCodes.Status503ServiceUnavailable);
    }

    private static async Task<IResult> GetObjectsAsync(
        CoreQueryService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetObjectsAsync(cancellationToken);
        if (result.IsFailure && result.Error!.Code == "authorization_error")
        {
            return TypedResults.Unauthorized();
        }

        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.Problem(result.Error!.Message, statusCode: StatusCodes.Status503ServiceUnavailable);
    }
}
