using FluentAssertions;
using TimeTracker.Application;
using TimeTracker.Domain;
using Xunit;

namespace TimeTracker.Domain.Tests;

public sealed class TaskControlTests
{
    private static readonly DateTimeOffset Start = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    private static (TaskControl Control, FakeTimeProvider Time, FakeTaskRepository Tasks, FakeEntryRepository Entries, FakeUnitOfWork UnitOfWork) CreateControl()
    {
        var time = new FakeTimeProvider(Start);
        var tasks = new FakeTaskRepository();
        var entries = new FakeEntryRepository();
        var unitOfWork = new FakeUnitOfWork();
        var control = new TaskControl(tasks, entries, unitOfWork, time);

        return (control, time, tasks, entries, unitOfWork);
    }

    [Fact]
    public async Task CreateTaskAsync_WithName_AddsNotStartedTask()
    {
        var (control, _, tasks, _, unitOfWork) = CreateControl();

        var taskId = await control.CreateTaskAsync("  Работа над отчетом  ", null, TestContext.Current.CancellationToken);

        var task = tasks.Tasks.Should().ContainSingle().Which;
        task.Id.Should().Be(taskId);
        task.Description.Should().Be("Работа над отчетом");
        task.Status.Should().Be(TaskStatus.NotStarted);
        task.CreatedAt.Should().Be(Start);
        unitOfWork.SaveCount.Should().Be(1);
    }

