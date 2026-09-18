using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using TimeTracker.Application.Abstractions.Persistence;
using TimeTracker.Application.TimeTracking;
using TimeTracker.Domain.TimeTracking;
using Xunit;

namespace TimeTracker.Application.Tests;

public class TimerServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 15, 10, 0, 0, TimeSpan.Zero);

    private sealed class TestScope : IDisposable
    {
        public ITimeEntryRepository Repository { get; } = Substitute.For<ITimeEntryRepository>();
        public IUnitOfWork UnitOfWork { get; } = Substitute.For<IUnitOfWork>();
        public IServiceScope Scope
        {
            get;
        }

        public TestScope()
        {
            var services = new ServiceCollection();
            services.AddSingleton(Repository);
            services.AddSingleton(UnitOfWork);
            var provider = services.BuildServiceProvider();
            Scope = provider.CreateScope();
        }

        public void Dispose() => Scope.Dispose();
    }

    private static (TimerService Service, FakeTimeProvider TimeProvider, ITimeEntryRepository Repository, IUnitOfWork UnitOfWork) CreateService(DateTimeOffset? startAt = null)
    {
        var timeProvider = new FakeTimeProvider(startAt ?? Now);
        var repository = Substitute.For<ITimeEntryRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        var service = new TimerService(
            timeProvider,
            repository,
            unitOfWork,
            NullLogger<TimerService>.Instance);

        return (service, timeProvider, repository, unitOfWork);
    }

    [Fact]
    public async Task StartTimerAsync_FromIdle_TransitionsToRunning()
    {
        // Arrange
        var (service, _, repository, unitOfWork) = CreateService();

        TimerState? capturedNewState = null;
        service.StateChanged += (_, e) => capturedNewState = e.NewState;

        // Act
        await service.StartTimerAsync("Работа", null);

        // Assert
        service.State.Should().Be(TimerState.Running);
        capturedNewState.Should().Be(TimerState.Running);
        await repository.Received(1).AddAsync(Arg.Any<TimeEntry>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartTimerAsync_FromRunning_Throws()
    {
        // Arrange
        var (service, _, _, _) = CreateService();
        await service.StartTimerAsync("первый", null);

        // Act
        var act = async () => await service.StartTimerAsync("второй", null);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task CurrentDuration_AfterTenMinutes_IsTenMinutes()
    {
        // Arrange
        var (service, timeProvider, _, _) = CreateService();
        await service.StartTimerAsync("Работа", null);

        // Act
        timeProvider.Advance(TimeSpan.FromMinutes(10));

        // Assert
        service.CurrentDuration.Value.Should().Be(TimeSpan.FromMinutes(10));
    }

    [Fact]
    public async Task CurrentDuration_DoesNotCountPausedTime()
    {
        // Arrange
        var (service, timeProvider, _, _) = CreateService();
        await service.StartTimerAsync("Работа", null);
        timeProvider.Advance(TimeSpan.FromMinutes(5));

        // Act
        service.PauseTimer();
        timeProvider.Advance(TimeSpan.FromHours(1));
        service.ResumeTimer();
        timeProvider.Advance(TimeSpan.FromMinutes(5));

        // Assert — 5 + 5 = 10 минут, час паузы не в счёт
        service.CurrentDuration.Value.Should().Be(TimeSpan.FromMinutes(10));
    }

    [Fact]
    public async Task StopTimerAsync_FromRunning_SavesAndGoesToIdle()
    {
        // Arrange
        var (service, timeProvider, _, unitOfWork) = CreateService();
        await service.StartTimerAsync("Работа", null);
        timeProvider.Advance(TimeSpan.FromMinutes(30));

        // Act
        await service.StopTimerAsync();

        // Assert
        service.State.Should().Be(TimerState.Idle);
        service.CurrentEntry.Should().BeNull();
        // SaveChanges вызывался дважды: при старте и при стопе.
        await unitOfWork.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StopTimerAsync_FromPaused_ClosesPauseThenStops()
    {
        // Arrange
        var (service, timeProvider, _, unitOfWork) = CreateService();
        await service.StartTimerAsync("Работа", null);
        timeProvider.Advance(TimeSpan.FromMinutes(5));
        service.PauseTimer();
        timeProvider.Advance(TimeSpan.FromMinutes(3));

        // Act
        await service.StopTimerAsync();

        // Assert
        service.State.Should().Be(TimerState.Idle);
        await unitOfWork.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RestoreTimerAsync_WithRunningEntry_TransitionsToRunning()
    {
        // Arrange
        var (service, _, repository, _) = CreateService();

        var runningEntry = TimeEntry.StartNew("восстановленная", startedAt: Now.AddMinutes(-15));
        repository.GetRunningAsync(Arg.Any<CancellationToken>()).Returns(runningEntry);

        // Act
        await service.RestoreTimerAsync();

        // Assert
        service.State.Should().Be(TimerState.Running);
        service.CurrentEntry.Should().BeSameAs(runningEntry);
        service.CurrentDuration.Value.Should().Be(TimeSpan.FromMinutes(15));
    }

    [Fact]
    public async Task RestoreTimerAsync_WithoutRunningEntry_StaysIdle()
    {
        // Arrange
        var (service, _, repository, _) = CreateService();
        repository.GetRunningAsync(Arg.Any<CancellationToken>()).Returns((TimeEntry?)null);

        // Act
        await service.RestoreTimerAsync();

        // Assert
        service.State.Should().Be(TimerState.Idle);
        service.CurrentEntry.Should().BeNull();
    }

    [Fact]
    public void PauseTimer_FromIdle_Throws()
    {
        // Arrange
        var (service, _, _, _) = CreateService();

        // Act
        var act = () => service.PauseTimer();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public async Task ResumeTimer_FromRunning_Throws()
    {
        // Arrange
        var (service, _, _, _) = CreateService();
        await service.StartTimerAsync("Работа", null);

        // Act
        var act = () => service.ResumeTimer();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public async Task Tick_FiresWhileRunning_AndStopsAfterPause()
    {
        // Arrange
        var (service, timeProvider, _, _) = CreateService();
        var tickCount = 0;
        service.Tick += (_, _) => tickCount++;

        // Act
        await service.StartTimerAsync("Работа", null);

        // FakeTimeProvider.Advance не эмулирует "N секунд прошло → N тиков".
        // Вызываем Advance по одной секунде — каждый вызов пересекает
        // одну границу таймера и запускает callback ровно один раз.
        for (var i = 0; i < 5; i++)
            timeProvider.Advance(TimeSpan.FromSeconds(1));

        var ticksWhileRunning = tickCount;

        service.PauseTimer();

        for (var i = 0; i < 5; i++)
            timeProvider.Advance(TimeSpan.FromSeconds(1));

        var ticksAfterPause = tickCount;

        // Assert
        ticksWhileRunning.Should().Be(5);
        ticksAfterPause.Should().Be(ticksWhileRunning);
    }

    [Fact]
    public async Task StateChanged_FiresOnEveryTransition()
    {
        // Arrange
        var (service, _, _, _) = CreateService();
        var transitions = new List<(TimerState Old, TimerState New)>();
        service.StateChanged += (_, e) => transitions.Add((e.OldState, e.NewState));

        // Act
        await service.StartTimerAsync("Работа", null);   // Idle → Running
        service.PauseTimer();                          // Running → Paused
        service.ResumeTimer();                         // Paused → Running
        await service.StopTimerAsync();                // Running → Idle

        // Assert
        transitions.Should().HaveCount(4);
        transitions[0].Should().Be((TimerState.Idle, TimerState.Running));
        transitions[1].Should().Be((TimerState.Running, TimerState.Paused));
        transitions[2].Should().Be((TimerState.Paused, TimerState.Running));
        transitions[3].Should().Be((TimerState.Running, TimerState.Idle));
    }
}
