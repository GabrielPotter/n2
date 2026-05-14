using Npgsql;

namespace Common;

public interface IPostgreSqlConnectionFactory
{
    NpgsqlConnection CreateConnection();
}

public sealed class PostgreSqlConnectionFactory : IPostgreSqlConnectionFactory
{
    private readonly string _connectionString;

    public PostgreSqlConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public NpgsqlConnection CreateConnection() => new(_connectionString);
}
