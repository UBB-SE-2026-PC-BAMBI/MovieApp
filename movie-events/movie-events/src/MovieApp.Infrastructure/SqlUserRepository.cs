using Microsoft.Data.SqlClient;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Infrastructure;

public sealed class SqlUserRepository : IUserRepository
{
    private readonly string _connectionString;

    public SqlUserRepository(DatabaseOptions databaseOptions)
    {
        _connectionString = databaseOptions.ConnectionString;
    }

    public async Task<User?> FindByAuthIdentityAsync(string authProvider, string authSubject, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT Id, AuthProvider, AuthSubject, Username
            FROM dbo.Users
            WHERE AuthProvider = @authProvider
              AND AuthSubject = @authSubject;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@authProvider", authProvider);
        command.Parameters.AddWithValue("@authSubject", authSubject);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new User
        {
            Id = reader.GetInt32(0),
            AuthProvider = reader.GetString(1),
            AuthSubject = reader.GetString(2),
            Username = reader.GetString(3),
        };
    }
}
