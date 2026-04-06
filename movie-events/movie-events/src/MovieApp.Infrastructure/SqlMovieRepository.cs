// <copyright file="SqlMovieRepository.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

using Microsoft.Data.SqlClient;

using MovieApp.Core.Models.Movie;
using MovieApp.Core.Repositories;

namespace MovieApp.Infrastructure;

/// <summary>
/// SQL Server-backed repository for managing movies and related entities
/// (genres, actors, directors).
/// </summary>
public sealed class SqlMovieRepository : IMovieRepository
{
    private readonly string _connectionString;


    /// <summary>
    /// Initialises a new instance of <see cref="SqlMovieRepository"/>.
    /// </summary>
    /// <param name="databaseOptions">Database connection options.</param>
    public SqlMovieRepository(DatabaseOptions databaseOptions)
    {
        ArgumentNullException.ThrowIfNull(databaseOptions);
        _connectionString = databaseOptions.ConnectionString;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Genre>> GetGenresAsync(CancellationToken cancellationToken = default)
    {
        const string sqlStringCommand = "SELECT Id, Name FROM dbo.Genres ORDER BY Name";

        List<Genre> genres = new List<Genre>();

        await using SqlConnection sqlConnection = new SqlConnection(_connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(sqlStringCommand, sqlConnection);
        await using SqlDataReader sqlDataReader = await sqlCommand.ExecuteReaderAsync(cancellationToken);

        while (await sqlDataReader.ReadAsync(cancellationToken))
        {
            genres.Add(new Genre
            {
                Id   = sqlDataReader.GetInt32(0),
                Name = sqlDataReader.GetString(1)
            });
        }

        return genres;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Actor>> GetActorsAsync(CancellationToken cancellationToken = default)
    {
        const string sqlStringCommand = "SELECT Id, Name FROM dbo.Actors ORDER BY Name";

        List<Actor> actors = new List<Actor>();

        await using SqlConnection sqlConnection = new SqlConnection(_connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(sqlStringCommand, sqlConnection);
        await using SqlDataReader sqlDataReader = await sqlCommand.ExecuteReaderAsync(cancellationToken);

        while (await sqlDataReader.ReadAsync(cancellationToken))
        {
            actors.Add(new Actor
            {
                Id   = sqlDataReader.GetInt32(0),
                Name = sqlDataReader.GetString(1)
            });
        }

        return actors;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Director>> GetDirectorsAsync(CancellationToken cancellationToken = default)
    {
        const string sqlStringCommand = "SELECT Id, Name FROM dbo.Directors ORDER BY Name";

        List<Director> directors = new List<Director>();

        await using SqlConnection sqlConnection = new SqlConnection(_connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(sqlStringCommand, sqlConnection);
        await using SqlDataReader sqlDataReader = await sqlCommand.ExecuteReaderAsync(cancellationToken);

        while (await sqlDataReader.ReadAsync(cancellationToken))
        {
            directors.Add(new Director
            {
                Id   = sqlDataReader.GetInt32(0),
                Name = sqlDataReader.GetString(1)
            });
        }

        return directors;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Movie>> FindMoviesByCriteriaAsync(int genreId, int actorId, int directorId, CancellationToken cancellationToken = default)
    {
        const string sqlStringCommand = @"
            SELECT DISTINCT m.Id, m.Title, m.Description, m.ReleaseYear, m.DurationMinutes
            FROM dbo.Movies m
            INNER JOIN dbo.MovieGenres mg ON m.Id = mg.MovieId
            INNER JOIN dbo.MovieActors ma ON m.Id = ma.MovieId
            INNER JOIN dbo.MovieDirectors md ON m.Id = md.MovieId
            WHERE mg.GenreId = @genreId
              AND ma.ActorId = @actorId
              AND md.DirectorId = @directorId
            ORDER BY m.Title";

        List<Movie> movies = new List<Movie>();

        await using SqlConnection sqlConnection = new SqlConnection(_connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(sqlStringCommand, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@genreId", genreId);
        sqlCommand.Parameters.AddWithValue("@actorId", actorId);
        sqlCommand.Parameters.AddWithValue("@directorId", directorId);

        await using var sqlDataReader = await sqlCommand.ExecuteReaderAsync(cancellationToken);

        while (await sqlDataReader.ReadAsync(cancellationToken))
        {
            movies.Add(new Movie
            {
                Id              = sqlDataReader.GetInt32(0),
                Title           = sqlDataReader.GetString(1),
                Description     = sqlDataReader.IsDBNull(2) ? string.Empty : sqlDataReader.GetString(2),
                ReleaseYear     = sqlDataReader.IsDBNull(3) ? 0 : sqlDataReader.GetInt32(3),
                DurationMinutes = sqlDataReader.IsDBNull(4) ? 0 : sqlDataReader.GetInt32(4),
                Genres          = [],
                Actors          = [],
                Directors       = []
            });
        }

        return movies;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Movie>> FindMoviesByAnyCriteriaAsync(int genreId, int actorId, int directorId, CancellationToken cancellationToken = default)
    {
        const string sqlStringCommand = @"
            SELECT DISTINCT m.Id, m.Title, m.Description, m.ReleaseYear, m.DurationMinutes
            FROM dbo.Movies m
            LEFT JOIN dbo.MovieGenres mg ON m.Id = mg.MovieId AND mg.GenreId = @genreId
            LEFT JOIN dbo.MovieActors ma ON m.Id = ma.MovieId AND ma.ActorId = @actorId
            LEFT JOIN dbo.MovieDirectors md ON m.Id = md.MovieId AND md.DirectorId = @directorId
            WHERE mg.GenreId IS NOT NULL
               OR ma.ActorId IS NOT NULL
               OR md.DirectorId IS NOT NULL
            ORDER BY m.Title";

        List<Movie> movies = new List<Movie>();

        await using SqlConnection sqlConnection = new SqlConnection(_connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(sqlStringCommand, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@genreId", genreId);
        sqlCommand.Parameters.AddWithValue("@actorId", actorId);
        sqlCommand.Parameters.AddWithValue("@directorId", directorId);

        await using SqlDataReader sqlDataReader = await sqlCommand.ExecuteReaderAsync(cancellationToken);

        while (await sqlDataReader.ReadAsync(cancellationToken))
        {
            movies.Add(new Movie
            {
                Id              = sqlDataReader.GetInt32(0),
                Title           = sqlDataReader.GetString(1),
                Description     = sqlDataReader.IsDBNull(2) ? string.Empty : sqlDataReader.GetString(2),
                ReleaseYear     = sqlDataReader.IsDBNull(3) ? 0 : sqlDataReader.GetInt32(3),
                DurationMinutes = sqlDataReader.IsDBNull(4) ? 0 : sqlDataReader.GetInt32(4),
                Genres          = [],
                Actors          = [],
                Directors       = []
            });
        }

        return movies;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<int>> FindScreeningEventIdsForMovieAsync(int movieId, CancellationToken cancellationToken = default)
    {
        const string sqlStringCommand = @"
            SELECT s.EventId
            FROM dbo.Screenings s
            INNER JOIN dbo.Events e ON s.EventId = e.Id
            WHERE s.MovieId = @movieId
              AND e.EventDateTime > GETUTCDATE()";

        List<int> eventIds = new List<int>();

        await using SqlConnection sqlConnection = new SqlConnection(_connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(sqlStringCommand, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@movieId", movieId);

        await using SqlDataReader sqlDataReader = await sqlCommand.ExecuteReaderAsync(cancellationToken);

        while (await sqlDataReader.ReadAsync(cancellationToken))
        {
            eventIds.Add(sqlDataReader.GetInt32(0));
        }

        return eventIds;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ReelCombination>> GetValidReelCombinationsAsync(CancellationToken cancellationToken = default)
    {
        const string sqlStringCommand = @"
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

        List<ReelCombination> combinations = new List<ReelCombination>();

        await using SqlConnection sqlConnection = new SqlConnection(_connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(sqlStringCommand, sqlConnection);
        await using SqlDataReader sqlDataReader = await sqlCommand.ExecuteReaderAsync(cancellationToken);

        while (await sqlDataReader.ReadAsync(cancellationToken))
        {
            combinations.Add(new ReelCombination
            {
                Genre    = new Genre { Id = sqlDataReader.GetInt32(0), Name = sqlDataReader.GetString(1) },
                Actor    = new Actor { Id = sqlDataReader.GetInt32(2), Name = sqlDataReader.GetString(3) },
                Director = new Director { Id = sqlDataReader.GetInt32(4), Name = sqlDataReader.GetString(5) }
            });
        }

        return combinations;
    }
}
