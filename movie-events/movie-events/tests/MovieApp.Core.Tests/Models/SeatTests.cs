using Xunit;
using MovieApp.Core.Models;

namespace MovieApp.Core.Tests.Models;

public class SeatTests
{
    [Theory]
    [InlineData(SeatQuality.Poor, "#FF4D4D")]
    [InlineData(SeatQuality.Standard, "#FFC107")]
    [InlineData(SeatQuality.Optimal, "#4CAF50")]
    [InlineData((SeatQuality)999, "#E0E0E0")]
    public void SeatColor_ReturnsCorrectHexCode(SeatQuality quality, string expectedColor)
    {
        var seat = new Seat
        {
            Row = 1,
            Column = 1,
            Quality = quality
        };

        Assert.Equal(expectedColor, seat.SeatColor);
        Assert.True(seat.IsAvailable);
    }
}