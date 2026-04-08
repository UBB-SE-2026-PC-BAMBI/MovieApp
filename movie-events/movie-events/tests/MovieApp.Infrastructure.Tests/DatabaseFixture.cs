// <copyright file="DatabaseFixture.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Infrastructure.Tests;

using Microsoft.Data.SqlClient;
using System;
using Xunit;

/// <summary>
/// xUnit collection definition that ensures the database fixture is shared
/// across all integration test classes.
/// </summary>
[CollectionDefinition("Database")]
public sealed class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
}

/// <summary>
/// Creates a fresh SQL Server (LocalDB) test database before all tests and drops it afterwards.
/// The schema mirrors the production schema defined in the Scripts/ folder.
/// </summary>
public sealed class DatabaseFixture : IDisposable
{
    private static readonly string DatabaseName =
        $"MovieAppIntTest_{Guid.NewGuid():N}";

    private const string MasterConnectionString =
        @"Server=(localdb)\MSSQLLocalDB;Database=master;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=30;";

    public string ConnectionString { get; }
    public DatabaseOptions DatabaseOptions { get; }

    public DatabaseFixture()
    {
        ConnectionString =
            $@"Server=(localdb)\MSSQLLocalDB;Database={DatabaseName};Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=30;";
        DatabaseOptions = new DatabaseOptions { ConnectionString = ConnectionString };

        CreateDatabase();
        CreateSchema();
    }

    // ── Helpers available to test classes ─────────────────────────────────────

    public int InsertUser(string username, string provider = "test", string? subject = null)
    {
        subject ??= Guid.NewGuid().ToString("N");
        using var conn = Open();
        using var cmd = new SqlCommand(
            "INSERT INTO dbo.Users (AuthProvider, AuthSubject, Username) VALUES (@p, @s, @u); SELECT CAST(SCOPE_IDENTITY() AS INT);",
            conn);
        cmd.Parameters.AddWithValue("@p", provider);
        cmd.Parameters.AddWithValue("@s", subject);
        cmd.Parameters.AddWithValue("@u", username);
        return (int)cmd.ExecuteScalar()!;
    }

