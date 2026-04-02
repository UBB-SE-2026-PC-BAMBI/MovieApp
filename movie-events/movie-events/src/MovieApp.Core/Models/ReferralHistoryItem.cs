using System;

namespace MovieApp.Core.Models;

public class ReferralHistoryItem
{
    public string FriendName { get; set; } = string.Empty;
    public string EventTitle { get; set; } = string.Empty;
    public DateTime UsedAt { get; set; }
    public string FormattedDate => UsedAt.ToString("g");
}
