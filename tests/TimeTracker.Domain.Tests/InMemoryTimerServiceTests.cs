using FluentAssertions;
using TimeTracker.Application;
using TimeTracker.Infrastructure;
using Xunit;

namespace TimeTracker.Domain.Tests;

/// <summary>
/// Тесты заглушки таймера: порт реализован в Infrastructure, расчет времени идет через
/// <see cref="TimeProvider"/>, а тест работает только через порт <see cref="ITimerService"/>.
/// </summary>
public sealed class InMemoryTimerServiceTests
{
    private static readonly DateTimeOffset Start = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);


    [Fact]
    public void GetElapsed_ImmediatelyAfterCreation_ReturnsZero()
    {
        var time = new FakeTimeProvider(Start);
        ITimerService service = new InMemoryTimerService(time);

        service.GetElapsed().Should().Be(Duration.Zero);
    }


    [Fact]
    public void GetElapsed_AfterTimeAdvance_Increases()
    {
        var time = new FakeTimeProvider(Start);
        ITimerService service = new InMemoryTimerService(time);

        time.Advance(TimeSpan.FromSeconds(65));

        service.GetElapsed().Should().Be(Duration.From(TimeSpan.FromSeconds(65)));
        service.GetElapsed().ToClockString().Should().Be("01:05");
    }


    [Fact]
    public void Constructor_WithoutTimeProvider_Throws()
    {
        var act = () => new InMemoryTimerService(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
