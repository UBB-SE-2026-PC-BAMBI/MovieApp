namespace MovieApp.Infrastructure;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

/// <summary>
/// A SQL Server-backed repository for managing user accounts via ADO.NET.
/// </summary>
public sealed class SqlUserRepository : IUserRepository
{
    private readonly string connectionString;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlUserRepository"/> class.
    /// </summary>
    /// <param name="databaseOptions">The database options containing the SQL connection string.</param>
    public SqlUserRepository(DatabaseOptions databaseOptions)
    {
        this.connectionString = databaseOptions.ConnectionString;
    }

    /// <summary>
    /// Asynchronously searches for a user based on their external authentication identity.
    /// </summary>
    /// <param name="authProvider">The name of the external authentication provider (e.g., "Google", "Auth0").</param>
    /// <param name="authSubject">The unique subject identifier (sub claim) issued by the authentication provider.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>The matching <see cref="User"/> if found; otherwise, <c>null</c>.</returns>
    public async Task<User?> FindByAuthIdentityAsync(string authProvider, string authSubject, CancellationToken cancellationToken = default)
    {
        const string sqlStringCommand = """
            SELECT Id, AuthProvider, AuthSubject, Username
            FROM dbo.Users
            WHERE AuthProvider = @authProvider
              AND AuthSubject = @authSubject;
            """;

        await using SqlConnection sqlConnection = new SqlConnection(this.connectionString);
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
            Id = sqlDataReader.GetInt32(0),
            AuthProvider = sqlDataReader.GetString(1),
            AuthSubject = sqlDataReader.GetString(2),
            Username = sqlDataReader.GetString(3),
        };
    }
}