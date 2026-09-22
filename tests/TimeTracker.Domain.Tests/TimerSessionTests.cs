using FluentAssertions;
using Xunit;

namespace TimeTracker.Domain.Tests;


public sealed class TimerSessionTests
{
    private static readonly DateTimeOffset Start = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void NewSession_IsNotRunning()
    {
        var session = new TimerSession();

        session.IsRunning.Should().BeFalse();
        session.Current.Should().BeNull();
    }

    [Fact]
    public void Start_OpensCurrentRange()
    {
        var session = new TimerSession();

        session.Start(Start);

        session.IsRunning.Should().BeTrue();
        session.Current!.Start.Should().Be(Start);
        session.Current.IsOpen.Should().BeTrue();
    }

    [Fact]
    public void Stop_ClosesCurrentRange()
    {
        var session = new TimerSession();
        session.Start(Start);

        session.Stop(Start.AddSeconds(30));

        session.IsRunning.Should().BeFalse();
        session.Current!.End.Should().Be(Start.AddSeconds(30));
    }

    [Fact]
    public void Stop_WithoutStart_Throws()
    {
        var session = new TimerSession();

        var act = () => session.Stop(Start);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Нельзя остановить незапущенную запись.");
    }

    [Fact]
    public void Stop_Twice_Throws()
    {
        var session = new TimerSession();
        session.Start(Start);
        session.Stop(Start.AddSeconds(10));

        var act = () => session.Stop(Start.AddSeconds(20));

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Start_AfterStop_StartsNewRange()
    {
        var session = new TimerSession();
        session.Start(Start);
        session.Stop(Start.AddSeconds(10));

        var restart = Start.AddMinutes(1);
        session.Start(restart);

        session.IsRunning.Should().BeTrue();
        session.Current!.Start.Should().Be(restart);
        session.Current.IsOpen.Should().BeTrue();
    }
}
