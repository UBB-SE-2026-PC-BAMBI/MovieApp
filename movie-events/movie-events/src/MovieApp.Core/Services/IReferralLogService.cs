namespace MovieApp.Core.Services;

public interface IReferralLogService
{
    Task LogReferralUsageAsync(string referralCode, int friendId, int eventId, CancellationToken cancellationToken = default);
}
