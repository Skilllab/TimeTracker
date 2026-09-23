using FluentAssertions;
using TimeTracker.Application;
using TimeTracker.Domain;
using Xunit;

namespace TimeTracker.Domain.Tests;

public sealed class IdleWatcherTests
{
    [Fact]
    public void Check_WhileRunningAndIdle_PausesRecord()
    {
        var control = new FakeTimerControl { IsRunning = true };
        var detector = new FakeIdleDetector { IdleTime = TimeSpan.FromMinutes(6) };
        var watcher = Create(detector, control);

        watcher.Check();

        control.PauseCount.Should().Be(1);
    }

    [Fact]
    public void Check_IdleBelowThreshold_KeepsRecordRunning()
    {
        var control = new FakeTimerControl { IsRunning = true };
        var detector = new FakeIdleDetector { IdleTime = TimeSpan.FromMinutes(1) };
        var watcher = Create(detector, control);

        watcher.Check();

        control.PauseCount.Should().Be(0);
    }

    [Fact]
    public void Check_IdleButNotRunning_DoesNotPause()
    {
        var control = new FakeTimerControl { IsRunning = false };
        var detector = new FakeIdleDetector { IdleTime = TimeSpan.FromHours(1) };
        var watcher = Create(detector, control);

        watcher.Check();

        control.PauseCount.Should().Be(0);
    }

    [Fact]
    public void Check_CustomThreshold_RespectsSetting()
    {
        var control = new FakeTimerControl { IsRunning = true };
        var detector = new FakeIdleDetector { IdleTime = TimeSpan.FromMinutes(3) };
        var settings = new IdleSettings();
        settings.SetThreshold(TimeSpan.FromMinutes(2));
        var watcher = new IdleWatcher(detector, control, settings, TimeProvider.System, TimeSpan.FromSeconds(30));

        watcher.Check();

        control.PauseCount.Should().Be(1);
    }

    private static IdleWatcher Create(FakeIdleDetector detector, FakeTimerControl control)
    {
        return new IdleWatcher(detector, control, new IdleSettings(), TimeProvider.System, TimeSpan.FromSeconds(30));
    }

    private sealed class FakeIdleDetector : IIdleDetector
    {
        public TimeSpan IdleTime { get; set; }

        public TimeSpan GetIdleTime() => IdleTime;
    }

    private sealed class FakeTimerControl : ITimerControl
    {
        public bool IsRunning { get; set; }

        public bool IsPaused { get; private set; }

        public bool IsFinished { get; private set; }

        public int PauseCount { get; private set; }

        public Task Start()
        {
            IsRunning = true;

            return Task.CompletedTask;
        }

        public Task Pause()
        {
            PauseCount++;
            IsPaused = true;
            IsRunning = false;

            return Task.CompletedTask;
        }

        public Task Resume()
        {
            IsPaused = false;
            IsRunning = true;

            return Task.CompletedTask;
        }

        public Task Stop()
        {
            IsRunning = false;
            IsFinished = true;

            return Task.CompletedTask;
        }

        public Task RestoreAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Duration GetElapsed() => Duration.Zero;
    }
}
