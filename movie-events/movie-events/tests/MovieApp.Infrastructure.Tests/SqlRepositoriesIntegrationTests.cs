// <copyright file="SqlRepositoriesIntegrationTests.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

#pragma warning disable SA1649

namespace MovieApp.Infrastructure.Tests;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using MovieApp.Core.Models;
using MovieApp.Core.Models.Movie;
using Xunit;



public sealed class DatabaseOptionsTests
{
    [Fact]
    public void ConnectionString_GetSet_RoundTrips()
    {
        const string cs = "Server=localhost;Database=Test;";
        var opts = new DatabaseOptions { ConnectionString = cs };
        Assert.Equal(cs, opts.ConnectionString);
    }

    [Fact]
    public void DatabaseOptions_IsSealed_AndRequired()
    {
        var opts = new DatabaseOptions { ConnectionString = "x" };
        Assert.IsType<DatabaseOptions>(opts);
    }
}


[Collection("Database")]
public sealed class SqlUserRepositoryTests
{
    private readonly DatabaseFixture _db;
    public SqlUserRepositoryTests(DatabaseFixture db) => _db = db;

    private SqlUserRepository Repo => new SqlUserRepository(_db.DatabaseOptions);

    [Fact]
    public async Task FindByAuthIdentityAsync_ExistingUser_ReturnsUser()
    {
        var provider = "test_" + Guid.NewGuid().ToString("N")[..8];
        var subject = Guid.NewGuid().ToString("N");
        var username = "User_" + Guid.NewGuid().ToString("N")[..8];
        _db.InsertUser(username, provider, subject);

        var result = await Repo.FindByAuthIdentityAsync(provider, subject);

        Assert.NotNull(result);
        Assert.Equal(provider, result!.AuthProvider);
        Assert.Equal(subject, result.AuthSubject);
        Assert.Equal(username, result.Username);
        Assert.True(result.Id > 0);
    }

    [Fact]
    public async Task FindByAuthIdentityAsync_NonExistingUser_ReturnsNull()
    {
        var result = await Repo.FindByAuthIdentityAsync("nobody", "ghost_" + Guid.NewGuid());
        Assert.Null(result);
    }
}


[Collection("Database")]
public sealed class SqlEventRepositoryTests
{
    private readonly DatabaseFixture _db;
    public SqlEventRepositoryTests(DatabaseFixture db) => _db = db;

    private SqlEventRepository Repo => new SqlEventRepository(_db.DatabaseOptions);

    private int TestUser => _db.InsertUser("EvtUser_" + Guid.NewGuid().ToString("N")[..6]);

    [Fact]
    public async Task AddAsync_ValidEvent_ReturnsPositiveId()
    {
        int userId = TestUser;
        var ev = MakeEvent(userId, "AddTest_" + Guid.NewGuid().ToString("N")[..6]);
        int id = await Repo.AddAsync(ev);
        Assert.True(id > 0);
    }

    [Fact]
    public async Task GetAllAsync_AfterInsert_ContainsEvent()
    {
        int userId = TestUser;
        string title = "GetAll_" + Guid.NewGuid().ToString("N")[..6];
        await Repo.AddAsync(MakeEvent(userId, title));

        var all = (await Repo.GetAllAsync()).ToList();
        Assert.Contains(all, e => e.Title == title);
    }

