using FluentAssertions;
using TimeTracker.Domain.Common;
using TimeTracker.Domain.TimeTracking;
using Xunit;

namespace TimeTracker.Domain.Tests;

public class TimeRangeTests
{
    private static readonly DateTimeOffset Now = new(2025, 1, 15, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Ctor_EndBeforeStart_Throws()
    {
        // Act
        var act = () => new TimeRange(Now, Now.AddMinutes(-1));

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Ctor_EndEqualsStart_IsValid()
    {
        // Act
        var range = new TimeRange(Now, Now);

        // Assert
        range.Start.Should().Be(Now);
        range.End.Should().Be(Now);
        range.IsRunning.Should().BeFalse();
    }

    [Fact]
    public void Ctor_EndNull_IsRunning()
    {
        // Act
        var range = new TimeRange(Now, null);

        // Assert
        range.IsRunning.Should().BeTrue();
    }

    [Fact]
    public void DurationAt_ForStoppedRange_ReturnsFixed()
    {
        // Arrange
        var range = new TimeRange(Now, Now.AddMinutes(30));

        // Act
        var duration = range.DurationAt(Now.AddHours(5));

        // Assert
        duration.Value.Should().Be(TimeSpan.FromMinutes(30));
    }

    [Fact]
    public void DurationAt_ForRunningRange_ReturnsFromStartToMoment()
    {
        // Arrange
        var range = new TimeRange(Now, null);

        // Act
        var duration = range.DurationAt(Now.AddMinutes(15));

        // Assert
        duration.Value.Should().Be(TimeSpan.FromMinutes(15));
    }

    [Fact]
    public void DurationAt_BeforeStart_Throws()
    {
        // Arrange
        var range = new TimeRange(Now, null);

        // Act
        var act = () => range.DurationAt(Now.AddMinutes(-1));

        // Assert
        act.Should().Throw<DomainException>();
    }
}