    public int InsertEvent(int creatorUserId, string title, string eventType = "Screening",
        decimal ticketPrice = 10m, int maxCapacity = 50, int enrollment = 0)
    {
        using var conn = Open();
        using var cmd = new SqlCommand(
            @"INSERT INTO dbo.Events (Title, Description, PosterUrl, EventDateTime, LocationReference,
                TicketPrice, EventType, HistoricalRating, MaxCapacity, CurrentEnrollment, CreatorUserId)
              VALUES (@title, NULL, '', DATEADD(day,1,GETDATE()), 'Hall A',
                @price, @type, 0.0, @cap, @enroll, @creator);
              SELECT CAST(SCOPE_IDENTITY() AS INT);",
            conn);
        cmd.Parameters.AddWithValue("@title", title);
        cmd.Parameters.AddWithValue("@price", ticketPrice);
        cmd.Parameters.AddWithValue("@type", eventType);
        cmd.Parameters.AddWithValue("@cap", maxCapacity);
        cmd.Parameters.AddWithValue("@enroll", enrollment);
        cmd.Parameters.AddWithValue("@creator", creatorUserId);
        return (int)cmd.ExecuteScalar()!;
    }

    public int InsertMovie(string title = "Test Movie", int releaseYear = 2020, int durationMinutes = 120)
    {
        using var conn = Open();
        using var cmd = new SqlCommand(
            "INSERT INTO dbo.Movies (Title, Description, ReleaseYear, DurationMinutes) VALUES (@t, NULL, @y, @d); SELECT CAST(SCOPE_IDENTITY() AS INT);",
            conn);
        cmd.Parameters.AddWithValue("@t", title);
        cmd.Parameters.AddWithValue("@y", releaseYear);
        cmd.Parameters.AddWithValue("@d", durationMinutes);
        return (int)cmd.ExecuteScalar()!;
    }

    public int InsertGenre(string name)
    {
        using var conn = Open();
        using var cmd = new SqlCommand(
            "INSERT INTO dbo.Genres (Name) VALUES (@n); SELECT CAST(SCOPE_IDENTITY() AS INT);",
            conn);
        cmd.Parameters.AddWithValue("@n", name);
        return (int)cmd.ExecuteScalar()!;
    }

    public int InsertActor(string name)
    {
        using var conn = Open();
        using var cmd = new SqlCommand(
            "INSERT INTO dbo.Actors (Name) VALUES (@n); SELECT CAST(SCOPE_IDENTITY() AS INT);",
            conn);
        cmd.Parameters.AddWithValue("@n", name);
        return (int)cmd.ExecuteScalar()!;
    }

    public int InsertDirector(string name)
    {
        using var conn = Open();
        using var cmd = new SqlCommand(
            "INSERT INTO dbo.Directors (Name) VALUES (@n); SELECT CAST(SCOPE_IDENTITY() AS INT);",
            conn);
        cmd.Parameters.AddWithValue("@n", name);
        return (int)cmd.ExecuteScalar()!;
    }

    public void LinkMovieGenre(int movieId, int genreId)
    {
        using var conn = Open();
        using var cmd = new SqlCommand(
            "INSERT INTO dbo.MovieGenres (MovieId, GenreId) VALUES (@m, @g);", conn);
        cmd.Parameters.AddWithValue("@m", movieId);
        cmd.Parameters.AddWithValue("@g", genreId);
        cmd.ExecuteNonQuery();
    }

    public void LinkMovieActor(int movieId, int actorId)
    {
        using var conn = Open();
        using var cmd = new SqlCommand(
            "INSERT INTO dbo.MovieActors (MovieId, ActorId) VALUES (@m, @a);", conn);
        cmd.Parameters.AddWithValue("@m", movieId);
        cmd.Parameters.AddWithValue("@a", actorId);
        cmd.ExecuteNonQuery();
    }

    public void LinkMovieDirector(int movieId, int directorId)
    {
        using var conn = Open();
        using var cmd = new SqlCommand(
            "INSERT INTO dbo.MovieDirectors (MovieId, DirectorId) VALUES (@m, @d);", conn);
        cmd.Parameters.AddWithValue("@m", movieId);
        cmd.Parameters.AddWithValue("@d", directorId);
        cmd.ExecuteNonQuery();
    }

    public int InsertMarathon(string title, bool isActive = true, string? weekScoping = null,
        int? prerequisiteMarathonId = null)
    {
        using var conn = Open();
        using var cmd = new SqlCommand(
            @"INSERT INTO dbo.Marathons (Title, Description, PosterUrl, Theme, PrerequisiteMarathonId,
                IsActive, LastFeaturedDate, WeekScoping)
              VALUES (@t, NULL, '', NULL, @prereq, @active, NULL, @week);
              SELECT CAST(SCOPE_IDENTITY() AS INT);",
            conn);
        cmd.Parameters.AddWithValue("@t", title);
        cmd.Parameters.AddWithValue("@prereq", (object?)prerequisiteMarathonId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@active", isActive ? 1 : 0);
        cmd.Parameters.AddWithValue("@week", (object?)weekScoping ?? DBNull.Value);
        return (int)cmd.ExecuteScalar()!;
    }

    public int InsertUserSpins(int userId, int dailySpins = 5, int bonusSpins = 0, int loginStreak = 0)
    {
        using var conn = Open();
        using var cmd = new SqlCommand(
            @"INSERT INTO dbo.UserSpins (UserId, DailySpinsRemaining, BonusSpins,
                LastSlotSpinReset, LastTriviaSpinReset, LoginStreak, LastLoginDate, EventSpinRewardsToday)
              VALUES (@uid, @daily, @bonus, NULL, NULL, @streak, NULL, 0);
              SELECT CAST(@uid AS INT);",
            conn);
        cmd.Parameters.AddWithValue("@uid", userId);
        cmd.Parameters.AddWithValue("@daily", dailySpins);
        cmd.Parameters.AddWithValue("@bonus", bonusSpins);
        cmd.Parameters.AddWithValue("@streak", loginStreak);
        return (int)cmd.ExecuteScalar()!;
    }

    public void InsertAmbassadorProfile(int userId, string referralCode, int rewardBalance = 0)
    {
        using var conn = Open();
        using var cmd = new SqlCommand(
            "INSERT INTO dbo.AmbassadorProfile (UserId, referral_code, reward_balance) VALUES (@uid, @code, @bal);",
            conn);
        cmd.Parameters.AddWithValue("@uid", userId);
        cmd.Parameters.AddWithValue("@code", referralCode);
        cmd.Parameters.AddWithValue("@bal", rewardBalance);
        cmd.ExecuteNonQuery();
    }

    public void InsertReferralLog(int ambassadorId, int friendId, int eventId)
    {
        using var conn = Open();
        using var cmd = new SqlCommand(
            "INSERT INTO dbo.ReferralLog (AmbassadorID, FriendID, EventID) VALUES (@a, @f, @e);",
            conn);
        cmd.Parameters.AddWithValue("@a", ambassadorId);
        cmd.Parameters.AddWithValue("@f", friendId);
        cmd.Parameters.AddWithValue("@e", eventId);
        cmd.ExecuteNonQuery();
    }

    public void InsertScreening(int eventId, int movieId)
    {
        using var conn = Open();
        using var cmd = new SqlCommand(
            "INSERT INTO dbo.Screenings (EventId, MovieId, ScreeningTime) VALUES (@e, @m, DATEADD(day,1,GETDATE()));",
            conn);
        cmd.Parameters.AddWithValue("@e", eventId);
        cmd.Parameters.AddWithValue("@m", movieId);
        cmd.ExecuteNonQuery();
    }

    public SqlConnection Open()
    {
        var conn = new SqlConnection(ConnectionString);
        conn.Open();
        return conn;
    }

    // ── IDisposable ────────────────────────────────────────────────────────────

    public void Dispose()
    {
        try
        {
            using var conn = new SqlConnection(MasterConnectionString);
            conn.Open();
            using var cmd = new SqlCommand(
                $"IF DB_ID(N'{DatabaseName}') IS NOT NULL BEGIN " +
                $"ALTER DATABASE [{DatabaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; " +
                $"DROP DATABASE [{DatabaseName}]; END",
                conn);
            cmd.ExecuteNonQuery();
        }
        catch
        {
            // Best-effort cleanup
        }
    }

    // ── Schema creation ────────────────────────────────────────────────────────

    private void CreateDatabase()
    {
        using var conn = new SqlConnection(MasterConnectionString);
        conn.Open();
        using var cmd = new SqlCommand($"CREATE DATABASE [{DatabaseName}];", conn);
        cmd.ExecuteNonQuery();
    }

    private void CreateSchema()
    {
        using var conn = new SqlConnection(ConnectionString);
        conn.Open();
        RunBatch(conn, CreateUsersTableSql);
        RunBatch(conn, CreateMoviesAndRelatedTablesSql);
        RunBatch(conn, CreateEventsTableSql);
        RunBatch(conn, CreateParticipationsTableSql);
        RunBatch(conn, CreateFavoriteEventsTableSql);
        RunBatch(conn, CreateNotificationsTableSql);
        RunBatch(conn, CreateUserSpinsTableSql);
        RunBatch(conn, CreateUserMovieDiscountsTableSql);
        RunBatch(conn, CreateMarathonTablesSql);
        RunBatch(conn, CreateTriviaQuestionsTableSql);
        RunBatch(conn, CreateAmbassadorProfileTableSql);
        RunBatch(conn, CreateReferralLogTableSql);
        RunBatch(conn, CreateScreeningsTableSql);
        RunBatch(conn, CreateTriviaRewardsTableSql);
        RunBatch(conn, CreateUserEventAttendanceTableSql);
    }

    private static void RunBatch(SqlConnection conn, string sql)
    {
        using var cmd = new SqlCommand(sql, conn);
        cmd.CommandTimeout = 60;
        cmd.ExecuteNonQuery();
    }

    // ── DDL ───────────────────────────────────────────────────────────────────

    private const string CreateUsersTableSql = @"
        CREATE TABLE dbo.Users (
            Id          INT IDENTITY(1,1) NOT NULL,
            AuthProvider NVARCHAR(32)  NOT NULL,
            AuthSubject  NVARCHAR(128) NOT NULL,
            Username     NVARCHAR(64)  NOT NULL,
            CONSTRAINT PK_Users PRIMARY KEY CLUSTERED (Id),
            CONSTRAINT UQ_Users_Auth    UNIQUE (AuthProvider, AuthSubject),
            CONSTRAINT UQ_Users_Username UNIQUE (Username)
        );";

    private const string CreateMoviesAndRelatedTablesSql = @"
        CREATE TABLE dbo.Movies (
            Id              INT IDENTITY(1,1) NOT NULL,
            Title           NVARCHAR(255) NOT NULL,
            Description     NVARCHAR(MAX) NULL,
            ReleaseYear     INT NULL,
            DurationMinutes INT NULL,
            CONSTRAINT PK_Movies PRIMARY KEY (Id)
        );
        CREATE TABLE dbo.Genres (
            Id   INT IDENTITY(1,1) NOT NULL,
            Name NVARCHAR(100) NOT NULL,
            CONSTRAINT PK_Genres PRIMARY KEY (Id),
            CONSTRAINT UQ_Genres_Name UNIQUE (Name)
        );
        CREATE TABLE dbo.MovieGenres (
            MovieId INT NOT NULL,
            GenreId INT NOT NULL,
            CONSTRAINT PK_MovieGenres PRIMARY KEY (MovieId, GenreId),
            CONSTRAINT FK_MG_Movies FOREIGN KEY (MovieId) REFERENCES dbo.Movies(Id),
            CONSTRAINT FK_MG_Genres FOREIGN KEY (GenreId) REFERENCES dbo.Genres(Id)
        );
        CREATE TABLE dbo.Actors (
            Id   INT IDENTITY(1,1) NOT NULL,
            Name NVARCHAR(200) NOT NULL,
            CONSTRAINT PK_Actors PRIMARY KEY (Id),
            CONSTRAINT UQ_Actors_Name UNIQUE (Name)
        );
        CREATE TABLE dbo.MovieActors (
            MovieId INT NOT NULL,
            ActorId INT NOT NULL,
            CONSTRAINT PK_MovieActors PRIMARY KEY (MovieId, ActorId),
            CONSTRAINT FK_MA_Movies FOREIGN KEY (MovieId) REFERENCES dbo.Movies(Id),
            CONSTRAINT FK_MA_Actors FOREIGN KEY (ActorId) REFERENCES dbo.Actors(Id)
        );
        CREATE TABLE dbo.Directors (
            Id   INT IDENTITY(1,1) NOT NULL,
            Name NVARCHAR(200) NOT NULL,
            CONSTRAINT PK_Directors PRIMARY KEY (Id),
            CONSTRAINT UQ_Directors_Name UNIQUE (Name)
        );
        CREATE TABLE dbo.MovieDirectors (
            MovieId    INT NOT NULL,
            DirectorId INT NOT NULL,
            CONSTRAINT PK_MovieDirectors PRIMARY KEY (MovieId, DirectorId),
            CONSTRAINT FK_MD_Movies    FOREIGN KEY (MovieId)    REFERENCES dbo.Movies(Id),
            CONSTRAINT FK_MD_Directors FOREIGN KEY (DirectorId) REFERENCES dbo.Directors(Id)
        );";

    private const string CreateEventsTableSql = @"
        CREATE TABLE dbo.Events (
            Id                INT IDENTITY(1,1) NOT NULL,
            Title             NVARCHAR(200)  NOT NULL,
            Description       NVARCHAR(MAX)  NULL,
            PosterUrl         NVARCHAR(500)  NOT NULL CONSTRAINT DF_Ev_Poster DEFAULT '',
            EventDateTime     DATETIME2      NOT NULL,
            LocationReference NVARCHAR(255)  NOT NULL,
            TicketPrice       DECIMAL(18,2)  NOT NULL,
            EventType         NVARCHAR(100)  NOT NULL CONSTRAINT DF_Ev_Type DEFAULT 'Screening',
            HistoricalRating  FLOAT          NOT NULL CONSTRAINT DF_Ev_Rating DEFAULT 0.0,
            MaxCapacity       INT            NOT NULL CONSTRAINT DF_Ev_Cap DEFAULT 50,
            CurrentEnrollment INT            NOT NULL CONSTRAINT DF_Ev_Enroll DEFAULT 0,
            CreatorUserId     INT            NOT NULL,
            CONSTRAINT PK_Events PRIMARY KEY CLUSTERED (Id),
            CONSTRAINT FK_Events_Users FOREIGN KEY (CreatorUserId) REFERENCES dbo.Users(Id),
            CONSTRAINT CK_Ev_Price    CHECK (TicketPrice >= 0),
            CONSTRAINT CK_Ev_Rating   CHECK (HistoricalRating >= 0 AND HistoricalRating <= 5),
            CONSTRAINT CK_Ev_Cap      CHECK (MaxCapacity > 0),
            CONSTRAINT CK_Ev_Enroll   CHECK (CurrentEnrollment >= 0 AND CurrentEnrollment <= MaxCapacity)
        );";

    private const string CreateParticipationsTableSql = @"
        CREATE TABLE dbo.Participations (
            Id       INT IDENTITY(1,1) NOT NULL,
            UserId   INT           NOT NULL,
            EventId  INT           NOT NULL,
            Status   NVARCHAR(32)  NOT NULL,
            JoinedAt DATETIME2     NOT NULL CONSTRAINT DF_Part_Joined DEFAULT GETUTCDATE(),
            CONSTRAINT PK_Participations   PRIMARY KEY CLUSTERED (Id),
            CONSTRAINT UQ_Part_UserEvent   UNIQUE (UserId, EventId),
            CONSTRAINT FK_Part_Users       FOREIGN KEY (UserId)  REFERENCES dbo.Users(Id),
            CONSTRAINT FK_Part_Events      FOREIGN KEY (EventId) REFERENCES dbo.Events(Id) ON DELETE CASCADE
        );";

    private const string CreateFavoriteEventsTableSql = @"
        CREATE TABLE dbo.FavoriteEvents (
            Id        INT IDENTITY(1,1) NOT NULL,
            UserId    INT       NOT NULL,
            EventId   INT       NOT NULL,
            CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_FE_Created DEFAULT GETUTCDATE(),
            CONSTRAINT PK_FavoriteEvents   PRIMARY KEY CLUSTERED (Id),
            CONSTRAINT UQ_FE_UserEvent     UNIQUE (UserId, EventId),
            CONSTRAINT FK_FE_Users         FOREIGN KEY (UserId)  REFERENCES dbo.Users(Id),
            CONSTRAINT FK_FE_Events        FOREIGN KEY (EventId) REFERENCES dbo.Events(Id) ON DELETE CASCADE
        );";

    private const string CreateNotificationsTableSql = @"
        CREATE TABLE dbo.Notifications (
            Id        INT IDENTITY(1,1) NOT NULL,
            UserId    INT           NOT NULL,
            EventId   INT           NOT NULL,
            Type      NVARCHAR(64)  NOT NULL,
            Message   NVARCHAR(500) NOT NULL,
            State     NVARCHAR(16)  NOT NULL CONSTRAINT DF_Notif_State DEFAULT 'Unread',
            CreatedAt DATETIME2     NOT NULL CONSTRAINT DF_Notif_Created DEFAULT GETUTCDATE(),
            CONSTRAINT PK_Notifications PRIMARY KEY CLUSTERED (Id),
            CONSTRAINT FK_Notif_Users   FOREIGN KEY (UserId)  REFERENCES dbo.Users(Id),
            CONSTRAINT FK_Notif_Events  FOREIGN KEY (EventId) REFERENCES dbo.Events(Id) ON DELETE CASCADE
        );";

    private const string CreateUserSpinsTableSql = @"
        CREATE TABLE dbo.UserSpins (
            UserId                INT NOT NULL,
            DailySpinsRemaining   INT NOT NULL CONSTRAINT DF_US_Daily  DEFAULT (1),
            BonusSpins            INT NOT NULL CONSTRAINT DF_US_Bonus  DEFAULT (0),
            LastSlotSpinReset     DATETIME NULL,
            LastTriviaSpinReset   DATETIME NULL,
            LoginStreak           INT NOT NULL CONSTRAINT DF_US_Streak DEFAULT (0),
            LastLoginDate         DATETIME NULL,
            EventSpinRewardsToday INT NOT NULL CONSTRAINT DF_US_EvRewards DEFAULT (0),
            CONSTRAINT PK_UserSpins PRIMARY KEY (UserId),
            CONSTRAINT FK_US_Users  FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
        );";

    private const string CreateUserMovieDiscountsTableSql = @"
        CREATE TABLE dbo.UserMovieDiscounts (
            Id                 INT IDENTITY(1,1) NOT NULL,
            UserId             INT            NOT NULL,
            MovieId            INT            NOT NULL,
            DiscountPercentage DECIMAL(5,2)   NOT NULL,
            CreatedAt          DATETIME       NOT NULL CONSTRAINT DF_UMD_Created DEFAULT (GETDATE()),
            Redeemed           BIT            NOT NULL CONSTRAINT DF_UMD_Redeemed DEFAULT (0),
            CONSTRAINT PK_UMD          PRIMARY KEY (Id),
            CONSTRAINT FK_UMD_Users    FOREIGN KEY (UserId)  REFERENCES dbo.Users(Id),
            CONSTRAINT FK_UMD_Movies   FOREIGN KEY (MovieId) REFERENCES dbo.Movies(Id),
            CONSTRAINT CK_UMD_Pct     CHECK (DiscountPercentage >= 0 AND DiscountPercentage <= 100)
        );";

    private const string CreateMarathonTablesSql = @"
        CREATE TABLE dbo.Marathons (
            Id                   INT IDENTITY(1,1) NOT NULL,
            Title                NVARCHAR(200) NOT NULL,
            Description          NVARCHAR(MAX) NULL,
            PosterUrl            NVARCHAR(500) NOT NULL CONSTRAINT DF_Mar_Poster DEFAULT '',
            Theme                NVARCHAR(100) NULL,
            PrerequisiteMarathonId INT NULL,
            IsActive             BIT           NOT NULL CONSTRAINT DF_Mar_Active DEFAULT 0,
            LastFeaturedDate     DATETIME2     NULL,
            WeekScoping          NVARCHAR(50)  NULL,
            CONSTRAINT PK_Marathons  PRIMARY KEY CLUSTERED (Id),
            CONSTRAINT FK_Mar_Prereq FOREIGN KEY (PrerequisiteMarathonId) REFERENCES dbo.Marathons(Id)
        );
        CREATE TABLE dbo.MarathonMovies (
            MarathonId INT NOT NULL,
            MovieId    INT NOT NULL,
            CONSTRAINT PK_MarathonMovies    PRIMARY KEY (MarathonId, MovieId),
            CONSTRAINT FK_MM_Marathons      FOREIGN KEY (MarathonId) REFERENCES dbo.Marathons(Id) ON DELETE CASCADE,
            CONSTRAINT FK_MM_Movies         FOREIGN KEY (MovieId)    REFERENCES dbo.Movies(Id)    ON DELETE CASCADE
        );
        CREATE TABLE dbo.MarathonProgress (
            UserId               INT   NOT NULL,
            MarathonId           INT   NOT NULL,
            JoinedAt             DATETIME2 NOT NULL CONSTRAINT DF_MP_Joined DEFAULT GETDATE(),
            TriviaAccuracy       FLOAT NOT NULL CONSTRAINT DF_MP_Accuracy DEFAULT 0.0,
            CompletedMoviesCount INT   NOT NULL CONSTRAINT DF_MP_Count DEFAULT 0,
            FinishedAt           DATETIME2 NULL,
            CONSTRAINT PK_MarathonProgress   PRIMARY KEY (UserId, MarathonId),
            CONSTRAINT FK_MP_Users           FOREIGN KEY (UserId)     REFERENCES dbo.Users(Id),
            CONSTRAINT FK_MP_Marathons       FOREIGN KEY (MarathonId) REFERENCES dbo.Marathons(Id),
            CONSTRAINT CK_MP_Accuracy        CHECK (TriviaAccuracy >= 0 AND TriviaAccuracy <= 100)
        );";

    private const string CreateTriviaQuestionsTableSql = @"
        CREATE TABLE dbo.TriviaQuestions (
            Id            INT IDENTITY(1,1) NOT NULL,
            QuestionText  NVARCHAR(MAX)  NOT NULL,
            Category      NVARCHAR(64)   NOT NULL,
            OptionA       NVARCHAR(256)  NOT NULL,
            OptionB       NVARCHAR(256)  NOT NULL,
            OptionC       NVARCHAR(256)  NOT NULL,
            OptionD       NVARCHAR(256)  NOT NULL,
            CorrectOption CHAR(1)        NOT NULL,
            MovieId       INT            NULL,
            CONSTRAINT PK_TriviaQ       PRIMARY KEY (Id),
            CONSTRAINT FK_TQ_Movies     FOREIGN KEY (MovieId) REFERENCES dbo.Movies(Id),
            CONSTRAINT CK_TQ_Option     CHECK (CorrectOption IN ('A','B','C','D'))
        );";

    private const string CreateAmbassadorProfileTableSql = @"
        CREATE TABLE dbo.AmbassadorProfile (
            UserId         INT          NOT NULL,
            referral_code  NVARCHAR(64) NOT NULL,
            reward_balance INT          NOT NULL DEFAULT 0,
            CONSTRAINT PK_AmbProfile PRIMARY KEY CLUSTERED (UserId),
            CONSTRAINT FK_Amb_Users  FOREIGN KEY (UserId) REFERENCES dbo.Users(Id),
            CONSTRAINT UQ_Amb_Code   UNIQUE (referral_code)
        );";

    private const string CreateReferralLogTableSql = @"
        CREATE TABLE dbo.ReferralLog (
            Id           INT IDENTITY(1,1) NOT NULL,
            AmbassadorID INT       NOT NULL,
            FriendID     INT       NOT NULL,
            EventID      INT       NOT NULL,
            UsedAt       DATETIME2 NOT NULL CONSTRAINT DF_RL_UsedAt DEFAULT SYSUTCDATETIME(),
            CONSTRAINT PK_ReferralLog    PRIMARY KEY CLUSTERED (Id),
            CONSTRAINT FK_RL_Ambassador  FOREIGN KEY (AmbassadorID) REFERENCES dbo.Users(Id),
            CONSTRAINT FK_RL_Friend      FOREIGN KEY (FriendID)     REFERENCES dbo.Users(Id),
            CONSTRAINT FK_RL_Event       FOREIGN KEY (EventID)      REFERENCES dbo.Events(Id)
        );";

    private const string CreateScreeningsTableSql = @"
        CREATE TABLE dbo.Screenings (
            Id            INT IDENTITY(1,1) NOT NULL,
            EventId       INT      NOT NULL,
            MovieId       INT      NOT NULL,
            ScreeningTime DATETIME NOT NULL,
            CONSTRAINT PK_Screenings       PRIMARY KEY (Id),
            CONSTRAINT UQ_Screen_EventMovie UNIQUE (EventId, MovieId),
            CONSTRAINT FK_Scr_Events       FOREIGN KEY (EventId) REFERENCES dbo.Events(Id),
            CONSTRAINT FK_Scr_Movies       FOREIGN KEY (MovieId) REFERENCES dbo.Movies(Id)
        );";

    private const string CreateTriviaRewardsTableSql = @"
        CREATE TABLE dbo.TriviaRewards (
            Id         INT IDENTITY(1,1) NOT NULL,
            UserId     INT      NOT NULL,
            IsRedeemed BIT      NOT NULL CONSTRAINT DF_TR_Redeemed DEFAULT (0),
            CreatedAt  DATETIME NOT NULL CONSTRAINT DF_TR_Created  DEFAULT (GETDATE()),
            CONSTRAINT PK_TriviaRewards PRIMARY KEY (Id),
            CONSTRAINT FK_TR_Users      FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
        );";

    private const string CreateUserEventAttendanceTableSql = @"
        CREATE TABLE dbo.UserEventAttendance (
            Id       INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_UEA PRIMARY KEY,
            UserId   INT       NOT NULL,
            EventId  INT       NOT NULL,
            JoinedAt DATETIME2 NOT NULL CONSTRAINT DF_UEA_Joined DEFAULT GETUTCDATE(),
            CONSTRAINT UQ_UEA_UserEvent UNIQUE (UserId, EventId)
        );";
}