    [Fact]
    public async Task CreateTaskAsync_WithoutName_Throws()
    {
        var (control, _, tasks, _, unitOfWork) = CreateControl();

        var act = async () => await control.CreateTaskAsync("   ", null, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidTaskStateException>();
        tasks.Tasks.Should().BeEmpty();
        unitOfWork.SaveCount.Should().Be(0);
    }

    [Fact]
    public async Task StartTaskAsync_FromNotStarted_OpensSegment()
    {
        var (control, time, tasks, entries, _) = CreateControl();
        var taskId = await control.CreateTaskAsync("Работа", null, TestContext.Current.CancellationToken);

        time.Advance(TimeSpan.FromMinutes(1));
        await control.StartTaskAsync(taskId, TestContext.Current.CancellationToken);

        tasks.Tasks[0].Status.Should().Be(TaskStatus.Running);
        tasks.Tasks[0].StartedAt.Should().Be(Start.AddMinutes(1));

        var segment = entries.Added.Should().ContainSingle().Which;
        segment.TaskId.Should().Be(taskId);
        segment.Description.Should().Be("Работа");
        segment.IsOpen.Should().BeTrue();
    }

    [Fact]
    public async Task StartTaskAsync_WhileOtherTaskRuns_PausesIt()
    {
        var (control, time, tasks, entries, _) = CreateControl();
        var firstId = await control.CreateTaskAsync("Первая", null, TestContext.Current.CancellationToken);
        var secondId = await control.CreateTaskAsync("Вторая", null, TestContext.Current.CancellationToken);

        time.Advance(TimeSpan.FromMinutes(1));
        await control.StartTaskAsync(firstId, TestContext.Current.CancellationToken);

        time.Advance(TimeSpan.FromMinutes(10));
        await control.StartTaskAsync(secondId, TestContext.Current.CancellationToken);

        var first = tasks.Tasks.Single(task => task.Id == firstId);
        var second = tasks.Tasks.Single(task => task.Id == secondId);

        first.Status.Should().Be(TaskStatus.Paused);
        first.PausedAt.Should().Be(Start.AddMinutes(11));
        second.Status.Should().Be(TaskStatus.Running);

        entries.Updated.Should().ContainSingle();
        entries.Updated[0].TaskId.Should().Be(firstId);
        entries.Updated[0].IsOpen.Should().BeFalse();
        entries.Added.Should().HaveCount(2);
    }

    [Fact]
    public async Task ResumeAsync_WhileOtherTaskRuns_PausesIt()
    {
        var (control, time, tasks, _, _) = CreateControl();
        var firstId = await control.CreateTaskAsync("Первая", null, TestContext.Current.CancellationToken);
        var secondId = await control.CreateTaskAsync("Вторая", null, TestContext.Current.CancellationToken);

        time.Advance(TimeSpan.FromMinutes(1));
        await control.StartTaskAsync(secondId, TestContext.Current.CancellationToken);
        await control.PauseAsync(secondId, TestContext.Current.CancellationToken);

        time.Advance(TimeSpan.FromMinutes(5));
        await control.StartTaskAsync(firstId, TestContext.Current.CancellationToken);

        time.Advance(TimeSpan.FromMinutes(5));
        await control.ResumeAsync(secondId, TestContext.Current.CancellationToken);

        var first = tasks.Tasks.Single(task => task.Id == firstId);
        var second = tasks.Tasks.Single(task => task.Id == secondId);

        first.Status.Should().Be(TaskStatus.Paused);
        second.Status.Should().Be(TaskStatus.Running);
        second.LastStartedAt.Should().Be(Start.AddMinutes(11));
    }

    [Fact]
    public async Task PauseAsync_ClosesOpenSegment()
    {
        var (control, time, tasks, entries, _) = CreateControl();
        var taskId = await control.CreateTaskAsync("Работа", null, TestContext.Current.CancellationToken);
        await control.StartTaskAsync(taskId, TestContext.Current.CancellationToken);

        time.Advance(TimeSpan.FromMinutes(10));
        await control.PauseAsync(taskId, TestContext.Current.CancellationToken);

        tasks.Tasks[0].Status.Should().Be(TaskStatus.Paused);
        entries.Updated.Should().ContainSingle();
        entries.Updated[0].TaskId.Should().Be(taskId);
        entries.Updated[0].EndedAt.Should().Be(Start.AddMinutes(10));
    }

    [Fact]
    public async Task FinishAsync_ClosesSegmentAndFinishesTask()
    {
        var (control, time, tasks, entries, _) = CreateControl();
        var taskId = await control.CreateTaskAsync("Работа", null, TestContext.Current.CancellationToken);
        await control.StartTaskAsync(taskId, TestContext.Current.CancellationToken);

        time.Advance(TimeSpan.FromMinutes(20));
        await control.FinishAsync(taskId, TestContext.Current.CancellationToken);

        tasks.Tasks[0].Status.Should().Be(TaskStatus.Finished);
        tasks.Tasks[0].FinishedAt.Should().Be(Start.AddMinutes(20));
        tasks.Tasks[0].ElapsedAt(Start.AddHours(5)).Should().Be(Duration.From(TimeSpan.FromMinutes(20)));
        entries.Updated.Should().ContainSingle();
    }

    [Fact]
    public async Task ReopenAsync_FromFinished_ReturnsToPaused()
    {
        var (control, time, tasks, _, _) = CreateControl();
        var taskId = await control.CreateTaskAsync("Работа", null, TestContext.Current.CancellationToken);
        await control.StartTaskAsync(taskId, TestContext.Current.CancellationToken);

        time.Advance(TimeSpan.FromMinutes(10));
        await control.FinishAsync(taskId, TestContext.Current.CancellationToken);

        time.Advance(TimeSpan.FromHours(1));
        await control.ReopenAsync(taskId, TestContext.Current.CancellationToken);

        tasks.Tasks[0].Status.Should().Be(TaskStatus.Paused);
        tasks.Tasks[0].FinishedAt.Should().BeNull();
        tasks.Tasks[0].ElapsedAt(Start.AddHours(2)).Should().Be(Duration.From(TimeSpan.FromMinutes(10)));
    }

    [Fact]
    public async Task RenameAsync_ChangesNameAndKeepsState()
    {
        var (control, _, tasks, _, _) = CreateControl();
        var taskId = await control.CreateTaskAsync("Работа", null, TestContext.Current.CancellationToken);
        await control.StartTaskAsync(taskId, TestContext.Current.CancellationToken);

        await control.RenameAsync(taskId, "  Другая работа  ", TestContext.Current.CancellationToken);

        tasks.Tasks[0].Description.Should().Be("Другая работа");
        tasks.Tasks[0].Status.Should().Be(TaskStatus.Running);
    }

    [Fact]
    public async Task RenameAsync_WithoutName_Throws()
    {
        var (control, _, tasks, _, _) = CreateControl();
        var taskId = await control.CreateTaskAsync("Работа", null, TestContext.Current.CancellationToken);

        var act = async () => await control.RenameAsync(taskId, "   ", TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidTaskStateException>();
        tasks.Tasks[0].Description.Should().Be("Работа");
    }

    [Fact]
    public async Task ChangeProjectAsync_WithProject_SavesNewProject()
    {
        var (control, _, tasks, _, unitOfWork) = CreateControl();
        var taskId = await control.CreateTaskAsync("Работа", null, TestContext.Current.CancellationToken);
        var projectId = Guid.NewGuid();

        await control.ChangeProjectAsync(taskId, projectId, TestContext.Current.CancellationToken);

        tasks.Tasks[0].ProjectId.Should().Be(projectId);
        unitOfWork.SaveCount.Should().Be(2);
    }

    [Fact]
    public async Task ChangeProjectAsync_WithoutProject_ClearsLink()
    {
        var (control, _, tasks, _, _) = CreateControl();
        var projectId = Guid.NewGuid();
        var taskId = await control.CreateTaskAsync("Работа", projectId, TestContext.Current.CancellationToken);

        await control.ChangeProjectAsync(taskId, null, TestContext.Current.CancellationToken);

        tasks.Tasks[0].ProjectId.Should().BeNull();
    }

    [Fact]
    public async Task ChangeProjectAsync_KeepsNameAndState()
    {
        var (control, _, tasks, _, _) = CreateControl();
        var taskId = await control.CreateTaskAsync("Работа", null, TestContext.Current.CancellationToken);
        await control.StartTaskAsync(taskId, TestContext.Current.CancellationToken);

        await control.ChangeProjectAsync(taskId, Guid.NewGuid(), TestContext.Current.CancellationToken);

        tasks.Tasks[0].Description.Should().Be("Работа");
        tasks.Tasks[0].Status.Should().Be(TaskStatus.Running);
    }

    [Fact]
    public async Task ChangeProjectAsync_UnknownTask_Throws()
    {
        var (control, _, _, _, _) = CreateControl();

        var act = async () => await control.ChangeProjectAsync(
            Guid.NewGuid(),
            null,
            TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidTaskStateException>();
    }

    [Fact]
    public async Task StartTaskAsync_UnknownTask_Throws()
    {
        var (control, _, _, _, _) = CreateControl();

        var act = async () => await control.StartTaskAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidTaskStateException>()
            .WithMessage("Задача не найдена.");
    }

    [Fact]
    public async Task RestoreAsync_AfterRestart_PausesTaskAtLastStartedMoment()
    {
        var (control, time, tasks, entries, _) = CreateControl();
        var taskId = await control.CreateTaskAsync("Работа", null, TestContext.Current.CancellationToken);
        await control.StartTaskAsync(taskId, TestContext.Current.CancellationToken);

        time.Advance(TimeSpan.FromHours(3));
        await control.RestoreAsync(TestContext.Current.CancellationToken);

        var task = tasks.Tasks[0];
        task.Status.Should().Be(TaskStatus.Paused);
        task.PausedAt.Should().Be(Start);
        task.ElapsedAt(Start.AddHours(3)).Should().Be(Duration.Zero);
        entries.Updated.Should().ContainSingle();
        entries.Updated[0].EndedAt.Should().Be(Start);
    }

    [Fact]
    public async Task RestoreAsync_WithoutRunningTask_DoesNothing()
    {
        var (control, _, _, entries, unitOfWork) = CreateControl();
        await control.CreateTaskAsync("Работа", null, TestContext.Current.CancellationToken);

        await control.RestoreAsync(TestContext.Current.CancellationToken);

        entries.Updated.Should().BeEmpty();
        unitOfWork.SaveCount.Should().Be(1);
    }

    private sealed class FakeTaskRepository : IWorkTaskRepository
    {
        private readonly List<WorkTask> _tasks = new();

        public IReadOnlyList<WorkTask> Tasks => _tasks;

        public Task AddAsync(WorkTask task, CancellationToken cancellationToken = default)
        {
            _tasks.Add(task);

            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<WorkTask>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<WorkTask>>(_tasks.ToList());

        public Task<WorkTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_tasks.FirstOrDefault(task => task.Id == id));

        public Task<WorkTask?> GetRunningAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(_tasks.FirstOrDefault(task => task.IsRunning));

        public Task UpdateAsync(WorkTask task, CancellationToken cancellationToken = default)
        {
            var index = _tasks.FindIndex(existing => existing.Id == task.Id);

            if (index >= 0)
            {
                _tasks[index] = task;
            }

            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<WorkTask>> GetRangeAsync(
            DateTimeOffset from,
            DateTimeOffset to,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<WorkTask>>(
                _tasks.Where(task => task.CreatedAt >= from && task.CreatedAt <= to).ToList());
    }

    private sealed class FakeEntryRepository : ITimeEntryRepository
    {
        private readonly List<TimeEntry> _entries = new();

        public List<TimeEntry> Added { get; } = new();

        public List<TimeEntry> Updated { get; } = new();

        public Task AddAsync(TimeEntry entry, CancellationToken cancellationToken = default)
        {
            _entries.Add(entry);
            Added.Add(entry);

            return Task.CompletedTask;
        }

        public Task<TimeEntry?> GetActiveAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(_entries.FirstOrDefault(entry => entry.IsOpen));

        public Task<TimeEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_entries.FirstOrDefault(entry => entry.Id == id));

        public Task<IReadOnlyList<TimeEntry>> GetRangeAsync(
            DateTimeOffset from,
            DateTimeOffset to,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<TimeEntry>>(
                _entries.Where(entry => entry.StartedAt >= from && entry.StartedAt <= to).ToList());

        public Task UpdateAsync(TimeEntry entry, CancellationToken cancellationToken = default)
        {
            var index = _entries.FindIndex(existing => existing.Id == entry.Id);

            if (index >= 0)
            {
                _entries[index] = entry;
            }

            Updated.Add(entry);

            return Task.CompletedTask;
        }
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveCount { get; private set; }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveCount++;

            return Task.CompletedTask;
        }
    }
}
