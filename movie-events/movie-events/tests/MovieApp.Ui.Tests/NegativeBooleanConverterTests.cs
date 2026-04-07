using MovieApp.Ui.Converters;
using Xunit;

namespace MovieApp.Ui.Tests;

public sealed class NegativeBooleanConverterTests
{
    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void Convert_BooleanValue_ReturnsInvertedBoolean(bool value, bool expected)
    {
        NegativeBooleanConverter converter = new NegativeBooleanConverter();

        object result = converter.Convert(value, typeof(bool), null!, string.Empty);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void ConvertBack_BooleanValue_ReturnsInvertedBoolean(bool value, bool expected)
    {
        NegativeBooleanConverter converter = new NegativeBooleanConverter();

        object result = converter.ConvertBack(value, typeof(bool), null!, string.Empty);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Convert_NonBooleanValue_ReturnsOriginalValue()
    {
        NegativeBooleanConverter converter = new NegativeBooleanConverter();

        object result = converter.Convert("text", typeof(string), null!, string.Empty);

        Assert.Equal("text", result);
    }
}