using System.Text.RegularExpressions;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;
using MovieApp.Core.Services;
using Xunit;

namespace MovieApp.Core.Tests;

public sealed class ReferralServicesTests
{
    [Fact]
    public void Generate_ValidInputs_ReturnsStableUppercasePattern()
    {
        ReferralCodeGenerator generator = new ReferralCodeGenerator();

        string code = generator.Generate("alice", 7);

        Assert.Equal($"ALICE{DateTime.UtcNow.Year}7", code);
    }

    [Fact]
    public void Generate_ValidInput_ReturnsAlphanumericCode()
    {
        ReferralCodeGenerator generator = new ReferralCodeGenerator();

        string code = generator.Generate("alice example", 7);

        Assert.Matches(new Regex("^[A-Za-z0-9]+$"), code);
    }

    [Fact]
    public async Task IsValidReferralAsync_CodeDoesNotExist_ReturnsFalse()
    {
        StubAmbassadorRepository repository = new StubAmbassadorRepository();
        ReferralValidator validator = new ReferralValidator(repository);

        bool result = await validator.IsValidReferralAsync("MISSING", currentUserId: 10);

        Assert.False(result);
    }

    [Fact]
    public async Task IsValidReferralAsync_UserIsCodeOwner_ReturnsFalse()
    {
        StubAmbassadorRepository repository = new StubAmbassadorRepository
        {
            OwnersByCode = { ["OWNCODE"] = 10 },
        };
        ReferralValidator validator = new ReferralValidator(repository);

        bool result = await validator.IsValidReferralAsync("OWNCODE", currentUserId: 10);

        Assert.False(result);
    }

    [Fact]
    public async Task IsValidReferralAsync_ValidCodeFromAnotherUser_ReturnsTrue()
    {
        StubAmbassadorRepository repository = new StubAmbassadorRepository
        {
            OwnersByCode = { ["FRIENDCODE"] = 42 },
        };
        ReferralValidator validator = new ReferralValidator(repository);

        bool result = await validator.IsValidReferralAsync("FRIENDCODE", currentUserId: 10);

        Assert.True(result);
    }

    [Fact]
    public async Task LogReferralUsageAsync_ValidCode_AddsLogAndAppliesReward()
    {
        StubAmbassadorRepository repository = new StubAmbassadorRepository
        {
            OwnersByCode = { ["FRIENDCODE"] = 42 },
        };
        ReferralLogService service = new ReferralLogService(repository);

        await service.LogReferralUsageAsync("FRIENDCODE", friendIdentifier: 10, eventIdentifier: 88);

        (int AmbassadorId, int FriendId, int EventId) loggedUsage = Assert.Single(repository.LogEntries);
        Assert.Equal((42, 10, 88), loggedUsage);
        Assert.Equal([42], repository.TryApplyRewardCalls);
    }

    [Fact]
    public async Task LogReferralUsageAsync_CodeDoesNotExist_TakesNoAction()
    {
        StubAmbassadorRepository repository = new StubAmbassadorRepository();
        ReferralLogService service = new ReferralLogService(repository);

        await service.LogReferralUsageAsync("MISSING", friendIdentifier: 10, eventIdentifier: 88);

        Assert.Empty(repository.LogEntries);
        Assert.Empty(repository.TryApplyRewardCalls);
    }

    // ── Event-scope referral tests ─────────────────────────────────────────────

    [Fact]
    public async Task IsValidReferralForEventAsync_AlreadyUsedForEvent_ReturnsFalse()
    {
        StubAmbassadorRepository repository = new StubAmbassadorRepository
        {
            OwnersByCode = { ["FRIENDCODE"] = 42 },
            ExistingLogs = { (42, 10, 88) },
        };
        ReferralValidator validator = new ReferralValidator(repository);

        bool result = await validator.IsValidReferralForEventAsync("FRIENDCODE", currentUserId: 10, eventId: 88);

        Assert.False(result);
    }

    [Fact]
    public async Task IsValidReferralForEventAsync_NotUsedForEvent_ReturnsTrue()
    {
        StubAmbassadorRepository repository = new StubAmbassadorRepository
        {
            OwnersByCode = { ["FRIENDCODE"] = 42 },
            ExistingLogs = { (42, 10, 88) }, // used for event 88
        };
        ReferralValidator validator = new ReferralValidator(repository);

        // event 99 has never been used → should be valid
        bool result = await validator.IsValidReferralForEventAsync("FRIENDCODE", currentUserId: 10, eventId: 99);

        Assert.True(result);
    }

    [Fact]
    public async Task IsValidReferralForEventAsync_CodeDoesNotExist_ReturnsFalse()
    {
        StubAmbassadorRepository repository = new StubAmbassadorRepository();
        ReferralValidator validator = new ReferralValidator(repository);

        bool result = await validator.IsValidReferralForEventAsync("MISSING", currentUserId: 10, eventId: 88);

        Assert.False(result);
    }

    private sealed class StubAmbassadorRepository : IAmbassadorRepository
    {
        public Dictionary<string, int> OwnersByCode { get; } = new(StringComparer.OrdinalIgnoreCase);

        public List<(int AmbassadorId, int FriendId, int EventId)> LogEntries { get; } = [];

        public List<int> TryApplyRewardCalls { get; } = [];

        /// <summary>Pre-seeded log entries for HasReferralLogAsync checks.</summary>
        public HashSet<(int AmbassadorId, int FriendId, int EventId)> ExistingLogs { get; } = [];

        public Task<bool> IsReferralCodeValidAsync(string referralCode, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(OwnersByCode.ContainsKey(referralCode));
        }

        public Task<string?> GetReferralCodeAsync(int userId, CancellationToken cancellationToken = default)
        {
            string code = OwnersByCode.FirstOrDefault(entry => entry.Value == userId).Key;
            return Task.FromResult(string.IsNullOrWhiteSpace(code) ? null : code);
        }

        public Task CreateAmbassadorProfileAsync(int userId, string referralCode, CancellationToken cancellationToken = default)
        {
            OwnersByCode[referralCode] = userId;
            return Task.CompletedTask;
        }

        public Task<int?> GetUserIdByReferralCodeAsync(string referralCode, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(OwnersByCode.TryGetValue(referralCode, out var ownerId) ? (int?)ownerId : null);
        }

        public Task AddReferralLogAsync(int ambassadorId, int friendId, int eventId, CancellationToken cancellationToken = default)
        {
            LogEntries.Add((ambassadorId, friendId, eventId));
            return Task.CompletedTask;
        }

        public Task<bool> TryApplyRewardAsync(int ambassadorId, CancellationToken cancellationToken = default)
        {
            TryApplyRewardCalls.Add(ambassadorId);
            return Task.FromResult(true);
        }

        public Task<IEnumerable<ReferralHistoryItem>> GetReferralHistoryAsync(int ambassadorId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IEnumerable<ReferralHistoryItem>>([]);
        }

        public Task<int> GetRewardBalanceAsync(int userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(0);
        }

        public Task DecrementRewardBalanceAsync(int userId, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<bool> HasReferralLogAsync(int ambassadorId, int friendId, int eventId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ExistingLogs.Contains((ambassadorId, friendId, eventId)));
        }
    }
}
