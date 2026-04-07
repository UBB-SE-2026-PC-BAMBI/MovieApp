using Xunit;
using MovieApp.Core.Models;

namespace MovieApp.Core.Tests.Models;

public class JoinEventResultTests
{
    [Fact]
    public void JoinEventResult_DefaultProperties_AreSetCorrectly()
    {
        var result = new JoinEventResult();

        Assert.False(result.Success);
        Assert.Equal(string.Empty, result.Message);
    }

    [Fact]
    public void JoinEventResult_Properties_GetAndSetCorrectly()
    {
        var result = new JoinEventResult
        {
            Success = true,
            Message = "Successfully joined the event!"
        };

        Assert.True(result.Success);
        Assert.Equal("Successfully joined the event!", result.Message);
    }
}