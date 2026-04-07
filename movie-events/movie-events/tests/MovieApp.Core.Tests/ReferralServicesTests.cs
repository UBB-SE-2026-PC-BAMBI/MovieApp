using System.Text.RegularExpressions;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;
using MovieApp.Core.Services;
using Xunit;

namespace MovieApp.Core.Tests;

public sealed class ReferralServicesTests
{
    [Fact]
    public void Generate_UsesStableUppercasePatternForSameInputs()
    {
        var generator = new ReferralCodeGenerator();

        var code = generator.Generate("alice", 7);

        Assert.Equal($"ALICE{DateTime.UtcNow.Year}7", code);
    }

    [Fact]
    public void Generate_ReturnsAlphanumericCode_AccordingToRequirements()
    {
        var generator = new ReferralCodeGenerator();

        var code = generator.Generate("alice example", 7);

        Assert.Matches(new Regex("^[A-Za-z0-9]+$"), code);
    }

    [Fact]
    public async Task IsValidReferralAsync_ReturnsFalseWhenCodeDoesNotExist()
    {
        var repository = new StubAmbassadorRepository();
        var validator = new ReferralValidator(repository);

        var result = await validator.IsValidReferralAsync("MISSING", currentUserIdentifier: 10);

        Assert.False(result);
    }

    [Fact]
    public async Task IsValidReferralAsync_ReturnsFalseForOwnCode()
    {
        var repository = new StubAmbassadorRepository
        {
            OwnersByCode = { ["OWNCODE"] = 10 },
        };
        var validator = new ReferralValidator(repository);

        var result = await validator.IsValidReferralAsync("OWNCODE", currentUserIdentifier: 10);

        Assert.False(result);
    }

    [Fact]
    public async Task IsValidReferralAsync_ReturnsTrueForAnotherUsersCode()
    {
        var repository = new StubAmbassadorRepository
        {
            OwnersByCode = { ["FRIENDCODE"] = 42 },
        };
        var validator = new ReferralValidator(repository);

        var result = await validator.IsValidReferralAsync("FRIENDCODE", currentUserIdentifier: 10);

        Assert.True(result);
    }

    [Fact]
    public async Task LogReferralUsageAsync_AddsLogAndAttemptsRewardWhenCodeExists()
    {
        var repository = new StubAmbassadorRepository
        {
            OwnersByCode = { ["FRIENDCODE"] = 42 },
        };
        var service = new ReferralLogService(repository);

        await service.LogReferralUsageAsync("FRIENDCODE", friendIdentifier: 10, eventIdentifier: 88);

        var loggedUsage = Assert.Single(repository.LogEntries);
        Assert.Equal((42, 10, 88), loggedUsage);
        Assert.Equal([42], repository.TryApplyRewardCalls);
    }

    [Fact]
    public async Task LogReferralUsageAsync_DoesNothingWhenCodeDoesNotExist()
    {
        var repository = new StubAmbassadorRepository();
        var service = new ReferralLogService(repository);

        await service.LogReferralUsageAsync("MISSING", friendIdentifier: 10, eventIdentifier: 88);

        Assert.Empty(repository.LogEntries);
        Assert.Empty(repository.TryApplyRewardCalls);
    }

    // ── Event-scope referral tests ─────────────────────────────────────────────

    [Fact]
    public async Task IsValidReferralForEventAsync_ReturnsFalse_WhenAlreadyUsedForSameEvent()
    {
        var repository = new StubAmbassadorRepository
        {
            OwnersByCode = { ["FRIENDCODE"] = 42 },
            ExistingLogs = { (42, 10, 88) },
        };
        var validator = new ReferralValidator(repository);

        var result = await validator.IsValidReferralForEventAsync("FRIENDCODE", currentUserIdentifier: 10, eventIdentifier: 88);

        Assert.False(result);
    }

    [Fact]
    public async Task IsValidReferralForEventAsync_ReturnsTrue_WhenDifferentEvent()
    {
        var repository = new StubAmbassadorRepository
        {
            OwnersByCode = { ["FRIENDCODE"] = 42 },
            ExistingLogs = { (42, 10, 88) }, // used for event 88
        };
        var validator = new ReferralValidator(repository);

        // event 99 has never been used → should be valid
        var result = await validator.IsValidReferralForEventAsync("FRIENDCODE", currentUserIdentifier: 10, eventIdentifier: 99);

        Assert.True(result);
    }

    [Fact]
    public async Task IsValidReferralForEventAsync_ReturnsFalse_WhenCodeDoesNotExist()
    {
        var repository = new StubAmbassadorRepository();
        var validator = new ReferralValidator(repository);

        var result = await validator.IsValidReferralForEventAsync("MISSING", currentUserIdentifier: 10, eventIdentifier: 88);

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
            var code = OwnersByCode.FirstOrDefault(entry => entry.Value == userId).Key;
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
    [Fact]
    public void Generate_WhenUsernameContainsNumbers_KeepsNumbersInOutput()
    {
        const string Username = "alice42";
        const int UserId = 7;
        const string ExpectedPrefix = "ALICE42";
        var generator = new ReferralCodeGenerator();

        var code = generator.Generate(Username, UserId);

        Assert.StartsWith(ExpectedPrefix, code, StringComparison.Ordinal);
    }

    [Fact]
    public void Generate_WhenUsernameIsEmpty_ReturnsCodeWithYearAndUserId()
    {
        const string Username = "";
        const int UserId = 5;
        string expectedCode = $"{DateTime.UtcNow.Year}{UserId}";
        var generator = new ReferralCodeGenerator();

        var code = generator.Generate(Username, UserId);

        Assert.Equal(expectedCode, code);
    }

    [Fact]
    public void Generate_WhenCalledTwiceWithSameInput_ReturnsSameCode()
    {
        const string Username = "alice";
        const int UserId = 7;
        var generator = new ReferralCodeGenerator();

        var firstCode = generator.Generate(Username, UserId);
        var secondCode = generator.Generate(Username, UserId);

        Assert.Equal(firstCode, secondCode);
    }
    [Fact]
    public void Generate_WhenUsernameHasSpecialCharacters_StripsNonAlphanumericCharacters()
    {
        const string Username = "al!ce@ex#ample";
        const int UserId = 3;
        const string ExpectedPrefix = "ALCEEXAMPLE";
        var generator = new ReferralCodeGenerator();

        var code = generator.Generate(Username, UserId);

        Assert.StartsWith(ExpectedPrefix, code, StringComparison.Ordinal);
        Assert.Matches(new Regex("^[A-Za-z0-9]+$"), code);
    }

    [Fact]
    public void Generate_WhenUserIdIsZero_StillProducesValidCode()
    {
        const string Username = "alice";
        const int UserId = 0;
        string expectedCode = $"ALICE{DateTime.UtcNow.Year}0";
        var generator = new ReferralCodeGenerator();

        var code = generator.Generate(Username, UserId);

        Assert.Equal(expectedCode, code);
        Assert.Matches(new Regex("^[A-Za-z0-9]+$"), code);
    }

}
