using FluentAssertions;
using Xunit;

namespace TimeTracker.Domain.Tests;


public sealed class TimeEntryTests
{
    private static readonly DateTimeOffset Start = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Constructor_TrimsDescription()
    {
        var entry = Create("  Работа  ");

        entry.Description.Should().Be("Работа");
    }

    [Fact]
    public void Constructor_WithoutDescription_LeavesItEmpty()
    {
        var entry = Create(null!);

        entry.Description.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_TooLongDescription_Throws()
    {
        var act = () => Create(new string('а', 501));

        act.Should().Throw<InvalidTimeEntryException>();
    }

    [Fact]
    public void Constructor_EndBeforeStart_Throws()
    {
        var act = () => new TimeEntry(Guid.NewGuid(), string.Empty, Start, Start.AddSeconds(-1), 0, false, null);

        act.Should().Throw<InvalidTimeEntryException>();
    }

    [Fact]
    public void Constructor_NegativePauses_Throws()
    {
        var act = () => new TimeEntry(Guid.NewGuid(), string.Empty, Start, null, -1, false, null);

        act.Should().Throw<InvalidTimeEntryException>();
    }

    [Fact]
    public void ElapsedAt_ClosedEntry_ExcludesPauses()
    {
        var entry = new TimeEntry(Guid.NewGuid(), string.Empty, Start, Start.AddMinutes(10), 120, false, null);

        entry.ElapsedAt(Start.AddHours(5)).Should().Be(Duration.From(TimeSpan.FromMinutes(8)));
    }

    [Fact]
    public void ElapsedAt_OpenEntry_CountsUpToCurrentMoment()
    {
        var entry = new TimeEntry(Guid.NewGuid(), string.Empty, Start, null, 0, false, null);

        entry.ElapsedAt(Start.AddSeconds(42)).Should().Be(Duration.From(TimeSpan.FromSeconds(42)));
    }

    private static TimeEntry Create(string description)
    {
        return new TimeEntry(Guid.NewGuid(), description, Start, Start.AddMinutes(5), 0, false, null);
    }
}
