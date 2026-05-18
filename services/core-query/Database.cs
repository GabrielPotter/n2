using Common;
using Npgsql;

namespace CoreQuery;

public sealed class DatabaseSettings
{
    public const string SectionName = "Database";

    public string ConnectionString { get; init; } =
        "Host=localhost;Port=5432;Database=platformdb;Username=platform;Password=platform";
}

public sealed class CoreQueryDatabase
{
    private readonly IPostgreSqlConnectionFactory _connectionFactory;
    private readonly ILogger<CoreQueryDatabase> _logger;

    public CoreQueryDatabase(IPostgreSqlConnectionFactory connectionFactory, ILogger<CoreQueryDatabase> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<Result<InternalStatusResponse>> GetStatusAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return Result<InternalStatusResponse>.Success(new InternalStatusResponse("core-query", RuntimeStatus.CreateDetails()));
    }

    public async Task<Result<IReadOnlyList<QueryObjectResponse>>> GetObjectsAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        try
        {
            var objects = new List<QueryObjectResponse>();

            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            const string sql = """
                select
                  go.object_id,
                  go.object_name,
                  oc.object_kind::text,
                  oc.category_id,
                  oc.category_name,
                  ot.type_id,
                  ot.type_name,
                  go.object_status::text
                from app.graph_object go
                join app.object_category oc
                  on oc.tenant_id = go.tenant_id
                 and oc.category_id = go.category_id
                join app.object_type ot
                  on ot.tenant_id = go.tenant_id
                 and ot.category_id = go.category_id
                 and ot.type_id = go.type_id
                where go.tenant_id = @tenantId
                  and go.object_status = 'active'
                order by go.created_at desc, go.object_name
                """;

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("tenantId", tenantId);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                objects.Add(
                    new QueryObjectResponse(
                        reader.GetGuid(0).ToString("D"),
                        reader.GetString(1),
                        reader.GetString(2),
                        reader.GetGuid(3).ToString("D"),
                        reader.GetString(4),
                        reader.GetGuid(5).ToString("D"),
                        reader.GetString(6),
                        reader.GetString(7)));
            }

            return Result<IReadOnlyList<QueryObjectResponse>>.Success(objects);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Graph object query failed.");
            return Result<IReadOnlyList<QueryObjectResponse>>.Failure(new Error("database_error", exception.Message));
        }
    }
}
