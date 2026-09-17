using FluentAssertions;
using TimeTracker.Domain.Common;
using TimeTracker.Domain.TimeTracking;
using Xunit;

namespace TimeTracker.Domain.Tests;

public class DurationTests
{
    [Fact]
    public void FromTimeSpan_Negative_Throws()
    {
        // Arrange
        var ts = TimeSpan.FromSeconds(-1);

        // Act
        var act = () => Duration.FromTimeSpan(ts);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Zero_HasZeroValue()
    {
        // Act
        var value = Duration.Zero;

        // Assert
        value.Value.Should().Be(TimeSpan.Zero);
    }

    [Theory]
    [InlineData(0, "00:00")]
    [InlineData(5, "00:05")]
    [InlineData(65, "01:05")]
    [InlineData(599, "09:59")]
    [InlineData(3600, "01:00:00")]
    [InlineData(3661, "01:01:01")]
    [InlineData(86399, "23:59:59")]
    public void ToHumanReadable_FormatsCorrectly(long seconds, string expected)
    {
        // Act
        var result = Duration.FromSeconds(seconds).ToHumanReadable();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Add_CombinesTwoDurations()
    {
        //Arrange
        var a = Duration.FromSeconds(60);
        var b = Duration.FromSeconds(30);

        //Act
        var result = a.Add(b);

        //Assert
        result.Value.Should().Be(TimeSpan.FromSeconds(90));
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        // Arrange
        var a = Duration.FromSeconds(60);
        var b = Duration.FromSeconds(60);

        // Assert
        a.Should().Be(b);
        (a == b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }
}
