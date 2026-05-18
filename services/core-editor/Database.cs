using Common;
using Npgsql;

namespace CoreEditor;

public sealed class DatabaseSettings
{
    public const string SectionName = "Database";

    public string ConnectionString { get; init; } =
        "Host=localhost;Port=5432;Database=platformdb;Username=platform;Password=platform";
}

public sealed class CoreEditorDatabase
{
    private readonly IPostgreSqlConnectionFactory _connectionFactory;
    private readonly ILogger<CoreEditorDatabase> _logger;

    public CoreEditorDatabase(IPostgreSqlConnectionFactory connectionFactory, ILogger<CoreEditorDatabase> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<Result<InternalStatusResponse>> GetStatusAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return Result<InternalStatusResponse>.Success(new InternalStatusResponse("core-editor", RuntimeStatus.CreateDetails()));
    }

    public async Task<Result<ObjectResponse>> CreateObjectAsync(
        CreateObjectRequest request,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            const string sql = """
                with inserted_object as (
                  insert into app.graph_object (
                    object_id,
                    tenant_id,
                    object_name,
                    category_id,
                    type_id,
                    properties,
                    object_status
                  )
                  select
                    @id,
                    @tenantId,
                    @objectName,
                    @categoryId,
                    @typeId,
                    '{}'::jsonb,
                    'active'
                  returning object_id, tenant_id, category_id, type_id, object_name, object_status
                )
                select
                  io.object_id,
                  io.object_name,
                  oc.object_kind::text,
                  oc.category_id,
                  oc.category_name,
                  ot.type_id,
                  ot.type_name,
                  io.object_status::text
                from inserted_object io
                join app.object_category oc
                  on oc.tenant_id = io.tenant_id
                 and oc.category_id = io.category_id
                join app.object_type ot
                  on ot.tenant_id = io.tenant_id
                 and ot.category_id = io.category_id
                 and ot.type_id = io.type_id
                """;

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("id", Guid.NewGuid());
            command.Parameters.AddWithValue("tenantId", tenantId);
            command.Parameters.AddWithValue("objectName", request.ObjectName.Trim());
            command.Parameters.AddWithValue("categoryId", Guid.Parse(request.CategoryId));
            command.Parameters.AddWithValue("typeId", Guid.Parse(request.TypeId));

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
            {
                return Result<ObjectResponse>.Failure(new Error("database_error", "Object creation query returned no rows."));
            }

            var response = new ObjectResponse(
                reader.GetGuid(0).ToString("D"),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetGuid(3).ToString("D"),
                reader.GetString(4),
                reader.GetGuid(5).ToString("D"),
                reader.GetString(6),
                reader.GetString(7));

            _logger.LogInformation(
                "Graph object inserted. ObjectId: {ObjectId}, CategoryId: {CategoryId}, TypeId: {TypeId}",
                response.ObjectId,
                response.CategoryId,
                response.TypeId);

            return Result<ObjectResponse>.Success(response);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Graph object insert failed. CategoryId: {CategoryId}, TypeId: {TypeId}", request.CategoryId, request.TypeId);
            return Result<ObjectResponse>.Failure(new Error("database_error", exception.Message));
        }
    }
}