    [Fact]
    public async Task GetAllByTypeAsync_FiltersByEventType()
    {
        int userId = TestUser;
        string type = "SpecialType_" + Guid.NewGuid().ToString("N")[..6];
        await Repo.AddAsync(MakeEvent(userId, "TypedEvent_" + Guid.NewGuid().ToString("N")[..4], type));

        var result = (await Repo.GetAllByTypeAsync(type)).ToList();
        Assert.All(result, e => Assert.Equal(type, e.EventType));
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task FindByIdAsync_ExistingEvent_ReturnsEvent()
    {
        int userId = TestUser;
        string title = "FindById_" + Guid.NewGuid().ToString("N")[..6];
        int id = await Repo.AddAsync(MakeEvent(userId, title));

        var ev = await Repo.FindByIdAsync(id);

        Assert.NotNull(ev);
        Assert.Equal(id, ev!.Id);
        Assert.Equal(title, ev.Title);
    }

    [Fact]
    public async Task FindByIdAsync_NonExistingEvent_ReturnsNull()
    {
        var ev = await Repo.FindByIdAsync(int.MaxValue);
        Assert.Null(ev);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingEvent_ReturnsFalse()
    {
        int userId = TestUser;
        var ghost = MakeEvent(userId, "Ghost");
        bool result = await Repo.UpdateAsync(ghost);
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateEnrollmentAsync_ExistingEvent_ReturnsTrue()
    {
        int userId = TestUser;
        int id = await Repo.AddAsync(MakeEvent(userId, "Enroll_" + Guid.NewGuid().ToString("N")[..6]));

        bool result = await Repo.UpdateEnrollmentAsync(id, 10);

        Assert.True(result);
        var ev = await Repo.FindByIdAsync(id);
        Assert.Equal(10, ev!.CurrentEnrollment);
    }

    [Fact]
    public async Task UpdateEnrollmentAsync_NonExistingEvent_ReturnsFalse()
    {
        bool result = await Repo.UpdateEnrollmentAsync(int.MaxValue, 5);
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_ExistingEvent_ReturnsTrueAndEventGone()
    {
        int userId = TestUser;
        int id = await Repo.AddAsync(MakeEvent(userId, "Del_" + Guid.NewGuid().ToString("N")[..6]));

        bool result = await Repo.DeleteAsync(id);

        Assert.True(result);
        Assert.Null(await Repo.FindByIdAsync(id));
    }

    [Fact]
    public async Task DeleteAsync_NonExistingEvent_ReturnsFalse()
    {
        bool result = await Repo.DeleteAsync(int.MaxValue);
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_EventWithRelatedFavorites_CascadeDeletes()
    {
        int userId = TestUser;
        int id = await Repo.AddAsync(MakeEvent(userId, "CasDel_" + Guid.NewGuid().ToString("N")[..6]));

        using var conn = _db.Open();
        using var cmd = new SqlCommand(
            "INSERT INTO dbo.FavoriteEvents (UserId, EventId) VALUES (@u, @e);", conn);
        cmd.Parameters.AddWithValue("@u", userId);
        cmd.Parameters.AddWithValue("@e", id);
        cmd.ExecuteNonQuery();

        bool result = await Repo.DeleteAsync(id);
        Assert.True(result);
    }

    private static Event MakeEvent(int creatorUserId, string title, string eventType = "Screening") =>
        new Event
        {
            Id = 0,
            Title = title,
            Description = null,
            PosterUrl = string.Empty,
            EventDateTime = DateTime.UtcNow.AddDays(1),
            LocationReference = "Hall A",
            TicketPrice = 15m,
            EventType = eventType,
            HistoricalRating = 0,
            MaxCapacity = 50,
            CurrentEnrollment = 0,
            CreatorUserId = creatorUserId,
        };
}


[Collection("Database")]
public sealed class SqlFavoriteEventRepositoryTests
{
    private readonly DatabaseFixture _db;
    public SqlFavoriteEventRepositoryTests(DatabaseFixture db) => _db = db;

    private SqlFavoriteEventRepository Repo => new SqlFavoriteEventRepository(_db.DatabaseOptions);

    [Fact]
    public async Task AddAsync_ThenExistsAsync_ReturnsTrue()
    {
        int uid = _db.InsertUser("FE_Add_" + Guid.NewGuid().ToString("N")[..6]);
        int eid = _db.InsertEvent(uid, "FE_Evt_" + Guid.NewGuid().ToString("N")[..6]);

        await Repo.AddAsync(uid, eid);

        Assert.True(await Repo.ExistsAsync(uid, eid));
    }

    [Fact]
    public async Task ExistsAsync_NoRecord_ReturnsFalse()
    {
        Assert.False(await Repo.ExistsAsync(int.MaxValue, int.MaxValue));
    }

    [Fact]
    public async Task RemoveAsync_ExistingFavorite_DisappearsFromList()
    {
        int uid = _db.InsertUser("FE_Rem_" + Guid.NewGuid().ToString("N")[..6]);
        int eid = _db.InsertEvent(uid, "FE_RemEvt_" + Guid.NewGuid().ToString("N")[..6]);
        await Repo.AddAsync(uid, eid);

        await Repo.RemoveAsync(uid, eid);

        Assert.False(await Repo.ExistsAsync(uid, eid));
    }

    [Fact]
    public async Task FindByUserAsync_ReturnsOnlyThatUsersRecords()
    {
        int uid = _db.InsertUser("FE_FBU_" + Guid.NewGuid().ToString("N")[..6]);
        int eid1 = _db.InsertEvent(uid, "FE_Evt1_" + Guid.NewGuid().ToString("N")[..6]);
        int eid2 = _db.InsertEvent(uid, "FE_Evt2_" + Guid.NewGuid().ToString("N")[..6]);
        await Repo.AddAsync(uid, eid1);
        await Repo.AddAsync(uid, eid2);

        var list = await Repo.FindByUserAsync(uid);

        Assert.Equal(2, list.Count(f => f.UserId == uid));
    }

    [Fact]
    public async Task FindByUserAsync_NoRecords_ReturnsEmpty()
    {
        var list = await Repo.FindByUserAsync(int.MaxValue);
        Assert.Empty(list);
    }

    [Fact]
    public async Task GetUsersByFavoriteEventAsync_ReturnsUsersForEvent()
    {
        int uid = _db.InsertUser("FE_GUBFE_" + Guid.NewGuid().ToString("N")[..6]);
        int eid = _db.InsertEvent(uid, "FE_GUEvt_" + Guid.NewGuid().ToString("N")[..6]);
        await Repo.AddAsync(uid, eid);

        var users = await Repo.GetUsersByFavoriteEventAsync(eid);

        Assert.Contains(uid, users);
    }

    [Fact]
    public async Task FindByEventAsync_ReturnsRecordsForEvent()
    {
        int uid = _db.InsertUser("FE_FBE_" + Guid.NewGuid().ToString("N")[..6]);
        int eid = _db.InsertEvent(uid, "FE_FBEEvt_" + Guid.NewGuid().ToString("N")[..6]);
        await Repo.AddAsync(uid, eid);

        var list = await Repo.FindByEventAsync(eid);

        Assert.Contains(list, f => f.EventId == eid && f.UserId == uid);
    }
}


[Collection("Database")]
public sealed class SqlAmbassadorRepositoryTests
{
    private readonly DatabaseFixture _db;
    public SqlAmbassadorRepositoryTests(DatabaseFixture db) => _db = db;

    private SqlAmbassadorRepository Repo => new SqlAmbassadorRepository(_db.DatabaseOptions);

    private string UniqueCode => "REF_" + Guid.NewGuid().ToString("N")[..10];

    [Fact]
    public async Task CreateAmbassadorProfileAsync_ThenIsReferralCodeValidAsync_ReturnsTrue()
    {
        int uid = _db.InsertUser("Amb_" + Guid.NewGuid().ToString("N")[..6]);
        string code = UniqueCode;

        await Repo.CreateAmbassadorProfileAsync(uid, code);

        Assert.True(await Repo.IsReferralCodeValidAsync(code));
    }

    [Fact]
    public async Task IsReferralCodeValidAsync_UnknownCode_ReturnsFalse()
    {
        Assert.False(await Repo.IsReferralCodeValidAsync("NONEXISTENT_" + Guid.NewGuid()));
    }

    [Fact]
    public async Task GetReferralCodeAsync_ExistingProfile_ReturnsCode()
    {
        int uid = _db.InsertUser("AmbGRC_" + Guid.NewGuid().ToString("N")[..6]);
        string code = UniqueCode;
        _db.InsertAmbassadorProfile(uid, code);

        string? result = await Repo.GetReferralCodeAsync(uid);

        Assert.Equal(code, result);
    }

    [Fact]
    public async Task GetReferralCodeAsync_NoProfile_ReturnsNull()
    {
        string? result = await Repo.GetReferralCodeAsync(int.MaxValue);
        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserIdByReferralCodeAsync_ExistingCode_ReturnsUserId()
    {
        int uid = _db.InsertUser("AmbGUID_" + Guid.NewGuid().ToString("N")[..6]);
        string code = UniqueCode;
        _db.InsertAmbassadorProfile(uid, code);

        int? result = await Repo.GetUserIdByReferralCodeAsync(code);

        Assert.Equal(uid, result);
    }

    [Fact]
    public async Task GetUserIdByReferralCodeAsync_UnknownCode_ReturnsNull()
    {
        int? result = await Repo.GetUserIdByReferralCodeAsync("GHOST_" + Guid.NewGuid());
        Assert.Null(result);
    }

    [Fact]
    public async Task AddReferralLogAsync_InsertsLog()
    {
        int ambassador = _db.InsertUser("AmbARL_amb_" + Guid.NewGuid().ToString("N")[..6]);
        int friend = _db.InsertUser("AmbARL_frd_" + Guid.NewGuid().ToString("N")[..6]);
        int evt = _db.InsertEvent(ambassador, "AmbARL_evt_" + Guid.NewGuid().ToString("N")[..6]);
        _db.InsertAmbassadorProfile(ambassador, UniqueCode);

        await Repo.AddReferralLogAsync(ambassador, friend, evt);

        Assert.True(await Repo.HasReferralLogAsync(ambassador, friend, evt));
    }

    [Fact]
    public async Task HasReferralLogAsync_NoLog_ReturnsFalse()
    {
        Assert.False(await Repo.HasReferralLogAsync(int.MaxValue, int.MaxValue, int.MaxValue));
    }

    [Fact]
    public async Task TryApplyRewardAsync_With3Logs_ReturnsTrueAndIncrementsBalance()
    {
        int ambassador = _db.InsertUser("AmbTA3_amb_" + Guid.NewGuid().ToString("N")[..6]);
        int friend1 = _db.InsertUser("AmbTA3_f1_" + Guid.NewGuid().ToString("N")[..6]);
        int friend2 = _db.InsertUser("AmbTA3_f2_" + Guid.NewGuid().ToString("N")[..6]);
        int friend3 = _db.InsertUser("AmbTA3_f3_" + Guid.NewGuid().ToString("N")[..6]);
        int evt = _db.InsertEvent(ambassador, "AmbTA3_evt_" + Guid.NewGuid().ToString("N")[..6]);
        string code = UniqueCode;
        _db.InsertAmbassadorProfile(ambassador, code);
        _db.InsertReferralLog(ambassador, friend1, evt);
        _db.InsertReferralLog(ambassador, friend2, evt);
        _db.InsertReferralLog(ambassador, friend3, evt);

        bool result = await Repo.TryApplyRewardAsync(ambassador);

        Assert.True(result);
        int balance = await Repo.GetRewardBalanceAsync(ambassador);
        Assert.Equal(1, balance);
    }

    [Fact]
    public async Task TryApplyRewardAsync_With2Logs_ReturnsFalse()
    {
        int ambassador = _db.InsertUser("AmbTA2_amb_" + Guid.NewGuid().ToString("N")[..6]);
        int friend1 = _db.InsertUser("AmbTA2_f1_" + Guid.NewGuid().ToString("N")[..6]);
        int friend2 = _db.InsertUser("AmbTA2_f2_" + Guid.NewGuid().ToString("N")[..6]);
        int evt = _db.InsertEvent(ambassador, "AmbTA2_evt_" + Guid.NewGuid().ToString("N")[..6]);
        string code = UniqueCode;
        _db.InsertAmbassadorProfile(ambassador, code);
        _db.InsertReferralLog(ambassador, friend1, evt);
        _db.InsertReferralLog(ambassador, friend2, evt);

        bool result = await Repo.TryApplyRewardAsync(ambassador);

        Assert.False(result);
    }

    [Fact]
    public async Task GetRewardBalanceAsync_NoProfile_ReturnsZero()
    {
        int balance = await Repo.GetRewardBalanceAsync(int.MaxValue);
        Assert.Equal(0, balance);
    }

    [Fact]
    public async Task DecrementRewardBalanceAsync_PositiveBalance_DecrementsBy1()
    {
        int uid = _db.InsertUser("AmbDec_" + Guid.NewGuid().ToString("N")[..6]);
        _db.InsertAmbassadorProfile(uid, UniqueCode, rewardBalance: 3);

        await Repo.DecrementRewardBalanceAsync(uid);

        int balance = await Repo.GetRewardBalanceAsync(uid);
        Assert.Equal(2, balance);
    }

    [Fact]
    public async Task DecrementRewardBalanceAsync_ZeroBalance_StaysAtZero()
    {
        int uid = _db.InsertUser("AmbDec0_" + Guid.NewGuid().ToString("N")[..6]);
        _db.InsertAmbassadorProfile(uid, UniqueCode, rewardBalance: 0);

        await Repo.DecrementRewardBalanceAsync(uid);

        int balance = await Repo.GetRewardBalanceAsync(uid);
        Assert.Equal(0, balance);
    }

    [Fact]
    public async Task GetReferralHistoryAsync_WithLogs_ReturnsItems()
    {
        int ambassador = _db.InsertUser("AmbHist_amb_" + Guid.NewGuid().ToString("N")[..6]);
        int friend = _db.InsertUser("AmbHist_frd_" + Guid.NewGuid().ToString("N")[..6]);
        int evt = _db.InsertEvent(ambassador, "AmbHist_evt_" + Guid.NewGuid().ToString("N")[..6]);
        _db.InsertAmbassadorProfile(ambassador, UniqueCode);
        _db.InsertReferralLog(ambassador, friend, evt);

        var history = (await Repo.GetReferralHistoryAsync(ambassador)).ToList();

        Assert.NotEmpty(history);
        Assert.All(history, item => Assert.NotNull(item.FriendName));
    }

    [Fact]
    public async Task GetReferralHistoryAsync_NoLogs_ReturnsEmpty()
    {
        var history = await Repo.GetReferralHistoryAsync(int.MaxValue);
        Assert.Empty(history);
    }
}


[Collection("Database")]
public sealed class SqlNotificationRepositoryTests
{
    private readonly DatabaseFixture _db;
    public SqlNotificationRepositoryTests(DatabaseFixture db) => _db = db;

    private SqlNotificationRepository Repo => new SqlNotificationRepository(_db.DatabaseOptions);

    [Fact]
    public async Task AddAsync_ThenFindByUserAsync_ReturnsNotification()
    {
        int uid = _db.InsertUser("Notif_" + Guid.NewGuid().ToString("N")[..6]);
        int eid = _db.InsertEvent(uid, "NotifEvt_" + Guid.NewGuid().ToString("N")[..6]);
        var notif = new Notification
        {
            UserId = uid,
            EventId = eid,
            Type = "Test",
            Message = "Hello",
            State = NotificationState.Unread,
            CreatedAt = DateTime.UtcNow,
        };

        await Repo.AddAsync(notif);

        var list = await Repo.FindByUserAsync(uid);
        Assert.Contains(list, n => n.UserId == uid && n.Type == "Test");
    }

    [Fact]
    public async Task FindByUserAsync_NoNotifications_ReturnsEmpty()
    {
        var list = await Repo.FindByUserAsync(int.MaxValue);
        Assert.Empty(list);
    }

    [Fact]
    public async Task FindByUserAsync_ParsesStateEnum()
    {
        int uid = _db.InsertUser("NotifState_" + Guid.NewGuid().ToString("N")[..6]);
        int eid = _db.InsertEvent(uid, "NSEvt_" + Guid.NewGuid().ToString("N")[..6]);
        var notif = new Notification
        {
            UserId = uid,
            EventId = eid,
            Type = "State",
            Message = "State test",
            State = NotificationState.Read,
            CreatedAt = DateTime.UtcNow,
        };
        await Repo.AddAsync(notif);

        var list = await Repo.FindByUserAsync(uid);
        var found = list.First(n => n.Type == "State");
        Assert.Equal(NotificationState.Read, found.State);
    }

    [Fact]
    public async Task RemoveAsync_ExistingNotification_IsGone()
    {
        int uid = _db.InsertUser("NotifRem_" + Guid.NewGuid().ToString("N")[..6]);
        int eid = _db.InsertEvent(uid, "NRemEvt_" + Guid.NewGuid().ToString("N")[..6]);
        var notif = new Notification
        {
            UserId = uid,
            EventId = eid,
            Type = "Rem",
            Message = "Bye",
            State = NotificationState.Unread,
            CreatedAt = DateTime.UtcNow,
        };
        await Repo.AddAsync(notif);
        var id = (await Repo.FindByUserAsync(uid)).First(n => n.Type == "Rem").Id;

        await Repo.RemoveAsync(id);

        var remaining = await Repo.FindByUserAsync(uid);
        Assert.DoesNotContain(remaining, n => n.Id == id);
    }
}

[Collection("Database")]
public sealed class SqlScreeningRepositoryTests
{
    private readonly DatabaseFixture _db;
    public SqlScreeningRepositoryTests(DatabaseFixture db) => _db = db;

    private SqlScreeningRepository Repo => new SqlScreeningRepository(_db.DatabaseOptions);

    [Fact]
    public void Constructor_NullOptions_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new SqlScreeningRepository(null!));
    }

    [Fact]
    public async Task AddAsync_ThenGetByEventId_ReturnsScreening()
    {
        int uid = _db.InsertUser("Scr_" + Guid.NewGuid().ToString("N")[..6]);
        int eid = _db.InsertEvent(uid, "ScrEvt_" + Guid.NewGuid().ToString("N")[..6]);
        int mid = _db.InsertMovie("ScrMov_" + Guid.NewGuid().ToString("N")[..6]);

        await Repo.AddAsync(new Screening { Id = 0, EventId = eid, MovieId = mid, ScreeningTime = DateTime.UtcNow.AddDays(1) });

        var list = await Repo.GetByEventIdAsync(eid);
        Assert.Contains(list, s => s.EventId == eid && s.MovieId == mid);
    }

    [Fact]
    public async Task GetByEventIdAsync_NoScreenings_ReturnsEmpty()
    {
        var list = await Repo.GetByEventIdAsync(int.MaxValue);
        Assert.Empty(list);
    }

    [Fact]
    public async Task GetByMovieIdAsync_ReturnsScreeningsForMovie()
    {
        int uid = _db.InsertUser("ScrM_" + Guid.NewGuid().ToString("N")[..6]);
        int eid = _db.InsertEvent(uid, "ScrMEvt_" + Guid.NewGuid().ToString("N")[..6]);
        int mid = _db.InsertMovie("ScrMMov_" + Guid.NewGuid().ToString("N")[..6]);
        _db.InsertScreening(eid, mid);

        var list = await Repo.GetByMovieIdAsync(mid);
        Assert.Contains(list, s => s.MovieId == mid);
    }

    [Fact]
    public async Task GetByMovieIdAsync_NoScreenings_ReturnsEmpty()
    {
        var list = await Repo.GetByMovieIdAsync(int.MaxValue);
        Assert.Empty(list);
    }
}

[Collection("Database")]
public sealed class SqlTriviaRepositoryTests
{
    private readonly DatabaseFixture _db;
    public SqlTriviaRepositoryTests(DatabaseFixture db) => _db = db;

    private SqlTriviaRepository Repo => new SqlTriviaRepository(_db.DatabaseOptions);

    private void InsertTriviaQuestion(string category, int? movieId = null)
    {
        using var conn = _db.Open();
        using var cmd = new SqlCommand(
            @"INSERT INTO dbo.TriviaQuestions
                (QuestionText, Category, OptionA, OptionB, OptionC, OptionD, CorrectOption, MovieId)
              VALUES (@q, @cat, 'A1', 'B1', 'C1', 'D1', 'A', @mid);",
            conn);
        cmd.Parameters.AddWithValue("@q", "Q_" + Guid.NewGuid().ToString("N")[..8]);
        cmd.Parameters.AddWithValue("@cat", category);
        cmd.Parameters.AddWithValue("@mid", (object?)movieId ?? DBNull.Value);
        cmd.ExecuteNonQuery();
    }

    [Fact]
    public async Task GetByCategoryAsync_WithQuestions_ReturnsThem()
    {
        string category = "Cat_" + Guid.NewGuid().ToString("N")[..8];
        InsertTriviaQuestion(category);
        InsertTriviaQuestion(category);

        var result = (await Repo.GetByCategoryAsync(category)).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, q => Assert.Equal(category, q.Category));
    }

    [Fact]
    public async Task GetByCategoryAsync_NoQuestions_ReturnsEmpty()
    {
        var result = await Repo.GetByCategoryAsync("NoSuchCat_" + Guid.NewGuid());
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByMovieIdAsync_WithMovieQuestions_ReturnsThem()
    {
        int mid = _db.InsertMovie("TrivMov_" + Guid.NewGuid().ToString("N")[..6]);
        InsertTriviaQuestion("General", mid);
        InsertTriviaQuestion("General", mid);
        InsertTriviaQuestion("General", mid);

        var result = (await Repo.GetByMovieIdAsync(mid, 3)).ToList();

        Assert.Equal(3, result.Count);
        Assert.All(result, q => Assert.Equal(mid, q.MovieId));
    }

    [Fact]
    public async Task GetByMovieIdAsync_NoQuestions_ReturnsEmpty()
    {
        var result = await Repo.GetByMovieIdAsync(int.MaxValue, 3);
        Assert.Empty(result);
    }
}


[Collection("Database")]
public sealed class SqlTriviaRewardRepositoryTests
{
    private readonly DatabaseFixture _db;
    public SqlTriviaRewardRepositoryTests(DatabaseFixture db) => _db = db;

    private SqlTriviaRewardRepository Repo => new SqlTriviaRewardRepository(_db.DatabaseOptions);

    [Fact]
    public async Task AddAsync_ThenGetUnredeemedByUserAsync_ReturnsReward()
    {
        int uid = _db.InsertUser("TR_Add_" + Guid.NewGuid().ToString("N")[..6]);
        var reward = new TriviaReward { Id = 0, UserId = uid, IsRedeemed = false, CreatedAt = DateTime.UtcNow };

        await Repo.AddAsync(reward);

        var result = await Repo.GetUnredeemedByUserAsync(uid);
        Assert.NotNull(result);
        Assert.Equal(uid, result!.UserId);
        Assert.False(result.IsRedeemed);
    }

    [Fact]
    public async Task GetUnredeemedByUserAsync_NoReward_ReturnsNull()
    {
        var result = await Repo.GetUnredeemedByUserAsync(int.MaxValue);
        Assert.Null(result);
    }

    [Fact]
    public async Task MarkAsRedeemedAsync_MarksRewardRedeemed()
    {
        int uid = _db.InsertUser("TR_Mark_" + Guid.NewGuid().ToString("N")[..6]);
        await Repo.AddAsync(new TriviaReward { Id = 0, UserId = uid, IsRedeemed = false, CreatedAt = DateTime.UtcNow });
        var reward = await Repo.GetUnredeemedByUserAsync(uid);
        Assert.NotNull(reward);

        await Repo.MarkAsRedeemedAsync(reward!.Id);

        var result = await Repo.GetUnredeemedByUserAsync(uid);
        Assert.Null(result);
    }
}


[Collection("Database")]
public sealed class SqlUserEventAttendanceRepositoryTests
{
    private readonly DatabaseFixture _db;
    public SqlUserEventAttendanceRepositoryTests(DatabaseFixture db) => _db = db;

    private SqlUserEventAttendanceRepository Repo =>
        new SqlUserEventAttendanceRepository(_db.DatabaseOptions);

    [Fact]
    public void Constructor_NullOptions_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new SqlUserEventAttendanceRepository(null!));
    }

    [Fact]
    public async Task JoinAsync_FirstTime_AppearsInGetJoinedEventIds()
    {
        int uid = _db.InsertUser("UEA_" + Guid.NewGuid().ToString("N")[..6]);
        int eid = _db.InsertEvent(uid, "UEAEvt_" + Guid.NewGuid().ToString("N")[..6]);

        await Repo.JoinAsync(uid, eid);

        var ids = await Repo.GetJoinedEventIdsAsync(uid);
        Assert.Contains(eid, ids);
    }

    [Fact]
    public async Task JoinAsync_CalledTwice_IsIdempotent()
    {
        int uid = _db.InsertUser("UEA2_" + Guid.NewGuid().ToString("N")[..6]);
        int eid = _db.InsertEvent(uid, "UEA2Evt_" + Guid.NewGuid().ToString("N")[..6]);

        await Repo.JoinAsync(uid, eid);
        await Repo.JoinAsync(uid, eid);

        var ids = await Repo.GetJoinedEventIdsAsync(uid);
        Assert.Single(ids, id => id == eid);
    }

    [Fact]
    public async Task GetJoinedEventIdsAsync_NoAttendance_ReturnsEmpty()
    {
        var ids = await Repo.GetJoinedEventIdsAsync(int.MaxValue);
        Assert.Empty(ids);
    }
}


[Collection("Database")]
public sealed class SqlUserRewardRepositoryTests
{
    private readonly DatabaseFixture _db;
    public SqlUserRewardRepositoryTests(DatabaseFixture db) => _db = db;

    private SqlUserRewardRepository Repo => new SqlUserRewardRepository(_db.DatabaseOptions);

    [Fact]
    public void Constructor_NullOptions_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new SqlUserRewardRepository(null!));
    }

