using FluentAssertions;
using TimeTracker.Application;
using Xunit;

namespace TimeTracker.Domain.Tests;

public sealed class IdleWatcherTests
{
    [Fact]
    public void Check_IdleAboveThreshold_PausesRunningTask()
    {
        var control = new FakeTaskControl();
        var detector = new FakeIdleDetector { IdleTime = TimeSpan.FromMinutes(6) };
        var watcher = Create(detector, control);

        watcher.Check();

        control.PauseRunningCount.Should().Be(1);
    }

    [Fact]
    public void Check_IdleBelowThreshold_KeepsTaskRunning()
    {
        var control = new FakeTaskControl();
        var detector = new FakeIdleDetector { IdleTime = TimeSpan.FromMinutes(1) };
        var watcher = Create(detector, control);

        watcher.Check();

        control.PauseRunningCount.Should().Be(0);
    }

    [Fact]
    public void Check_CustomThreshold_RespectsSetting()
    {
        var control = new FakeTaskControl();
        var detector = new FakeIdleDetector { IdleTime = TimeSpan.FromMinutes(3) };
        var settings = new IdleSettings();
        settings.SetThreshold(TimeSpan.FromMinutes(2));
        var watcher = new IdleWatcher(detector, control, settings, TimeProvider.System, TimeSpan.FromSeconds(30));

        watcher.Check();

        control.PauseRunningCount.Should().Be(1);
    }

    private static IdleWatcher Create(FakeIdleDetector detector, FakeTaskControl control)
    {
        return new IdleWatcher(detector, control, new IdleSettings(), TimeProvider.System, TimeSpan.FromSeconds(30));
    }

    private sealed class FakeIdleDetector : IIdleDetector
    {
        public TimeSpan IdleTime { get; set; }

        public TimeSpan GetIdleTime() => IdleTime;
    }

    private sealed class FakeTaskControl : ITaskControl
    {
        public int PauseRunningCount { get; private set; }

        public Task<Guid> CreateTaskAsync(string name, Guid? projectId, CancellationToken cancellationToken = default)
            => Task.FromResult(Guid.NewGuid());

        public Task StartTaskAsync(Guid taskId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task PauseAsync(Guid taskId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task ResumeAsync(Guid taskId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task FinishAsync(Guid taskId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task ReopenAsync(Guid taskId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task RenameAsync(Guid taskId, string name, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task ChangeProjectAsync(Guid taskId, Guid? projectId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task AddTagAsync(Guid taskId, string tag, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task RemoveTagAsync(Guid taskId, string tag, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task RestoreAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task PauseRunningAsync(CancellationToken cancellationToken = default)
        {
            PauseRunningCount++;

            return Task.CompletedTask;
        }
    }
}
