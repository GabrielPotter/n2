using Common;

namespace CoreEditor;

public static class Api
{
    public static IEndpointRouteBuilder MapCoreEditorApi(this IEndpointRouteBuilder app)
    {
        app.MapGet("/health", async Task<HealthResponse> () =>
        {
            await Task.CompletedTask;
            return new HealthResponse("core-editor", "ok");
        });

        app.MapGet("/internal/status", GetInternalStatusAsync);
        app.MapPost("/api/editor/object", CreateObjectAsync);

        return app;
    }

    private static async Task<IResult> GetInternalStatusAsync(
        CoreEditorService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetStatusAsync(cancellationToken);
        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.Problem(result.Error!.Message, statusCode: StatusCodes.Status503ServiceUnavailable);
    }

    private static async Task<IResult> CreateObjectAsync(
        CreateObjectRequest request,
        CoreEditorService service,
        CancellationToken cancellationToken)
    {
        var result = await service.CreateObjectAsync(request, cancellationToken);

        if (result.IsFailure && result.Error!.Code == "validation_error")
        {
            return TypedResults.BadRequest(new ErrorResponse(result.Error.Code, result.Error.Message));
        }

        if (result.IsFailure && result.Error!.Code == "authorization_error")
        {
            return TypedResults.Unauthorized();
        }

        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.Problem(result.Error!.Message, statusCode: StatusCodes.Status503ServiceUnavailable);
    }
}
