using Xunit;

namespace MovieApp.Infrastructure.Tests;

public sealed class EventSqlQueriesTests
{
    [Fact]
    public void EventProjection_IncludesEventTypeColumn()
    {
        var queryFile = ReadRepoFile("src", "MovieApp.Infrastructure", "EventSqlQueries.cs");

        Assert.Contains("public const string Projection", queryFile);
        Assert.Contains("EventType", queryFile);
        Assert.Contains("WHERE Id = @id;", queryFile);
    }

    [Fact]
    public void EventInsert_StoresEventTypeColumn()
    {
        var queryFile = ReadRepoFile("src", "MovieApp.Infrastructure", "EventSqlQueries.cs");
        var repositoryFile = ReadRepoFile("src", "MovieApp.Infrastructure", "SqlEventRepository.cs");

        Assert.Contains("public const string Insert", queryFile);
        Assert.Contains("TicketPrice, EventType, HistoricalRating", queryFile);
        Assert.Contains("@ticketPrice, @eventType, @historicalRating", queryFile);
        Assert.Contains("sqlCommand.Parameters.AddWithValue(\"@eventType\", @event.EventType);", repositoryFile);
    }

    [Fact]
    public void EventInsert_ValidatesAndConvertsTheReturnedIdentityValue()
    {
        var repositoryFile = ReadRepoFile("src", "MovieApp.Infrastructure", "SqlEventRepository.cs");

        Assert.Contains("if (result is null or DBNull)", repositoryFile);
        Assert.Contains("Expected the event insert to return the new identity value.", repositoryFile);
        Assert.Contains("return Convert.ToInt32(result);", repositoryFile);
        Assert.DoesNotContain("return (int)result!;", repositoryFile);
    }

    [Fact]
    public void SqlEventRepository_UsesSharedQueryDefinitions()
    {
        var repositoryFile = ReadRepoFile("src", "MovieApp.Infrastructure", "SqlEventRepository.cs");

        Assert.Contains("new SqlCommand(EventSqlQueries.SelectAll, connection)", repositoryFile);
        Assert.Contains("new SqlCommand(EventSqlQueries.SelectByType, connection)", repositoryFile);
        Assert.Contains("new SqlCommand(EventSqlQueries.SelectById, connection)", repositoryFile);
        Assert.Contains("new SqlCommand(EventSqlQueries.Insert, connection)", repositoryFile);
        Assert.Contains("MapEvent", repositoryFile);
    }

    private static string ReadRepoFile(params string[] pathSegments)
    {
        var currentDirectory = new DirectoryInfo(AppContext.BaseDirectory);

        while (currentDirectory is not null
            && !File.Exists(Path.Combine(currentDirectory.FullName, "MovieApp.sln")))
        {
            currentDirectory = currentDirectory.Parent;
        }

        Assert.NotNull(currentDirectory);

        var filePath = Path.Combine([currentDirectory!.FullName, .. pathSegments]);
        return File.ReadAllText(filePath);
    }

    [Fact]
    public void BaseEventMockData_IsGuardedAgainstDuplicateSeedData()
    {
        var seedScript = ReadRepoFile("src", "MovieApp.Infrastructure", "Database", "MockData", "002-seed-base-events.sql");

        Assert.Contains("IF NOT EXISTS", seedScript);
        Assert.Contains("WHERE Title = 'Cannes Winner Screening'", seedScript);
    }

    [Fact]
    public void ExtraEventMockData_AddsAdditionalEventsPerEventType()
    {
        var seedScript = ReadRepoFile("src", "MovieApp.Infrastructure", "Database", "MockData", "007-seed-extra-users-and-events.sql");

        Assert.Contains("Sunset Classics Screening", seedScript);
        Assert.Contains("Midnight Horror Marathon", seedScript);
        Assert.Contains("Directors Roundtable Live", seedScript);
        Assert.Contains("Award Winners Spotlight", seedScript);
    }

    [Fact]
    public void LegacySeedScripts_AreDocumentedAsMovedToMockData()
    {
        var eventsStub = ReadRepoFile("src", "MovieApp.Infrastructure", "Database", "Scripts", "012-seed-events.sql");
        //var marathonsStub = ReadRepoFile("src", "MovieApp.Infrastructure", "Database", "Scripts", "020-seed-marathons.sql");

        Assert.Contains("MockData", eventsStub);
        //Assert.Contains("MockData", marathonsStub);
        Assert.DoesNotContain("INSERT INTO dbo.Events", eventsStub);
        //Assert.DoesNotContain("INSERT INTO dbo.Marathons", marathonsStub);
    }

    [Fact]
    public void SqlMarathonRepository_UsesDatabaseOptionsLikeOtherSqlRepositories()
    {
        var repositoryFile = ReadRepoFile("src", "MovieApp.Infrastructure", "SqlMarathonRepository.cs");

        Assert.Contains("public SqlMarathonRepository(DatabaseOptions databaseOptions)", repositoryFile);
        Assert.Contains("ArgumentNullException.ThrowIfNull(databaseOptions);", repositoryFile);
        Assert.Contains("this._connectionString = databaseOptions.ConnectionString;", repositoryFile);
        Assert.Contains("await using SqlConnection sqlConnection = new SqlConnection(this._connectionString);", repositoryFile);
        Assert.Contains("await using SqlCommand sqlCommand = new SqlCommand(", repositoryFile);
        Assert.DoesNotContain("public SqlMarathonRepository(string connectionString)", repositoryFile);
    }
}