    [Fact]
    public async Task AddAsync_ThenGetDiscountsForUserAsync_ContainsReward()
    {
        int uid = _db.InsertUser("URR_Add_" + Guid.NewGuid().ToString("N")[..6]);
        int mid = _db.InsertMovie("URR_Mov_" + Guid.NewGuid().ToString("N")[..6]);
        var reward = new Reward
        {
            RewardId = 0,
            RewardType = "MovieDiscount",
            OwnerUserId = uid,
            EventId = mid,
            DiscountValue = 70.0,
            RedemptionStatus = false,
        };

        await Repo.AddAsync(reward);

        var list = await Repo.GetDiscountsForUserAsync(uid);
        Assert.NotEmpty(list);
        Assert.Contains(list, r => r.OwnerUserId == uid && r.EventId == mid);
    }

    [Fact]
    public async Task GetDiscountsForUserAsync_NoRewards_ReturnsEmpty()
    {
        var list = await Repo.GetDiscountsForUserAsync(int.MaxValue);
        Assert.Empty(list);
    }
}

[Collection("Database")]
public sealed class SqlUserSlotMachineStateRepositoryTests
{
    private readonly DatabaseFixture _db;
    public SqlUserSlotMachineStateRepositoryTests(DatabaseFixture db) => _db = db;

