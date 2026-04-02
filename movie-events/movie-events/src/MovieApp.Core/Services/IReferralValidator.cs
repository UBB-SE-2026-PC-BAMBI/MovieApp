namespace MovieApp.Core.Services;

public interface IReferralValidator
{
    Task<bool> IsValidReferralAsync(string referralCode, int currentUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks code validity AND that this friend has not already used the same code for the same event.
    /// </summary>
    Task<bool> IsValidReferralForEventAsync(string referralCode, int currentUserId, int eventId, CancellationToken cancellationToken = default);
}
