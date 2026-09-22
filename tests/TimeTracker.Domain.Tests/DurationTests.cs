using FluentAssertions;
using Xunit;

namespace TimeTracker.Domain.Tests;

public sealed class DurationTests
{

    [Fact]
    public void From_NegativeValue_ThrowException()
    {
        var act = () => Duration.From(TimeSpan.FromSeconds(-1));

        act.Should().Throw<ArgumentOutOfRangeException>();
    }


    [Fact]
    public void From_ZeroValue_ReturnsZeroDuration()
    {
        Duration.From(TimeSpan.Zero).Should().Be(Duration.Zero);
    }


    [Theory]
    [InlineData(0, "00:00")]
    [InlineData(7, "00:07")]
    [InlineData(59, "00:59")]
    [InlineData(60, "01:00")]
    [InlineData(61, "01:01")]
    [InlineData(600, "10:00")]
    [InlineData(3599, "59:59")]
    [InlineData(3600, "60:00")]
    public void ToClockString_FormatMinutesAndSeconds(int seconds, string expected)
    {
        Duration.From(TimeSpan.FromSeconds(seconds)).ToClockString().Should().Be(expected);
    }

    [Fact]
    public void ToClockString_WithoutMicroseconds()
    {
        Duration.From(TimeSpan.FromMilliseconds(1900)).ToClockString().Should().Be("00:01");
    }
}
