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
        await Task.CompletedTask;
        return Result<InternalStatusResponse>.Success(new InternalStatusResponse("system", RuntimeStatus.CreateDetails()));
    }

    public async Task<Result<IReadOnlyList<TenantResponse>>> GetTenantsAsync(CancellationToken cancellationToken)
    {
        try
        {
            const string sql = """
                select
                  t.tenant_id,
                  t.tenant_name,
                  t.tenant_status::text,
                  t.properties,
                  t.created_at,
                  t.updated_at,
                  t.deleted_at
                from app.tenant t
                where t.tenant_status <> 'deleted'
                order by t.tenant_name, t.tenant_id
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
                  t.tenant_id,
                  t.tenant_name,
                  t.tenant_status::text,
                  t.properties,
                  t.created_at,
                  t.updated_at,
                  t.deleted_at
                from app.tenant t
                where t.tenant_id = @tenantId
                  and t.tenant_status <> 'deleted'
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

    public async Task<Result<TenantLookupResponse>> GetTenantByNameAsync(string tenantName, CancellationToken cancellationToken)
    {
        try
        {
            const string sql = """
                select
                  t.tenant_id,
                  t.tenant_name
                from app.tenant t
                where t.tenant_name = @tenantName
                  and t.tenant_status <> 'deleted'
                """;

            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("tenantName", tenantName);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
            {
                return Result<TenantLookupResponse>.Failure(Error.NotFound("Tenant was not found."));
            }

            return Result<TenantLookupResponse>.Success(
                new TenantLookupResponse(
                    reader.GetGuid(0).ToString("D"),
                    reader.GetString(1)));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Tenant get by name query failed. TenantName: {TenantName}", tenantName);
            return Result<TenantLookupResponse>.Failure(new Error("database_error", exception.Message));
        }
    }

    public async Task<Result<TenantResponse>> CreateTenantAsync(
        Guid tenantId,
        string tenantName,
        string tenantStatus,
        JsonElement properties,
        CancellationToken cancellationToken)
    {
        try
        {
            const string sql = """
                insert into app.tenant (
                  tenant_id,
                  tenant_name,
                  tenant_status,
                  properties,
                  deleted_at
                )
                values (
                  @tenantId,
                  @tenantName,
                  cast(@tenantStatus as app.record_status),
                  @properties,
                  case when @tenantStatus = 'deleted' then now() else null end
                )
                returning
                  tenant_id,
                  tenant_name,
                  tenant_status::text,
                  properties,
                  created_at,
                  updated_at,
                  deleted_at
                """;

            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("tenantId", tenantId);
            command.Parameters.AddWithValue("tenantName", tenantName);
            command.Parameters.AddWithValue("tenantStatus", tenantStatus);
            command.Parameters.Add("properties", NpgsqlDbType.Jsonb).Value = properties.GetRawText();

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            await reader.ReadAsync(cancellationToken);

            return Result<TenantResponse>.Success(ReadTenant(reader));
        }
        catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            _logger.LogWarning(exception, "Tenant create conflict. TenantId: {TenantId}", tenantId);
            return Result<TenantResponse>.Failure(Error.Conflict("Tenant name already exists."));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Tenant create failed. TenantId: {TenantId}", tenantId);
            return Result<TenantResponse>.Failure(new Error("database_error", exception.Message));
        }
    }

    public async Task<Result<TenantResponse>> UpdateTenantAsync(
        Guid tenantId,
        string tenantName,
        string tenantStatus,
        JsonElement properties,
        CancellationToken cancellationToken)
    {
        try
        {
            const string sql = """
                update app.tenant
                set
                  tenant_name = @tenantName,
                  tenant_status = cast(@tenantStatus as app.record_status),
                  properties = @properties,
                  deleted_at = case
                    when @tenantStatus = 'deleted' then coalesce(deleted_at, now())
                    else null
                  end
                where tenant_id = @tenantId
                returning
                  tenant_id,
                  tenant_name,
                  tenant_status::text,
                  properties,
                  created_at,
                  updated_at,
                  deleted_at
                """;

            return await ExecuteTenantMutationAsync(
                sql,
                tenantId,
                tenantName,
                tenantStatus,
                properties,
                cancellationToken);
        }
        catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            _logger.LogWarning(exception, "Tenant update conflict. TenantId: {TenantId}", tenantId);
            return Result<TenantResponse>.Failure(Error.Conflict("Tenant name already exists."));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Tenant update failed. TenantId: {TenantId}", tenantId);
            return Result<TenantResponse>.Failure(new Error("database_error", exception.Message));
        }
    }

    public async Task<Result<TenantResponse>> PatchTenantAsync(
        Guid tenantId,
        string? tenantName,
        string? tenantStatus,
        JsonElement? properties,
        CancellationToken cancellationToken)
    {
        try
        {
            const string sql = """
                update app.tenant
                set
                  tenant_name = coalesce(@tenantName, tenant_name),
                  tenant_status = coalesce(cast(@tenantStatus as app.record_status), tenant_status),
                  properties = coalesce(@properties, properties),
                  deleted_at = case
                    when coalesce(cast(@tenantStatus as app.record_status), tenant_status) = 'deleted' then coalesce(deleted_at, now())
                    else null
                  end
                where tenant_id = @tenantId
                returning
                  tenant_id,
                  tenant_name,
                  tenant_status::text,
                  properties,
                  created_at,
                  updated_at,
                  deleted_at
                """;

            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("tenantId", tenantId);
            command.Parameters.AddWithValue("tenantName", (object?)tenantName ?? DBNull.Value);
            command.Parameters.AddWithValue("tenantStatus", (object?)tenantStatus ?? DBNull.Value);
            command.Parameters.Add("properties", NpgsqlDbType.Jsonb).Value =
                properties.HasValue ? properties.Value.GetRawText() : DBNull.Value;

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
            {
                return Result<TenantResponse>.Failure(Error.NotFound("Tenant was not found."));
            }

            return Result<TenantResponse>.Success(ReadTenant(reader));
        }
        catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            _logger.LogWarning(exception, "Tenant patch conflict. TenantId: {TenantId}", tenantId);
            return Result<TenantResponse>.Failure(Error.Conflict("Tenant name already exists."));
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
                  tenant_status = 'deleted',
                  deleted_at = coalesce(deleted_at, now())
                where tenant_id = @tenantId
                  and tenant_status <> 'deleted'
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
        string tenantName,
        string tenantStatus,
        JsonElement properties,
        CancellationToken cancellationToken)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("tenantId", tenantId);
        command.Parameters.AddWithValue("tenantName", tenantName);
        command.Parameters.AddWithValue("tenantStatus", tenantStatus);
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