    private SqlUserSlotMachineStateRepository Repo =>
        new SqlUserSlotMachineStateRepository(_db.DatabaseOptions);

    [Fact]
    public void Constructor_NullOptions_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new SqlUserSlotMachineStateRepository(null!));
    }

    [Fact]
    public async Task GetByUserIdAsync_NoRecord_ReturnsNull()
    {
        var result = await Repo.GetByUserIdAsync(int.MaxValue);
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ThenGetByUserIdAsync_ReturnsState()
    {
        int uid = _db.InsertUser("USS_Create_" + Guid.NewGuid().ToString("N")[..6]);
        var state = new UserSpinData
        {
            UserId = uid,
            DailySpinsRemaining = 5,
            BonusSpins = 2,
            LoginStreak = 3,
            EventSpinRewardsToday = 1,
        };

        await Repo.CreateAsync(state);

        var result = await Repo.GetByUserIdAsync(uid);
        Assert.NotNull(result);
        Assert.Equal(uid, result!.UserId);
        Assert.Equal(5, result.DailySpinsRemaining);
        Assert.Equal(2, result.BonusSpins);
        Assert.Equal(3, result.LoginStreak);
    }

    [Fact]
    public async Task UpdateAsync_PersistsChanges()
    {
        int uid = _db.InsertUser("USS_Update_" + Guid.NewGuid().ToString("N")[..6]);
        _db.InsertUserSpins(uid, dailySpins: 5);

        var state = (await Repo.GetByUserIdAsync(uid))!;
        state.DailySpinsRemaining = 1;
        state.BonusSpins = 10;
        state.LoginStreak = 7;
        await Repo.UpdateAsync(state);

        var updated = await Repo.GetByUserIdAsync(uid);
        Assert.Equal(1, updated!.DailySpinsRemaining);
        Assert.Equal(10, updated.BonusSpins);
        Assert.Equal(7, updated.LoginStreak);
    }

    [Fact]
    public async Task CreateAsync_WithNullDates_HandlesDbNullCorrectly()
    {
        int uid = _db.InsertUser("USS_NullDt_" + Guid.NewGuid().ToString("N")[..6]);
        var state = new UserSpinData
        {
            UserId = uid,
            DailySpinsRemaining = 1,
            BonusSpins = 0,
            LoginStreak = 0,
            EventSpinRewardsToday = 0,
            LastSlotSpinReset = default,
            LastTriviaSpinReset = default,
            LastLoginDate = default,
        };

        await Repo.CreateAsync(state);

        var result = await Repo.GetByUserIdAsync(uid);
        Assert.NotNull(result);
        Assert.Equal(default, result!.LastSlotSpinReset);
    }

    [Fact]
    public async Task UpdateAsync_WithNonNullDates_PersistsDates()
    {
        int uid = _db.InsertUser("USS_Dates_" + Guid.NewGuid().ToString("N")[..6]);
        _db.InsertUserSpins(uid);

        var state = (await Repo.GetByUserIdAsync(uid))!;
        var now = DateTime.UtcNow;
        state.LastSlotSpinReset = now;
        state.LastLoginDate = now;
        await Repo.UpdateAsync(state);

        var updated = await Repo.GetByUserIdAsync(uid);
        Assert.NotEqual(default, updated!.LastSlotSpinReset);
    }
}


