using Common;
using Npgsql;

namespace Catalog;

public sealed class DatabaseSettings
{
    public const string SectionName = "Database";

    public string ConnectionString { get; init; } =
        "Host=localhost;Port=5432;Database=platformdb;Username=platform;Password=platform";
}

public sealed class CatalogDatabase
{
    private readonly IPostgreSqlConnectionFactory _connectionFactory;
    private readonly ILogger<CatalogDatabase> _logger;

    public CatalogDatabase(IPostgreSqlConnectionFactory connectionFactory, ILogger<CatalogDatabase> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<Result<InternalStatusResponse>> GetStatusAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return Result<InternalStatusResponse>.Success(new InternalStatusResponse("catalog", RuntimeStatus.CreateDetails()));
    }

    public async Task<Result<IReadOnlyList<CategoryResponse>>> GetCategoriesAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        try
        {
            var categories = new List<CategoryResponse>();

            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            const string sql = """
                select
                  oc.category_id,
                  oc.object_kind::text,
                  oc.category_name
                from app.object_category oc
                where oc.tenant_id = @tenantId
                  and oc.category_status = 'active'
                order by oc.category_name
                """;

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("tenantId", tenantId);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                categories.Add(
                    new CategoryResponse(
                        reader.GetGuid(0).ToString("D"),
                        reader.GetString(1),
                        reader.GetString(2)));
            }

            return Result<IReadOnlyList<CategoryResponse>>.Success(categories);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Catalog categories query failed.");
            return Result<IReadOnlyList<CategoryResponse>>.Failure(new Error("database_error", exception.Message));
        }
    }

    public async Task<Result<IReadOnlyList<TypeResponse>>> GetTypesAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        try
        {
            var types = new List<TypeResponse>();

            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            const string sql = """
                select
                  ot.type_id,
                  ot.category_id,
                  ot.type_name
                from app.object_type ot
                where ot.tenant_id = @tenantId
                  and ot.type_status = 'active'
                order by ot.type_name
                """;

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("tenantId", tenantId);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                types.Add(
                    new TypeResponse(
                        reader.GetGuid(0).ToString("D"),
                        reader.GetGuid(1).ToString("D"),
                        reader.GetString(2)));
            }

            return Result<IReadOnlyList<TypeResponse>>.Success(types);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Catalog types query failed.");
            return Result<IReadOnlyList<TypeResponse>>.Failure(new Error("database_error", exception.Message));
        }
    }
}
