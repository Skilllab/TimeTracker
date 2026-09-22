using FluentAssertions;
using Xunit;

namespace TimeTracker.Domain.Tests;


public sealed class TimerSessionTests
{
    private static readonly DateTimeOffset Start = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Start_FromIdle_OpensRange()
    {
        var session = new TimerSession();

        session.Start(Start);

        session.State.Should().Be(TimerState.Running);
        session.Current!.Start.Should().Be(Start);
    }

    [Fact]
    public void Pause_FromRunning_SetsPaused()
    {
        var session = new TimerSession();
        session.Start(Start);

        session.Pause(Start.AddSeconds(10));

        session.State.Should().Be(TimerState.Paused);
    }

    [Fact]
    public void Resume_FromPaused_ReturnsToRunning()
    {
        var session = new TimerSession();
        session.Start(Start);
        session.Pause(Start.AddSeconds(10));

        session.Resume(Start.AddMinutes(1));

        session.State.Should().Be(TimerState.Running);
    }

    [Fact]
    public void Start_Twice_Throws()
    {
        var session = new TimerSession();
        session.Start(Start);

        var act = () => session.Start(Start.AddSeconds(10));

        act.Should().Throw<InvalidTimerStateException>()
            .WithMessage("Нельзя начать запись, если она уже начата или завершена.");
    }

    [Fact]
    public void Pause_WithoutStart_Throws()
    {
        var session = new TimerSession();

        var act = () => session.Pause(Start);

        act.Should().Throw<InvalidTimerStateException>()
            .WithMessage("Нельзя приостановить запись, которая не идет.");
    }

    [Fact]
    public void Pause_WhilePaused_Throws()
    {
        var session = new TimerSession();
        session.Start(Start);
        session.Pause(Start.AddSeconds(10));

        var act = () => session.Pause(Start.AddSeconds(20));

        act.Should().Throw<InvalidTimerStateException>()
            .WithMessage("Нельзя приостановить запись, которая не идет.");
    }

    [Fact]
    public void Resume_WithoutPause_Throws()
    {
        var session = new TimerSession();
        session.Start(Start);

        var act = () => session.Resume(Start.AddSeconds(10));

        act.Should().Throw<InvalidTimerStateException>()
            .WithMessage("Нельзя возобновить запись, которая не приостановлена.");
    }

    [Fact]
    public void Stop_FromIdle_Throws()
    {
        var session = new TimerSession();

        var act = () => session.Stop(Start);

        act.Should().Throw<InvalidTimerStateException>()
            .WithMessage("Нельзя завершить незапущенную запись.");
    }

    [Fact]
    public void Start_AfterFinish_Throws()
    {
        var session = new TimerSession();
        session.Start(Start);
        session.Stop(Start.AddSeconds(10));

        var act = () => session.Start(Start.AddMinutes(1));

        act.Should().Throw<InvalidTimerStateException>()
            .WithMessage("Нельзя начать запись, если она уже начата или завершена.");
    }

    [Fact]
    public void ElapsedAt_WhilePaused_DoesNotGrow()
    {
        var session = new TimerSession();
        session.Start(Start);
        session.Pause(Start.AddSeconds(10));

        session.ElapsedAt(Start.AddMinutes(5)).Should().Be(Duration.From(TimeSpan.FromSeconds(10)));
    }

    [Fact]
    public void ElapsedAt_AfterResume_ExcludesPauseTime()
    {
        var session = new TimerSession();
        session.Start(Start);
        session.Pause(Start.AddSeconds(10));
        session.Resume(Start.AddMinutes(1));

        session.ElapsedAt(Start.AddMinutes(1).AddSeconds(5)).Should().Be(Duration.From(TimeSpan.FromSeconds(15)));
    }

    [Fact]
    public void ElapsedAt_AfterStop_KeepsFrozenValue()
    {
        var session = new TimerSession();
        session.Start(Start);
        session.Pause(Start.AddSeconds(10));
        session.Stop(Start.AddMinutes(1));

        session.State.Should().Be(TimerState.Finished);
        session.ElapsedAt(Start.AddHours(5)).Should().Be(Duration.From(TimeSpan.FromSeconds(10)));
    }

    [Fact]
    public void PausedSeconds_SumsAllPauses()
    {
        var session = new TimerSession();
        session.Start(Start);
        session.Pause(Start.AddSeconds(10));
        session.Resume(Start.AddMinutes(1));
        session.Pause(Start.AddMinutes(2));
        session.Resume(Start.AddMinutes(3));

        session.PausedSeconds.Should().Be(110);
    }

    [Fact]
    public void Restore_FromOpenEntry_StartsPaused()
    {
        var session = new TimerSession();
        var entry = new TimeEntry(Guid.NewGuid(), string.Empty, Start, null, 0, null, false, null);

        session.Restore(entry, Start.AddHours(1));

        session.State.Should().Be(TimerState.Paused);
        session.Current!.Start.Should().Be(Start);
        session.PausedSeconds.Should().Be(0);
    }

    [Fact]
    public void Restore_FromPausedEntry_KeepsPausedAt()
    {
        var session = new TimerSession();
        var entry = new TimeEntry(Guid.NewGuid(), string.Empty, Start, null, 300, Start.AddMinutes(15), false, null);

        session.Restore(entry, Start.AddHours(1));

        session.State.Should().Be(TimerState.Paused);
        session.PausedAt.Should().Be(Start.AddMinutes(15));
        session.PausedSeconds.Should().Be(300);
        session.ElapsedAt(Start.AddHours(1)).Should().Be(Duration.From(TimeSpan.FromMinutes(10)));
    }


    [Fact]
    public void Restore_FromClosedEntry_Throws()
    {
        var session = new TimerSession();
        var entry = new TimeEntry(Guid.NewGuid(), string.Empty, Start, Start.AddMinutes(5), 0, null, false, null);

        var act = () => session.Restore(entry, Start.AddHours(1));

        act.Should().Throw<InvalidTimerStateException>();
    }
}