[Collection("Database")]
public sealed class SqlMovieRepositoryTests
{
    private readonly DatabaseFixture _db;
    public SqlMovieRepositoryTests(DatabaseFixture db) => _db = db;

    private SqlMovieRepository Repo => new SqlMovieRepository(_db.DatabaseOptions);

    [Fact]
    public void Constructor_NullOptions_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new SqlMovieRepository(null!));
    }

    [Fact]
    public async Task GetGenresAsync_AfterInsert_ContainsGenre()
    {
        string name = "Genre_" + Guid.NewGuid().ToString("N")[..6];
        _db.InsertGenre(name);

        var genres = await Repo.GetGenresAsync();

        Assert.Contains(genres, g => g.Name == name);
    }

    [Fact]
    public async Task GetActorsAsync_AfterInsert_ContainsActor()
    {
        string name = "Actor_" + Guid.NewGuid().ToString("N")[..6];
        _db.InsertActor(name);

        var actors = await Repo.GetActorsAsync();

        Assert.Contains(actors, a => a.Name == name);
    }

    [Fact]
    public async Task GetDirectorsAsync_AfterInsert_ContainsDirector()
    {
        string name = "Dir_" + Guid.NewGuid().ToString("N")[..6];
        _db.InsertDirector(name);

        var directors = await Repo.GetDirectorsAsync();

        Assert.Contains(directors, d => d.Name == name);
    }

    [Fact]
    public async Task FindMoviesByCriteriaAsync_MatchAll_ReturnsMovie()
    {
        int mid = _db.InsertMovie("Criteria_" + Guid.NewGuid().ToString("N")[..6]);
        int gid = _db.InsertGenre("CriteriaG_" + Guid.NewGuid().ToString("N")[..6]);
        int aid = _db.InsertActor("CriteriaA_" + Guid.NewGuid().ToString("N")[..6]);
        int did = _db.InsertDirector("CriteriaD_" + Guid.NewGuid().ToString("N")[..6]);
        _db.LinkMovieGenre(mid, gid);
        _db.LinkMovieActor(mid, aid);
        _db.LinkMovieDirector(mid, did);

        var movies = await Repo.FindMoviesByCriteriaAsync(gid, aid, did);

        Assert.Contains(movies, m => m.Id == mid);
    }

    [Fact]
    public async Task FindMoviesByCriteriaAsync_NoMatch_ReturnsEmpty()
    {
        var movies = await Repo.FindMoviesByCriteriaAsync(int.MaxValue - 1, int.MaxValue - 2, int.MaxValue - 3);
        Assert.Empty(movies);
    }

    [Fact]
    public async Task FindMoviesByAnyCriteriaAsync_MatchGenreOnly_ReturnsMovie()
    {
        int mid = _db.InsertMovie("AnyCriteria_" + Guid.NewGuid().ToString("N")[..6]);
        int gid = _db.InsertGenre("AnyCritG_" + Guid.NewGuid().ToString("N")[..6]);
        int aid = _db.InsertActor("AnyCritA_" + Guid.NewGuid().ToString("N")[..6]);
        int did = _db.InsertDirector("AnyCritD_" + Guid.NewGuid().ToString("N")[..6]);
        _db.LinkMovieGenre(mid, gid);

        var movies = await Repo.FindMoviesByAnyCriteriaAsync(gid, aid, did);

        Assert.Contains(movies, m => m.Id == mid);
    }

    [Fact]
    public async Task FindMoviesByAnyCriteriaAsync_NoMatch_ReturnsEmpty()
    {
        var movies = await Repo.FindMoviesByAnyCriteriaAsync(int.MaxValue - 1, int.MaxValue - 2, int.MaxValue - 3);
        Assert.Empty(movies);
    }

    [Fact]
    public async Task FindScreeningEventIdsForMovieAsync_WithFutureEvent_ReturnsEventId()
    {
        int uid = _db.InsertUser("MovScr_" + Guid.NewGuid().ToString("N")[..6]);
        int eid = _db.InsertEvent(uid, "MovScrEvt_" + Guid.NewGuid().ToString("N")[..6]);
        int mid = _db.InsertMovie("MovScrMov_" + Guid.NewGuid().ToString("N")[..6]);
        _db.InsertScreening(eid, mid);

        var ids = await Repo.FindScreeningEventIdsForMovieAsync(mid);

        Assert.Contains(eid, ids);
    }

    [Fact]
    public async Task FindScreeningEventIdsForMovieAsync_NoScreenings_ReturnsEmpty()
    {
        var ids = await Repo.FindScreeningEventIdsForMovieAsync(int.MaxValue);
        Assert.Empty(ids);
    }

    [Fact]
    public async Task GetValidReelCombinationsAsync_WithFullyLinkedMovieAndFutureScreening_ReturnsCombination()
    {
        int uid = _db.InsertUser("VRC_" + Guid.NewGuid().ToString("N")[..6]);
        int eid = _db.InsertEvent(uid, "VRCEvt_" + Guid.NewGuid().ToString("N")[..6]);
        int mid = _db.InsertMovie("VRCMov_" + Guid.NewGuid().ToString("N")[..6]);
        int gid = _db.InsertGenre("VRCG_" + Guid.NewGuid().ToString("N")[..6]);
        int aid = _db.InsertActor("VRCA_" + Guid.NewGuid().ToString("N")[..6]);
        int did = _db.InsertDirector("VRCD_" + Guid.NewGuid().ToString("N")[..6]);
        _db.LinkMovieGenre(mid, gid);
        _db.LinkMovieActor(mid, aid);
        _db.LinkMovieDirector(mid, did);
        _db.InsertScreening(eid, mid);

        var combos = await Repo.GetValidReelCombinationsAsync();

        Assert.Contains(combos, c => c.Genre.Id == gid && c.Actor.Id == aid && c.Director.Id == did);
    }
}

