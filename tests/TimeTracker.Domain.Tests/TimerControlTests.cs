using FluentAssertions;
using TimeTracker.Application;
using TimeTracker.Domain;
using Xunit;

namespace TimeTracker.Domain.Tests;

/// <summary>
/// Тесты сценария управления: входящий порт поверх доменной сессии.
/// </summary>
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
    public void Stop_FreezesElapsed()
    {
        var (control, time) = CreateControl();
        control.Start();
        time.Advance(TimeSpan.FromSeconds(10));

        control.Stop();
        time.Advance(TimeSpan.FromMinutes(5));

        control.IsRunning.Should().BeFalse();
        control.GetElapsed().ToClockString().Should().Be("00:10");
    }

    [Fact]
    public void Start_AfterStop_CountsFromNewMoment()
    {
        var (control, time) = CreateControl();
        control.Start();
        time.Advance(TimeSpan.FromSeconds(10));
        control.Stop();

        time.Advance(TimeSpan.FromMinutes(1));
        control.Start();
        time.Advance(TimeSpan.FromSeconds(5));

        control.GetElapsed().ToClockString().Should().Be("00:05");
    }

    [Fact]
    public void Stop_WithoutStart_Throws()
    {
        var (control, _) = CreateControl();

        var act = () => control.Stop();

        act.Should().Throw<InvalidOperationException>();
    }
}
