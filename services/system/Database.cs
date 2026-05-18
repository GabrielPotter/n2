using System.Text.Json;
using Common;
using Npgsql;
using NpgsqlTypes;

namespace SystemService;

public sealed class DatabaseSettings
{
    public const string SectionName = "Database";

    public string ConnectionString { get; init; } =
        "Host=localhost;Port=5432;Database=platformdb;Username=platform;Password=platform";
}

public sealed class SystemDatabase
{
    private static readonly JsonElement EmptyJsonObject = JsonDocument.Parse("{}").RootElement.Clone();

    private readonly IPostgreSqlConnectionFactory _connectionFactory;
    private readonly ILogger<SystemDatabase> _logger;

    public SystemDatabase(IPostgreSqlConnectionFactory connectionFactory, ILogger<SystemDatabase> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<Result<InternalStatusResponse>> GetStatusAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await using var command = new NpgsqlCommand("select current_database()", connection);
            var databaseName = (string?)await command.ExecuteScalarAsync(cancellationToken) ?? "unknown";

            return Result<InternalStatusResponse>.Success(
                new InternalStatusResponse("system", "ok", $"connected:{databaseName}", DateTimeOffset.UtcNow));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "System database status query failed.");
            return Result<InternalStatusResponse>.Failure(new Error("database_error", exception.Message));
        }
    }

    public async Task<Result<IReadOnlyList<TenantResponse>>> GetTenantsAsync(CancellationToken cancellationToken)
    {
        try
        {
            const string sql = """
                select
                  t.id,
                  t.name,
                  t.status::text,
                  t.properties,
                  t.created_at,
                  t.updated_at,
                  t.deleted_at
                from app.tenant t
                where t.status <> 'deleted'
                order by t.name, t.id
                """;

            return Result<IReadOnlyList<TenantResponse>>.Success(
                await QueryTenantsAsync(sql, cancellationToken));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Tenant list query failed.");
            return Result<IReadOnlyList<TenantResponse>>.Failure(new Error("database_error", exception.Message));
        }
    }

    public async Task<Result<TenantResponse>> GetTenantByIdAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        try
        {
            const string sql = """
                select
                  t.id,
                  t.name,
                  t.status::text,
                  t.properties,
                  t.created_at,
                  t.updated_at,
                  t.deleted_at
                from app.tenant t
                where t.id = @tenantId
                  and t.status <> 'deleted'
                """;

            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("tenantId", tenantId);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
            {
                return Result<TenantResponse>.Failure(Error.NotFound("Tenant was not found."));
            }

            return Result<TenantResponse>.Success(ReadTenant(reader));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Tenant get query failed. TenantId: {TenantId}", tenantId);
            return Result<TenantResponse>.Failure(new Error("database_error", exception.Message));
        }
    }

    public async Task<Result<TenantResponse>> CreateTenantAsync(
        Guid tenantId,
        string name,
        string status,
        JsonElement properties,
        CancellationToken cancellationToken)
    {
        try
        {
            const string sql = """
                insert into app.tenant (
                  id,
                  name,
                  status,
                  properties,
                  deleted_at
                )
                values (
                  @tenantId,
                  @name,
                  cast(@status as app.record_status),
                  @properties,
                  case when @status = 'deleted' then now() else null end
                )
                returning
                  id,
                  name,
                  status::text,
                  properties,
                  created_at,
                  updated_at,
                  deleted_at
                """;

            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("tenantId", tenantId);
            command.Parameters.AddWithValue("name", name);
            command.Parameters.AddWithValue("status", status);
            command.Parameters.Add("properties", NpgsqlDbType.Jsonb).Value = properties.GetRawText();

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            await reader.ReadAsync(cancellationToken);

            return Result<TenantResponse>.Success(ReadTenant(reader));
        }
        catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            _logger.LogWarning(exception, "Tenant create conflict. TenantId: {TenantId}", tenantId);
            return Result<TenantResponse>.Failure(Error.Conflict("Tenant already exists."));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Tenant create failed. TenantId: {TenantId}", tenantId);
            return Result<TenantResponse>.Failure(new Error("database_error", exception.Message));
        }
    }

    public async Task<Result<TenantResponse>> UpdateTenantAsync(
        Guid tenantId,
        string name,
        string status,
        JsonElement properties,
        CancellationToken cancellationToken)
    {
        try
        {
            const string sql = """
                update app.tenant
                set
                  name = @name,
                  status = cast(@status as app.record_status),
                  properties = @properties,
                  deleted_at = case
                    when @status = 'deleted' then coalesce(deleted_at, now())
                    else null
                  end
                where id = @tenantId
                returning
                  id,
                  name,
                  status::text,
                  properties,
                  created_at,
                  updated_at,
                  deleted_at
                """;

            return await ExecuteTenantMutationAsync(
                sql,
                tenantId,
                name,
                status,
                properties,
                cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Tenant update failed. TenantId: {TenantId}", tenantId);
            return Result<TenantResponse>.Failure(new Error("database_error", exception.Message));
        }
    }

    public async Task<Result<TenantResponse>> PatchTenantAsync(
        Guid tenantId,
        string? name,
        string? status,
        JsonElement? properties,
        CancellationToken cancellationToken)
    {
        try
        {
            const string sql = """
                update app.tenant
                set
                  name = coalesce(@name, name),
                  status = coalesce(cast(@status as app.record_status), status),
                  properties = coalesce(@properties, properties),
                  deleted_at = case
                    when coalesce(cast(@status as app.record_status), status) = 'deleted' then coalesce(deleted_at, now())
                    else null
                  end
                where id = @tenantId
                returning
                  id,
                  name,
                  status::text,
                  properties,
                  created_at,
                  updated_at,
                  deleted_at
                """;

            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("tenantId", tenantId);
            command.Parameters.AddWithValue("name", (object?)name ?? DBNull.Value);
            command.Parameters.AddWithValue("status", (object?)status ?? DBNull.Value);
            command.Parameters.Add("properties", NpgsqlDbType.Jsonb).Value =
                properties.HasValue ? properties.Value.GetRawText() : DBNull.Value;

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
            {
                return Result<TenantResponse>.Failure(Error.NotFound("Tenant was not found."));
            }

            return Result<TenantResponse>.Success(ReadTenant(reader));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Tenant patch failed. TenantId: {TenantId}", tenantId);
            return Result<TenantResponse>.Failure(new Error("database_error", exception.Message));
        }
    }

    public async Task<Result<bool>> DeleteTenantAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        try
        {
            const string sql = """
                update app.tenant
                set
                  status = 'deleted',
                  deleted_at = coalesce(deleted_at, now())
                where id = @tenantId
                  and status <> 'deleted'
                """;

            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("tenantId", tenantId);

            var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);

            return affectedRows == 0
                ? Result<bool>.Failure(Error.NotFound("Tenant was not found."))
                : Result<bool>.Success(true);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Tenant delete failed. TenantId: {TenantId}", tenantId);
            return Result<bool>.Failure(new Error("database_error", exception.Message));
        }
    }

    private async Task<Result<TenantResponse>> ExecuteTenantMutationAsync(
        string sql,
        Guid tenantId,
        string name,
        string status,
        JsonElement properties,
        CancellationToken cancellationToken)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("tenantId", tenantId);
        command.Parameters.AddWithValue("name", name);
        command.Parameters.AddWithValue("status", status);
        command.Parameters.Add("properties", NpgsqlDbType.Jsonb).Value = properties.GetRawText();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return Result<TenantResponse>.Failure(Error.NotFound("Tenant was not found."));
        }

        return Result<TenantResponse>.Success(ReadTenant(reader));
    }

    private async Task<IReadOnlyList<TenantResponse>> QueryTenantsAsync(string sql, CancellationToken cancellationToken)
    {
        var tenants = new List<TenantResponse>();

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            tenants.Add(ReadTenant(reader));
        }

        return tenants;
    }

    private static TenantResponse ReadTenant(NpgsqlDataReader reader)
    {
        return new TenantResponse(
            reader.GetGuid(0).ToString("D"),
            reader.GetString(1),
            reader.GetString(2),
            reader.IsDBNull(3) ? EmptyJsonObject : JsonDocument.Parse(reader.GetFieldValue<string>(3)).RootElement.Clone(),
            reader.GetFieldValue<DateTimeOffset>(4),
            reader.GetFieldValue<DateTimeOffset>(5),
            reader.IsDBNull(6) ? null : reader.GetFieldValue<DateTimeOffset>(6));
    }
}
