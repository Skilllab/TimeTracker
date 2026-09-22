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
        var act = () => new TimeEntry(
            id: Guid.NewGuid(),
            description: string.Empty,
            startedAt: Start,
            endedAt: Start.AddSeconds(-1),
            pausedSeconds: 0,
            pausedAt: null,
            isBillable: false,
            projectId: null);

        act.Should().Throw<InvalidTimeEntryException>();
    }

    [Fact]
    public void Constructor_NegativePauses_Throws()
    {
        var act = () => new TimeEntry(
            id: Guid.NewGuid(),
            description: string.Empty,
            startedAt: Start,
            endedAt: null,
            pausedSeconds: -1,
            pausedAt: null,
            isBillable: false,
            projectId: null);

        act.Should().Throw<InvalidTimeEntryException>();
    }

    [Fact]
    public void ElapsedAt_ClosedEntry_ExcludesPauses()
    {
        var entry = new TimeEntry(
            id: Guid.NewGuid(),
            description: string.Empty,
            startedAt: Start,
            endedAt: Start.AddMinutes(10),
            pausedSeconds: 120,
            pausedAt: null,
            isBillable: false,
            projectId: null);

        entry.ElapsedAt(Start.AddHours(5)).Should().Be(Duration.From(TimeSpan.FromMinutes(8)));
    }

    [Fact]
    public void ElapsedAt_OpenEntry_CountsUpToCurrentMoment()
    {
        var entry = new TimeEntry(
            id: Guid.NewGuid(),
            description: string.Empty,
            startedAt: Start,
            endedAt: null,
            pausedSeconds: 0,
            pausedAt: null,
            isBillable: false,
            projectId: null);

        entry.ElapsedAt(Start.AddSeconds(42)).Should().Be(Duration.From(TimeSpan.FromSeconds(42)));
    }

    private static TimeEntry Create(string description)
    {
        return new TimeEntry(
            id: Guid.NewGuid(),
            description: description,
            startedAt: Start,
            endedAt: null,
            pausedSeconds: 0,
            pausedAt: null,
            isBillable: false,
            projectId: null);
    }

    [Fact]
    public void Pause_SetsPausedAt()
    {
        var entry = Create("Работа");

        var paused = entry.Pause(Start.AddMinutes(1));

        paused.IsPaused.Should().BeTrue();
        paused.PausedAt.Should().Be(Start.AddMinutes(1));
        paused.PausedSeconds.Should().Be(0);
    }

    [Fact]
    public void Resume_AddsPauseToTotal()
    {
        var entry = Create("Работа").Pause(Start.AddMinutes(1));

        var resumed = entry.Resume(Start.AddMinutes(3));

        resumed.IsPaused.Should().BeFalse();
        resumed.PausedAt.Should().BeNull();
        resumed.PausedSeconds.Should().Be(120);
    }

    [Fact]
    public void Close_WhilePaused_CountsOpenPause()
    {
        var entry = Create(description: "Работа").Pause(Start.AddMinutes(1));

        var closed = entry.Close(Start.AddMinutes(4));

        closed.IsOpen.Should().BeFalse();
        closed.EndedAt.Should().Be(Start.AddMinutes(4));
        closed.PausedSeconds.Should().Be(180);
    }

    [Fact]
    public void Pause_OnClosedEntry_Throws()
    {
        var entry = Create("Работа").Close(Start.AddMinutes(5));

        var act = () => entry.Pause(Start.AddMinutes(6));

        act.Should().Throw<InvalidTimeEntryException>();
    }

    [Fact]
    public void Resume_WithoutPause_Throws()
    {
        var entry = Create("Работа");

        var act = () => entry.Resume(Start.AddMinutes(1));

        act.Should().Throw<InvalidTimeEntryException>();
    }
}
