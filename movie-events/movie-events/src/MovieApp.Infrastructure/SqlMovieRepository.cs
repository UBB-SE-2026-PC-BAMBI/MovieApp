using Microsoft.Data.SqlClient;
using MovieApp.Core.Models.Movie;
using MovieApp.Core.Repositories;

namespace MovieApp.Infrastructure;

/// <summary>
/// SQL Server-backed repository for managing movies and related entities (genres, actors, directors).
/// </summary>
public sealed class SqlMovieRepository : IMovieRepository
{
    private readonly string _connectionString;

    public SqlMovieRepository(DatabaseOptions databaseOptions)
    {
        ArgumentNullException.ThrowIfNull(databaseOptions);
        _connectionString = databaseOptions.ConnectionString;
    }

    public async Task<IReadOnlyList<Genre>> GetGenresAsync(CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT Id, Name FROM dbo.Genres ORDER BY Name";

        var genres = new List<Genre>();

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            genres.Add(new Genre
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1)
            });
        }

        return genres;
    }

    public async Task<IReadOnlyList<Actor>> GetActorsAsync(CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT Id, Name FROM dbo.Actors ORDER BY Name";

        var actors = new List<Actor>();

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            actors.Add(new Actor
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1)
            });
        }

        return actors;
    }

    public async Task<IReadOnlyList<Director>> GetDirectorsAsync(CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT Id, Name FROM dbo.Directors ORDER BY Name";

        var directors = new List<Director>();

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            directors.Add(new Director
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1)
            });
        }

        return directors;
    }

    public async Task<IReadOnlyList<Movie>> FindMoviesByCriteriaAsync(int genreId, int actorId, int directorId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT DISTINCT m.Id, m.Title, m.Description, m.ReleaseYear, m.DurationMinutes
            FROM dbo.Movies m
            INNER JOIN dbo.MovieGenres mg ON m.Id = mg.MovieId
            INNER JOIN dbo.MovieActors ma ON m.Id = ma.MovieId
            INNER JOIN dbo.MovieDirectors md ON m.Id = md.MovieId
            WHERE mg.GenreId = @genreId
              AND ma.ActorId = @actorId
              AND md.DirectorId = @directorId
            ORDER BY m.Title";

        var movies = new List<Movie>();

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@genreId", genreId);
        command.Parameters.AddWithValue("@actorId", actorId);
        command.Parameters.AddWithValue("@directorId", directorId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            movies.Add(new Movie
            {
                Id = reader.GetInt32(0),
                Title = reader.GetString(1),
                Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                ReleaseYear = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                DurationMinutes = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                Genres = [],
                Actors = [],
                Directors = []
            });
        }

        return movies;
    }

    public async Task<IReadOnlyList<Movie>> FindMoviesByAnyCriteriaAsync(int genreId, int actorId, int directorId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT DISTINCT m.Id, m.Title, m.Description, m.ReleaseYear, m.DurationMinutes
            FROM dbo.Movies m
            LEFT JOIN dbo.MovieGenres mg ON m.Id = mg.MovieId AND mg.GenreId = @genreId
            LEFT JOIN dbo.MovieActors ma ON m.Id = ma.MovieId AND ma.ActorId = @actorId
            LEFT JOIN dbo.MovieDirectors md ON m.Id = md.MovieId AND md.DirectorId = @directorId
            WHERE mg.GenreId IS NOT NULL
               OR ma.ActorId IS NOT NULL
               OR md.DirectorId IS NOT NULL
            ORDER BY m.Title";

        var movies = new List<Movie>();

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@genreId", genreId);
        command.Parameters.AddWithValue("@actorId", actorId);
        command.Parameters.AddWithValue("@directorId", directorId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            movies.Add(new Movie
            {
                Id = reader.GetInt32(0),
                Title = reader.GetString(1),
                Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                ReleaseYear = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                DurationMinutes = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                Genres = [],
                Actors = [],
                Directors = []
            });
        }

        return movies;
    }

    public async Task<IReadOnlyList<int>> FindScreeningEventIdsForMovieAsync(int movieId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT s.EventId
            FROM dbo.Screenings s
            INNER JOIN dbo.Events e ON s.EventId = e.Id
            WHERE s.MovieId = @movieId
              AND e.EventDateTime > GETUTCDATE()";

        var eventIds = new List<int>();

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@movieId", movieId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            eventIds.Add(reader.GetInt32(0));
        }

        return eventIds;
    }

    public async Task<IReadOnlyList<ReelCombination>> GetValidReelCombinationsAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT DISTINCT
                g.Id AS GenreId, g.Name AS GenreName,
                a.Id AS ActorId, a.Name AS ActorName,
                d.Id AS DirectorId, d.Name AS DirectorName
            FROM dbo.Movies m
            INNER JOIN dbo.MovieGenres mg ON m.Id = mg.MovieId
            INNER JOIN dbo.Genres g ON mg.GenreId = g.Id
            INNER JOIN dbo.MovieActors ma ON m.Id = ma.MovieId
            INNER JOIN dbo.Actors a ON ma.ActorId = a.Id
            INNER JOIN dbo.MovieDirectors md ON m.Id = md.MovieId
            INNER JOIN dbo.Directors d ON md.DirectorId = d.Id
            INNER JOIN dbo.Screenings s ON m.Id = s.MovieId
            INNER JOIN dbo.Events e ON s.EventId = e.Id
            WHERE e.EventDateTime > GETUTCDATE()";

        var combinations = new List<ReelCombination>();

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            combinations.Add(new ReelCombination
            {
                Genre = new Genre { Id = reader.GetInt32(0), Name = reader.GetString(1) },
                Actor = new Actor { Id = reader.GetInt32(2), Name = reader.GetString(3) },
                Director = new Director { Id = reader.GetInt32(4), Name = reader.GetString(5) }
            });
        }

        return combinations;
    }
}