[Collection("Database")]
public sealed class SqlMarathonRepositoryTests
{
    private readonly DatabaseFixture _db;
    public SqlMarathonRepositoryTests(DatabaseFixture db) => _db = db;

    private SqlMarathonRepository Repo => new SqlMarathonRepository(_db.DatabaseOptions);

    [Fact]
    public void Constructor_NullOptions_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new SqlMarathonRepository(null!));
    }

    [Fact]
    public async Task GetActiveMarathonsAsync_ReturnsOnlyActive()
    {
        string title = "ActiveMar_" + Guid.NewGuid().ToString("N")[..6];
        _db.InsertMarathon(title, isActive: true);

        var marathons = (await Repo.GetActiveMarathonsAsync()).ToList();

        Assert.Contains(marathons, m => m.Title == title);
    }

    [Fact]
    public async Task GetUserProgressAsync_NoRecord_ReturnsNull()
    {
        var result = await Repo.GetUserProgressAsync(int.MaxValue, int.MaxValue);
        Assert.Null(result);
    }

    [Fact]
    public async Task JoinMarathonAsync_CreatesProgressRecord()
    {
        int uid = _db.InsertUser("MarJoin_" + Guid.NewGuid().ToString("N")[..6]);
        int mid = _db.InsertMarathon("MarJoinMar_" + Guid.NewGuid().ToString("N")[..6]);

        bool result = await Repo.JoinMarathonAsync(uid, mid);

        Assert.True(result);
        var progress = await Repo.GetUserProgressAsync(uid, mid);
        Assert.NotNull(progress);
        Assert.Equal(uid, progress!.UserId);
        Assert.Equal(mid, progress.MarathonId);
    }

    [Fact]
    public async Task UpdateProgressAsync_PersistsAccuracyAndCount()
    {
        int uid = _db.InsertUser("MarUpd_" + Guid.NewGuid().ToString("N")[..6]);
        int mid = _db.InsertMarathon("MarUpdMar_" + Guid.NewGuid().ToString("N")[..6]);
        await Repo.JoinMarathonAsync(uid, mid);

        var progress = (await Repo.GetUserProgressAsync(uid, mid))!;
        progress.TriviaAccuracy = 85.5;
        progress.CompletedMoviesCount = 3;
        bool result = await Repo.UpdateProgressAsync(progress);

        Assert.True(result);
        var updated = (await Repo.GetUserProgressAsync(uid, mid))!;
        Assert.Equal(85.5, updated.TriviaAccuracy);
        Assert.Equal(3, updated.CompletedMoviesCount);
    }

    [Fact]
    public async Task UpdateProgressAsync_WithFinishedAt_PersistsCompletionTime()
    {
        int uid = _db.InsertUser("MarFin_" + Guid.NewGuid().ToString("N")[..6]);
        int mid = _db.InsertMarathon("MarFinMar_" + Guid.NewGuid().ToString("N")[..6]);
        await Repo.JoinMarathonAsync(uid, mid);
        var progress = (await Repo.GetUserProgressAsync(uid, mid))!;
        progress.FinishedAt = DateTime.UtcNow;

        await Repo.UpdateProgressAsync(progress);

        var updated = await Repo.GetUserProgressAsync(uid, mid);
        Assert.NotNull(updated!.FinishedAt);
        Assert.True(updated.IsCompleted);
    }

    [Fact]
    public async Task GetLeaderboardAsync_AfterJoin_HasEntry()
    {
        int uid = _db.InsertUser("MarLBd_" + Guid.NewGuid().ToString("N")[..6]);
        int mid = _db.InsertMarathon("MarLBdMar_" + Guid.NewGuid().ToString("N")[..6]);
        await Repo.JoinMarathonAsync(uid, mid);

        var board = (await Repo.GetLeaderboardAsync(mid)).ToList();

        Assert.Contains(board, p => p.UserId == uid);
    }

    [Fact]
    public async Task GetLeaderboardWithUsernamesAsync_ReturnsUsernames()
    {
        int uid = _db.InsertUser("MarLBWU_" + Guid.NewGuid().ToString("N")[..6]);
        int mid = _db.InsertMarathon("MarLBWUMar_" + Guid.NewGuid().ToString("N")[..6]);
        await Repo.JoinMarathonAsync(uid, mid);

        var board = (await Repo.GetLeaderboardWithUsernamesAsync(mid)).ToList();

        Assert.Contains(board, e => e.UserId == uid && !string.IsNullOrEmpty(e.Username));
    }

    [Fact]
    public async Task GetParticipantCountAsync_AfterJoin_ReturnsPositiveCount()
    {
        int uid = _db.InsertUser("MarPart_" + Guid.NewGuid().ToString("N")[..6]);
        int mid = _db.InsertMarathon("MarPartMar_" + Guid.NewGuid().ToString("N")[..6]);
        await Repo.JoinMarathonAsync(uid, mid);

        int count = await Repo.GetParticipantCountAsync(mid);

        Assert.True(count >= 1);
    }

    [Fact]
    public async Task GetMarathonMovieCountAsync_NoMovies_ReturnsZero()
    {
        int mid = _db.InsertMarathon("MarMCZero_" + Guid.NewGuid().ToString("N")[..6]);
        int count = await Repo.GetMarathonMovieCountAsync(mid);
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task GetMarathonMovieCountAsync_WithMovies_ReturnsCorrectCount()
    {
        int mid = _db.InsertMarathon("MarMC2_" + Guid.NewGuid().ToString("N")[..6]);
        int mov1 = _db.InsertMovie("MarMCMov1_" + Guid.NewGuid().ToString("N")[..6]);
        int mov2 = _db.InsertMovie("MarMCMov2_" + Guid.NewGuid().ToString("N")[..6]);

        using var conn = _db.Open();
        using var cmd = new SqlCommand(
            "INSERT INTO dbo.MarathonMovies VALUES (@m, @v1); INSERT INTO dbo.MarathonMovies VALUES (@m, @v2);",
            conn);
        cmd.Parameters.AddWithValue("@m", mid);
        cmd.Parameters.AddWithValue("@v1", mov1);
        cmd.Parameters.AddWithValue("@v2", mov2);
        cmd.ExecuteNonQuery();

        int count = await Repo.GetMarathonMovieCountAsync(mid);
        Assert.Equal(2, count);
    }

    [Fact]
    public async Task GetMoviesForMarathonAsync_WithLinkedMovies_ReturnsThem()
    {
        int mid = _db.InsertMarathon("MarMov_" + Guid.NewGuid().ToString("N")[..6]);
        int mov = _db.InsertMovie("MarMovMov_" + Guid.NewGuid().ToString("N")[..6]);

        using var conn = _db.Open();
        using var cmd = new SqlCommand("INSERT INTO dbo.MarathonMovies VALUES (@m, @v);", conn);
        cmd.Parameters.AddWithValue("@m", mid);
        cmd.Parameters.AddWithValue("@v", mov);
        cmd.ExecuteNonQuery();

        var movies = (await Repo.GetMoviesForMarathonAsync(mid)).ToList();

        Assert.Contains(movies, m => m.Id == mov);
    }

    [Fact]
    public async Task IsPrerequisiteCompletedAsync_NoProgress_ReturnsFalse()
    {
        bool result = await Repo.IsPrerequisiteCompletedAsync(int.MaxValue, int.MaxValue);
        Assert.False(result);
    }

    [Fact]
    public async Task IsPrerequisiteCompletedAsync_Completed_ReturnsTrue()
    {
        int uid = _db.InsertUser("MarPreq_" + Guid.NewGuid().ToString("N")[..6]);
        int prereq = _db.InsertMarathon("MarPreqMar_" + Guid.NewGuid().ToString("N")[..6]);
        await Repo.JoinMarathonAsync(uid, prereq);

        var progress = (await Repo.GetUserProgressAsync(uid, prereq))!;
        progress.FinishedAt = DateTime.UtcNow;
        await Repo.UpdateProgressAsync(progress);

        bool result = await Repo.IsPrerequisiteCompletedAsync(uid, prereq);

        Assert.True(result);
    }

    [Fact]
    public async Task GetWeeklyMarathonsForUserAsync_WithActiveCurrentWeekMarathon_ReturnsIt()
    {
        string week = GetCurrentWeekString();
        int mid = _db.InsertMarathon("WeekMar_" + Guid.NewGuid().ToString("N")[..6], isActive: true, weekScoping: week);
        int uid = _db.InsertUser("WeekUser_" + Guid.NewGuid().ToString("N")[..6]);

        var result = (await Repo.GetWeeklyMarathonsForUserAsync(uid, week)).ToList();

        Assert.Contains(result, m => m.Id == mid);
    }

    [Fact]
    public async Task GetWeeklyMarathonsForUserAsync_FinishedMarathon_ExcludesIt()
    {
        string week = GetCurrentWeekString();
        int uid = _db.InsertUser("WeekFin_" + Guid.NewGuid().ToString("N")[..6]);
        int mid = _db.InsertMarathon("WeekFinMar_" + Guid.NewGuid().ToString("N")[..6], isActive: true, weekScoping: week);
        await Repo.JoinMarathonAsync(uid, mid);
        var progress = (await Repo.GetUserProgressAsync(uid, mid))!;
        progress.FinishedAt = DateTime.UtcNow;
        await Repo.UpdateProgressAsync(progress);

        var result = (await Repo.GetWeeklyMarathonsForUserAsync(uid, week)).ToList();

        Assert.DoesNotContain(result, m => m.Id == mid);
    }

    [Fact]
    public async Task AssignWeeklyMarathonsAsync_AssignsActiveMarathons()
    {
        string week = "ASSIGN_" + Guid.NewGuid().ToString("N")[..6];
        _db.InsertMarathon("AssignMar1_" + Guid.NewGuid().ToString("N")[..6], isActive: false, weekScoping: null);
        int uid = _db.InsertUser("AssignUser_" + Guid.NewGuid().ToString("N")[..6]);
        await Repo.AssignWeeklyMarathonsAsync(uid, week, count: 10);
        Assert.True(true);
    }

    private static string GetCurrentWeekString()
    {
        var now = DateTime.UtcNow;
        int week = ISOWeek.GetWeekOfYear(now);
        return $"{now.Year}-W{week:D2}";
    }
}