using Xunit;
using MovieApp.Core.Models;

namespace MovieApp.Core.Tests.Models;

public class MarathonTests
{
    [Fact]
    public void IsElite_WithPrerequisite_ReturnsTrue()
    {
        var marathon = new Marathon { Id = 1, Title = "Sequel Trilogy", PrerequisiteMarathonId = 42 };

        Assert.True(marathon.IsElite);
    }

    [Fact]
    public void IsElite_WithoutPrerequisite_ReturnsFalse()
    {
        var marathon = new Marathon { Id = 2, Title = "Original Trilogy" };

        Assert.False(marathon.IsElite);
    }
}