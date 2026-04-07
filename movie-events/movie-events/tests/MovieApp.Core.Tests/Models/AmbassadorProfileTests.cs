using Xunit;
using MovieApp.Core.Models;

namespace MovieApp.Core.Tests.Models;

public class AmbassadorProfileTests
{
    [Fact]
    public void AmbassadorProfile_Properties_GetAndSetCorrectly()
    {
        var profile = new AmbassadorProfile
        {
            UserId = 123,
            PermanentCode = "REF-ABC"
        };

        profile.RewardBalance = 50;

        Assert.Equal(123, profile.UserId);
        Assert.Equal("REF-ABC", profile.PermanentCode);
        Assert.Equal(50, profile.RewardBalance);
    }
}