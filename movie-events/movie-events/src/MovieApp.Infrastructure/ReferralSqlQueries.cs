namespace MovieApp.Infrastructure;

/// <summary>
/// Centralizes the referral-related queries.
/// </summary>
public static class ReferralSqlQueries
{
    /// <summary>
    /// Query to count how many unique users (friends) have used a specific referral code for a specific event.
    /// </summary>
    public const string CountUniqueFriendsForEvent = """
        SELECT COUNT(DISTINCT rl.FriendID)
        FROM dbo.ReferralLog rl
        JOIN dbo.AmbassadorProfile ap ON rl.AmbassadorID = ap.UserId
        WHERE ap.referral_code = @referralCode
          AND rl.EventID = @eventId;
        """;

    /// <summary>
    /// Checks if a referral code exists in the AmbassadorProfile table.
    /// </summary>
    public const string CheckReferralCodeExists = """
        SELECT CAST(CASE WHEN EXISTS (
            SELECT 1 FROM dbo.AmbassadorProfile WHERE referral_code = @referralCode
        ) THEN 1 ELSE 0 END AS BIT);
        """;

    /// <summary>
    /// Gets the referral code for a specific user ID.
    /// </summary>
    public const string SelectReferralCodeByUserId = """
        SELECT referral_code
        FROM dbo.AmbassadorProfile
        WHERE UserId = @userId;
        """;

    /// <summary>
    /// Inserts a newly generated referral code for an ambassador (user).
    /// </summary>
    public const string InsertAmbassadorProfile = """
        INSERT INTO dbo.AmbassadorProfile (UserId, referral_code)
        VALUES (@userId, @referralCode);
        """;

    /// <summary>
    /// Gets the User ID who owns the given referral code.
    /// </summary>
    public const string SelectUserIdByReferralCode = """
        SELECT UserId
        FROM dbo.AmbassadorProfile
        WHERE referral_code = @referralCode;
        """;

    /// <summary>
    /// Inserts a new referral log entry when an enrollment happens.
    /// </summary>
    public const string InsertReferralLog = """
        INSERT INTO dbo.ReferralLog (AmbassadorID, FriendID, EventID)
        VALUES (@ambassadorId, @friendId, @eventId);
        """;

    /// <summary>
    /// Checks if a user has 3 logs, deletes the 3 oldest, and increments reward_balance atomically.
    /// </summary>
    public const string ApplyRewardIfEligible = """
        DECLARE @LogsToClear TABLE (Id INT);

        INSERT INTO @LogsToClear
        SELECT TOP 3 Id
        FROM dbo.ReferralLog
        WHERE AmbassadorID = @ambassadorId
        ORDER BY UsedAt ASC;

        IF (SELECT COUNT(*) FROM @LogsToClear) = 3
        BEGIN
            DELETE FROM dbo.ReferralLog WHERE Id IN (SELECT Id FROM @LogsToClear);
            UPDATE dbo.AmbassadorProfile SET reward_balance = reward_balance + 1 WHERE UserId = @ambassadorId;
            SELECT CAST(1 AS BIT);
        END
        ELSE
        BEGIN
            SELECT CAST(0 AS BIT);
        END
        """;

    /// <summary>
    /// Retrieves the referral usage history for a specific ambassador.
    /// </summary>
    public const string SelectReferralHistoryByAmbassadorId = """
        SELECT 
            u.Username AS FriendName,
            e.Title AS EventTitle,
            l.UsedAt
        FROM dbo.ReferralLog l
        INNER JOIN dbo.Users u ON l.FriendID = u.Id
        INNER JOIN dbo.Events e ON l.EventID = e.Id
        WHERE l.AmbassadorID = @ambassadorId
        ORDER BY l.UsedAt DESC;
        """;

    /// <summary>
    /// Gets the current reward_balance for a user.
    /// </summary>
    public const string SelectRewardBalance = """
        SELECT reward_balance
        FROM dbo.AmbassadorProfile
        WHERE UserId = @userId;
        """;

    /// <summary>
    /// Safely decrements reward_balance by 1, minimum 0.
    /// </summary>
    public const string DecrementRewardBalance = """
        UPDATE dbo.AmbassadorProfile
        SET reward_balance = CASE WHEN reward_balance > 0 THEN reward_balance - 1 ELSE 0 END
        WHERE UserId = @userId;
        """;
}
