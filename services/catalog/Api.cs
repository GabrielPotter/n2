using Common;

namespace Catalog;

public static class Api
{
    public static IEndpointRouteBuilder MapCatalogApi(this IEndpointRouteBuilder app)
    {
        app.MapGet("/health", async Task<HealthResponse> () =>
        {
            await Task.CompletedTask;
            return new HealthResponse("catalog", "ok");
        });

        app.MapGet("/internal/status", GetInternalStatusAsync)
            .RequireAuthorization(AppAuthorizationPolicies.SupportAdmin);
        app.MapGet("/api/catalog/categories", GetCategoriesAsync)
            .RequireAuthorization(AppAuthorizationPolicies.Viewer);
        app.MapGet("/api/catalog/types", GetTypesAsync)
            .RequireAuthorization(AppAuthorizationPolicies.Viewer);

        return app;
    }

    private static async Task<IResult> GetInternalStatusAsync(
        CatalogService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetStatusAsync(cancellationToken);
        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.Problem(result.Error!.Message, statusCode: StatusCodes.Status503ServiceUnavailable);
    }

    private static async Task<IResult> GetCategoriesAsync(
        CatalogService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetCategoriesAsync(cancellationToken);
        if (result.IsFailure && result.Error!.Code == "authorization_error")
        {
            return TypedResults.Unauthorized();
        }

        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.Problem(result.Error!.Message, statusCode: StatusCodes.Status503ServiceUnavailable);
    }

    private static async Task<IResult> GetTypesAsync(
        CatalogService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetTypesAsync(cancellationToken);
        if (result.IsFailure && result.Error!.Code == "authorization_error")
        {
            return TypedResults.Unauthorized();
        }

        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.Problem(result.Error!.Message, statusCode: StatusCodes.Status503ServiceUnavailable);
    }
}
