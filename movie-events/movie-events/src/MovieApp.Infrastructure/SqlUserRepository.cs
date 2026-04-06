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
        const string sqlStringCommand = """
            SELECT Id, AuthProvider, AuthSubject, Username
            FROM dbo.Users
            WHERE AuthProvider = @authProvider
              AND AuthSubject = @authSubject;
            """;

        await using SqlConnection sqlConnection = new SqlConnection(_connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(sqlStringCommand, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@authProvider", authProvider);
        sqlCommand.Parameters.AddWithValue("@authSubject", authSubject);

        await using SqlDataReader sqlDataReader = await sqlCommand.ExecuteReaderAsync(cancellationToken);
        if (!await sqlDataReader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new User
        {
            Id           = sqlDataReader.GetInt32(0),
            AuthProvider = sqlDataReader.GetString(1),
            AuthSubject  = sqlDataReader.GetString(2),
            Username     = sqlDataReader.GetString(3),
        };
    }
}
