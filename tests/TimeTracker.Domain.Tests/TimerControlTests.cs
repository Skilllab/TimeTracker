using FluentAssertions;
using TimeTracker.Application;
using Xunit;

namespace TimeTracker.Domain.Tests;


public sealed class TimerControlTests
{
    private static readonly DateTimeOffset Start = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    private static (ITimerControl Control, FakeTimeProvider Time) CreateControl()
    {
        var time = new FakeTimeProvider(Start);
        ITimerControl control = new TimerControl(new TimerSession(), time);

        return (control, time);
    }

    [Fact]
    public void BeforeStart_ElapsedIsZero()
    {
        var (control, _) = CreateControl();

        control.IsRunning.Should().BeFalse();
        control.IsPaused.Should().BeFalse();
        control.IsFinished.Should().BeFalse();
        control.GetElapsed().Should().Be(Duration.Zero);
    }

    [Fact]
    public void Start_ThenTimeAdvance_CountsUp()
    {
        var (control, time) = CreateControl();

        control.Start();
        time.Advance(TimeSpan.FromSeconds(65));

        control.IsRunning.Should().BeTrue();
        control.GetElapsed().ToClockString().Should().Be("01:05");
    }

    [Fact]
    public void Pause_FreezesElapsed()
    {
        var (control, time) = CreateControl();
        control.Start();
        time.Advance(TimeSpan.FromSeconds(10));

        control.Pause();
        time.Advance(TimeSpan.FromMinutes(5));

        control.IsPaused.Should().BeTrue();
        control.GetElapsed().ToClockString().Should().Be("00:10");
    }

    [Fact]
    public void Resume_ContinuesFromSameValue()
    {
        var (control, time) = CreateControl();
        control.Start();
        time.Advance(TimeSpan.FromSeconds(10));
        control.Pause();
        time.Advance(TimeSpan.FromMinutes(1));

        control.Resume();
        time.Advance(TimeSpan.FromSeconds(5));

        control.IsRunning.Should().BeTrue();
        control.GetElapsed().ToClockString().Should().Be("00:15");
    }

    [Fact]
    public void Stop_FromPaused_ExcludesPauseTime()
    {
        var (control, time) = CreateControl();
        control.Start();
        time.Advance(TimeSpan.FromSeconds(10));
        control.Pause();
        time.Advance(TimeSpan.FromMinutes(1));

        control.Stop();
        time.Advance(TimeSpan.FromMinutes(5));

        control.IsFinished.Should().BeTrue();
        control.GetElapsed().ToClockString().Should().Be("00:10");
    }

    [Fact]
    public void Pause_WithoutStart_Throws()
    {
        var (control, _) = CreateControl();

        var act = () => control.Pause();

        act.Should().Throw<InvalidTimerStateException>();
    }

    [Fact]
    public void Stop_WithoutStart_Throws()
    {
        var (control, _) = CreateControl();

        var act = () => control.Stop();

        act.Should().Throw<InvalidTimerStateException>();
    }
}
