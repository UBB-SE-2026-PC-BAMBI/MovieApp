// <copyright file="TriviaRewardSqlQueries.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>
namespace MovieApp.Infrastructure;

/// <summary>
/// Centralizes the trivia-reward queries used by <see cref="SqlTriviaRewardRepository"/>.
/// </summary>
public static class TriviaRewardSqlQueries
{
    /// <summary>
    /// Gets the SQL command used to insert a new trivia reward for a specific user.
    /// </summary>
    public const string Insert = """
        INSERT INTO dbo.TriviaRewards (UserId)
        VALUES (@userId);
        """;

    /// <summary>
    /// Gets the SQL query used to retrieve unredeemed trivia rewards belonging to a specific user.
    /// </summary>
    public const string SelectUnredeemedByUser = """
        SELECT Id, UserId, IsRedeemed, CreatedAt
        FROM dbo.TriviaRewards
        WHERE UserId = @userId
          AND IsRedeemed = 0;
        """;

    /// <summary>
    /// Gets the SQL command used to update a specific reward's status to redeemed.
    /// </summary>
    public const string MarkAsRedeemed = """
        UPDATE dbo.TriviaRewards
        SET IsRedeemed = 1
        WHERE Id = @rewardId;
        """;
}