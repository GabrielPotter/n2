using Common;

namespace Gateway;

public static class Api
{
    public static IEndpointRouteBuilder MapSystemApi(this IEndpointRouteBuilder app)
    {
        app.MapGet("/health", async Task<HealthResponse> () =>
        {
            await Task.CompletedTask;
            return new HealthResponse("gateway", "ok");
        });

        var systemApi = app.MapGroup("/api/v1/system")
            .RequireAuthorization(AppAuthorizationPolicies.SystemUser);
        app.MapGet("/api/v1/status", GetGatewayStatusListAsync)
            .RequireAuthorization(AppAuthorizationPolicies.SupportAdmin);

        systemApi.MapGet("/me", GetSystemCurrentUser);
        systemApi.MapGet("/tenants", GetSystemTenantsAsync);
        systemApi.MapGet("/tenants/{tenantId}", GetSystemTenantAsync);
        systemApi.MapPost("/tenants", CreateSystemTenantAsync);
        systemApi.MapPut("/tenants/{tenantId}", UpdateSystemTenantAsync);
        systemApi.MapPatch("/tenants/{tenantId}", PatchSystemTenantAsync);
        systemApi.MapDelete("/tenants/{tenantId}", DeleteSystemTenantAsync);

        return app;
    }

    public static IEndpointRouteBuilder MapUserApi(this IEndpointRouteBuilder app)
    {
        var catalogApi = app.MapGroup("/api/v1/catalog")
            .RequireAuthorization(AppAuthorizationPolicies.Viewer);

        var queryApi = app.MapGroup("/api/v1/query")
            .RequireAuthorization(AppAuthorizationPolicies.Viewer);

        var editorApi = app.MapGroup("/api/v1/editor")
            .RequireAuthorization(AppAuthorizationPolicies.Viewer);

        catalogApi.MapGet("/categories", GetCatalogCategoriesAsync);
        catalogApi.MapGet("/types", GetCatalogTypesAsync);
        queryApi.MapGet("/objects", GetQueryObjectsAsync);
        editorApi.MapPost("/object", CreateObjectAsync)
            .RequireAuthorization(AppAuthorizationPolicies.Editor);

        return app;
    }

    private static async Task<IResult> GetGatewayStatusListAsync(
        GatewayService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetStatusAsync(cancellationToken);
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

    private static async Task<IResult> GetSystemCurrentUser(
        GatewayService service,
        CancellationToken cancellationToken)
    {
        return (await service.GetSystemCurrentUserAsync(cancellationToken)).ToResult();
    }

    private static async Task<IResult> GetSystemTenantsAsync(
        GatewayService service,
        CancellationToken cancellationToken)
    {
        return (await service.GetSystemTenantsAsync(cancellationToken)).ToResult();
    }

    private static async Task<IResult> GetSystemTenantAsync(
        string tenantId,
        GatewayService service,
        CancellationToken cancellationToken)
    {
        return (await service.GetSystemTenantAsync(tenantId, cancellationToken)).ToResult();
    }

    private static async Task<IResult> CreateSystemTenantAsync(
        TenantCreateRequest request,
        GatewayService service,
        CancellationToken cancellationToken)
    {
        return (await service.CreateSystemTenantAsync(request, cancellationToken)).ToResult();
    }

    private static async Task<IResult> UpdateSystemTenantAsync(
        string tenantId,
        TenantUpdateRequest request,
        GatewayService service,
        CancellationToken cancellationToken)
    {
        return (await service.UpdateSystemTenantAsync(tenantId, request, cancellationToken)).ToResult();
    }

    private static async Task<IResult> PatchSystemTenantAsync(
        string tenantId,
        TenantPatchRequest request,
        GatewayService service,
        CancellationToken cancellationToken)
    {
        return (await service.PatchSystemTenantAsync(tenantId, request, cancellationToken)).ToResult();
    }

    private static async Task<IResult> DeleteSystemTenantAsync(
        string tenantId,
        GatewayService service,
        CancellationToken cancellationToken)
    {
        return (await service.DeleteSystemTenantAsync(tenantId, cancellationToken)).ToResult();
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
