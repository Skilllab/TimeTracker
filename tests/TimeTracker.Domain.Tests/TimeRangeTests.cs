using FluentAssertions;
using Xunit;

namespace TimeTracker.Domain.Tests;

/// <summary>
/// Тесты интервала <see cref="TimeRange"/>.
/// </summary>
public sealed class TimeRangeTests
{
    private static readonly DateTimeOffset Start = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);


    [Fact]
    public void Constructor_WithoutEnd_IsOpen()
    {
        var range = new TimeRange(Start);

        range.IsOpen.Should().BeTrue();
        range.End.Should().BeNull();
    }


    [Fact]
    public void Constructor_EndBeforeStart_Throws()
    {
        var act = () => new TimeRange(Start, Start.AddSeconds(-1));

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ElapsedAt_OpenRange_CountsUpToCurrentMoment()
    {
        var range = new TimeRange(Start);

        range.ElapsedAt(Start.AddSeconds(42)).Should().Be(Duration.From(TimeSpan.FromSeconds(42)));
    }


    [Fact]
    public void ElapsedAt_ClosedRange_IgnoresCurrentMoment()
    {
        var range = new TimeRange(Start, Start.AddSeconds(10));

        range.ElapsedAt(Start.AddHours(5)).Should().Be(Duration.From(TimeSpan.FromSeconds(10)));
    }


    [Fact]
    public void ElapsedAt_MomentBeforeStart_ReturnsZero()
    {
        var range = new TimeRange(Start);

        range.ElapsedAt(Start.AddSeconds(-5)).Should().Be(Duration.Zero);
    }
}
