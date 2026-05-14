using Common;

namespace Gateway;

public static class Api
{
    public static IEndpointRouteBuilder MapGatewayApi(this IEndpointRouteBuilder app)
    {
        app.MapGet("/health", async Task<HealthResponse> () =>
        {
            await Task.CompletedTask;
            return new HealthResponse("gateway", "ok");
        });

        var tenantApi = app.MapGroup("/api")
            .RequireAuthorization(AppAuthorizationPolicies.Viewer);

        var systemApi = app.MapGroup("/api")
            .RequireAuthorization(AppAuthorizationPolicies.SupportAdmin);

        systemApi.MapGet("/status", async (GatewayService service, CancellationToken cancellationToken) =>
        {
            var result = await service.GetStatusAsync(cancellationToken);
            return TypedResults.Ok(result);
        });

        systemApi.MapGet("/catalog/status", GetCatalogStatusAsync);
        systemApi.MapGet("/editor/status", GetEditorStatusAsync);
        systemApi.MapGet("/query/status", GetQueryStatusAsync);
        tenantApi.MapGet("/catalog/categories", GetCatalogCategoriesAsync);
        tenantApi.MapGet("/catalog/types", GetCatalogTypesAsync);
        tenantApi.MapGet("/query/objects", GetQueryObjectsAsync);
        tenantApi.MapPost("/editor/object", CreateObjectAsync)
            .RequireAuthorization(AppAuthorizationPolicies.Editor);

        return app;
    }

    private static async Task<IResult> GetCatalogStatusAsync(
        GatewayService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetCatalogStatusAsync(cancellationToken);
        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.Problem(result.Error!.Message, statusCode: StatusCodes.Status502BadGateway);
    }

    private static async Task<IResult> GetEditorStatusAsync(
        GatewayService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetEditorStatusAsync(cancellationToken);
        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.Problem(result.Error!.Message, statusCode: StatusCodes.Status502BadGateway);
    }

    private static async Task<IResult> GetQueryStatusAsync(
        GatewayService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetQueryStatusAsync(cancellationToken);
        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.Problem(result.Error!.Message, statusCode: StatusCodes.Status502BadGateway);
    }

    private static async Task<IResult> GetCatalogCategoriesAsync(
        GatewayService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetCatalogCategoriesAsync(cancellationToken);
        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.Problem(result.Error!.Message, statusCode: StatusCodes.Status502BadGateway);
    }

    private static async Task<IResult> GetQueryObjectsAsync(
        GatewayService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetQueryObjectsAsync(cancellationToken);
        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.Problem(result.Error!.Message, statusCode: StatusCodes.Status502BadGateway);
    }

    private static async Task<IResult> GetCatalogTypesAsync(
        GatewayService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetCatalogTypesAsync(cancellationToken);
        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.Problem(result.Error!.Message, statusCode: StatusCodes.Status502BadGateway);
    }

    private static async Task<IResult> CreateObjectAsync(
        CreateObjectRequest request,
        GatewayService service,
        CancellationToken cancellationToken)
    {
        var result = await service.CreateObjectAsync(request, cancellationToken);
        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.Problem(result.Error!.Message, statusCode: StatusCodes.Status502BadGateway);
    }
}
