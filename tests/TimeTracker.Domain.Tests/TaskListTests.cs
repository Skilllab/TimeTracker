using FluentAssertions;
using TimeTracker.Application;
using TimeTracker.Domain;
using Xunit;

namespace TimeTracker.Domain.Tests;

public sealed class TaskListTests
{
    private static readonly DateTimeOffset Start = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task GetAsync_OrdersRunningThenRecentlyStartedAndFinishedLast()
    {
        var running = WorkTask.Create(Guid.NewGuid(), "Идущая", null, Start).Start(Start.AddMinutes(1));
        var paused = WorkTask.Create(Guid.NewGuid(), "Приостановленная", null, Start)
            .Start(Start.AddMinutes(30))
            .Pause(Start.AddMinutes(40));
        var dormant = WorkTask.Create(Guid.NewGuid(), "Неначатая", null, Start.AddMinutes(5));
        var finished = WorkTask.Create(Guid.NewGuid(), "Завершенная", null, Start)
            .Start(Start.AddMinutes(2))
            .Finish(Start.AddMinutes(3));
        var list = CreateList(new[] { dormant, finished, paused, running });

        var items = await list.GetAsync(null, null, null, TestContext.Current.CancellationToken);

        items.Select(item => item.Description)
            .Should().ContainInOrder("Идущая", "Приостановленная", "Неначатая", "Завершенная");
    }

    [Fact]
    public async Task GetAsync_TwoFinishedTasks_KeepsRecentlyStartedAbove()
    {
        var older = WorkTask.Create(Guid.NewGuid(), "Старая", null, Start)
            .Start(Start.AddMinutes(1))
            .Finish(Start.AddMinutes(2));
        var newer = WorkTask.Create(Guid.NewGuid(), "Новая", null, Start)
            .Start(Start.AddMinutes(30))
            .Finish(Start.AddMinutes(40));
        var list = CreateList(new[] { older, newer });

        var items = await list.GetAsync(null, null, null, TestContext.Current.CancellationToken);

        items.Select(item => item.Description).Should().ContainInOrder("Новая", "Старая");
    }

    [Fact]
    public async Task GetAsync_WithSearch_FiltersByNameOnly()
    {
        var running = WorkTask.Create(Guid.NewGuid(), "Идущая работа", null, Start).Start(Start.AddMinutes(1));
        var other = WorkTask.Create(Guid.NewGuid(), "Отчет за квартал", null, Start);
        var tasks = new FakeTaskRepository(new[] { running, other });

        var control = new TaskList(tasks, new FakeProjectRepository(), new FakeTimeProvider(Start.AddHours(1)));
        var items = await control.GetAsync("  отчет  ", null, null, TestContext.Current.CancellationToken);

        items.Should().ContainSingle().Which.Description.Should().Be("Отчет за квартал");
        running.Status.Should().Be(TaskStatus.Running);
        other.Status.Should().Be(TaskStatus.NotStarted);
    }

    [Fact]
    public async Task GetAsync_WithTagSearch_FiltersByTagName()
    {
        var matching = WorkTask.Create(Guid.NewGuid(), "Первая", null, Start).AddTag("Отчеты");
        var other = WorkTask.Create(Guid.NewGuid(), "Вторая", null, Start).AddTag("Анализ");
        var withoutTags = WorkTask.Create(Guid.NewGuid(), "Третья", null, Start);

        var items = await new TaskList(
                new FakeTaskRepository(new[] { matching, other, withoutTags }),
                new FakeProjectRepository(),
                new FakeTimeProvider(Start))
            .GetAsync(null, "отчет", null, TestContext.Current.CancellationToken);

        items.Should().ContainSingle().Which.Description.Should().Be("Первая");
    }

    [Fact]
    public async Task GetAsync_WithEmptySearch_ReturnsAllTasks()
    {
        var tasks = new FakeTaskRepository(new[]
        {
            WorkTask.Create(Guid.NewGuid(), "Первая", null, Start),
            WorkTask.Create(Guid.NewGuid(), "Вторая", null, Start.AddMinutes(1))
        });
        var list = CreateList(tasks);

        var items = await list.GetAsync("   ", null, null, TestContext.Current.CancellationToken);

        items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAsync_WithProject_ReturnsOnlyItsTasks()
    {
        var project = new Project(Guid.NewGuid(), "Внутренние работы", ProjectPalette.Colors[0]);
        var other = new Project(Guid.NewGuid(), "Учебный курс", ProjectPalette.Colors[0]);
        var mine = WorkTask.Create(Guid.NewGuid(), "Моя задача", project.Id, Start);
        var alien = WorkTask.Create(Guid.NewGuid(), "Чужая задача", other.Id, Start);
        var withoutProject = WorkTask.Create(Guid.NewGuid(), "Без проекта", null, Start);

        var items = await new TaskList(
                new FakeTaskRepository(new[] { mine, alien, withoutProject }),
                new FakeProjectRepository(project, other),
                new FakeTimeProvider(Start))
            .GetAsync(null, null, project.Id, TestContext.Current.CancellationToken);

        items.Should().ContainSingle().Which.Description.Should().Be("Моя задача");
    }

    [Fact]
    public async Task GetAsync_ComputesElapsedWithoutPauseTime()
    {
        var task = WorkTask.Create(Guid.NewGuid(), "Работа", null, Start)
            .Start(Start)
            .Pause(Start.AddMinutes(10))
            .Resume(Start.AddMinutes(40));
        var time = new FakeTimeProvider(Start.AddMinutes(50));

        var items = await new TaskList(new FakeTaskRepository(new[] { task }), new FakeProjectRepository(), time)
            .GetAsync(null, null, null, TestContext.Current.CancellationToken);

        items.Should().ContainSingle().Which.Elapsed.Should().Be(Duration.From(TimeSpan.FromMinutes(20)));
    }

    [Fact]
    public async Task GetAsync_WithProject_FillsNameAndColor()
    {
        var project = new Project(Guid.NewGuid(), "Внутренние работы", ProjectPalette.Colors[0]);
        var task = WorkTask.Create(Guid.NewGuid(), "Работа", project.Id, Start);

        var items = await new TaskList(
                new FakeTaskRepository(new[] { task }),
                new FakeProjectRepository(project),
                new FakeTimeProvider(Start))
            .GetAsync(null, null, null, TestContext.Current.CancellationToken);

        var item = items.Should().ContainSingle().Which;
        item.ProjectId.Should().Be(project.Id);
        item.ProjectName.Should().Be("Внутренние работы");
        item.ProjectColor.Should().Be(project.Color);
    }

    [Fact]
    public async Task GetAsync_WithoutProject_LeavesProjectFieldsEmpty()
    {
        var task = WorkTask.Create(Guid.NewGuid(), "Работа", null, Start);

        var items = await new TaskList(
                new FakeTaskRepository(new[] { task }),
                new FakeProjectRepository(),
                new FakeTimeProvider(Start))
            .GetAsync(null, null, null, TestContext.Current.CancellationToken);

        var item = items.Should().ContainSingle().Which;
        item.ProjectId.Should().BeNull();
        item.ProjectName.Should().BeEmpty();
        item.ProjectColor.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAsync_ReportsTagsAndCreationMoments()
    {
        var task = WorkTask.Create(Guid.NewGuid(), "Работа", null, Start).AddTag("Отчеты").Start(Start.AddMinutes(5));

        var items = await new TaskList(
                new FakeTaskRepository(new[] { task }),
                new FakeProjectRepository(),
                new FakeTimeProvider(Start.AddMinutes(30)))
            .GetAsync(null, null, null, TestContext.Current.CancellationToken);

        var item = items.Should().ContainSingle().Which;
        item.Tags.Should().Be("Отчеты");
        item.TagNames.Should().ContainSingle().Which.Should().Be("Отчеты");
        item.CreatedAt.Should().Be(Start);
        item.StartedAt.Should().Be(Start.AddMinutes(5));
        item.LastStartedAt.Should().Be(Start.AddMinutes(5));
        item.IsRunning.Should().BeTrue();
        item.CanPause.Should().BeTrue();
        item.CanFinish.Should().BeTrue();
        item.CanStart.Should().BeFalse();
        item.CanReopen.Should().BeFalse();
    }

    [Fact]
    public async Task GetAsync_ReportsLifecycleLabels()
    {
        var started = WorkTask.Create(Guid.NewGuid(), "Начатая", null, Start).Start(Start.AddMinutes(1));
        var finished = WorkTask.Create(Guid.NewGuid(), "Завершенная", null, Start)
            .Start(Start.AddMinutes(1))
            .Finish(Start.AddMinutes(2));
        var dormant = WorkTask.Create(Guid.NewGuid(), "Неначатая", null, Start);

        var items = await new TaskList(
                new FakeTaskRepository(new[] { started, finished, dormant }),
                new FakeProjectRepository(),
                new FakeTimeProvider(Start.AddHours(1)))
            .GetAsync(null, null, null, TestContext.Current.CancellationToken);

        items.Single(item => item.Description == "Неначатая").Labels.Should().ContainSingle()
            .Which.Should().Be(TaskLifecycle.Created);

        items.Single(item => item.Description == "Начатая").Labels.Should().ContainInOrder(
            TaskLifecycle.Created,
            TaskLifecycle.Started);

        items.Single(item => item.Description == "Завершенная").Labels.Should().ContainInOrder(
            TaskLifecycle.Created,
            TaskLifecycle.Started,
            TaskLifecycle.Finished);
    }

    [Fact]
    public async Task GetAsync_FinishedTask_OffersReopenOnly()
    {
        var task = WorkTask.Create(Guid.NewGuid(), "Работа", null, Start)
            .Start(Start)
            .Finish(Start.AddMinutes(10));

        var items = await new TaskList(
                new FakeTaskRepository(new[] { task }),
                new FakeProjectRepository(),
                new FakeTimeProvider(Start.AddHours(1)))
            .GetAsync(null, null, null, TestContext.Current.CancellationToken);

        var item = items.Should().ContainSingle().Which;
        item.IsFinished.Should().BeTrue();
        item.CanReopen.Should().BeTrue();
        item.CanStart.Should().BeFalse();
        item.CanFinish.Should().BeFalse();
    }

    [Fact]
    public async Task GetAsync_WithoutIncludeDeleted_HidesDeletedTasks()
    {
        var deleted = WorkTask.Create(Guid.NewGuid(), "Удаленная", null, Start).Delete();
        var active = WorkTask.Create(Guid.NewGuid(), "Действующая", null, Start);
        var list = CreateList(new[] { deleted, active });

        var items = await list.GetAsync(null, null, null, TestContext.Current.CancellationToken);

        items.Should().ContainSingle();
        items[0].Description.Should().Be("Действующая");
    }

    [Fact]
    public async Task GetAsync_WithIncludeDeleted_PlacesDeletedLast()
    {
        var finished = WorkTask.Create(Guid.NewGuid(), "Завершенная", null, Start)
            .Start(Start.AddMinutes(1))
            .Finish(Start.AddMinutes(2));
        var deleted = WorkTask.Create(Guid.NewGuid(), "Удаленная", null, Start).Delete();
        var list = CreateList(new[] { deleted, finished });

        var items = await list.GetAsync(null, null, null, TestContext.Current.CancellationToken, includeDeleted: true);

        items.Select(item => item.Description)
            .Should().ContainInOrder("Завершенная", "Удаленная");
        items[1].IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task GetAsync_WithIncludeDeletedAndNameFilter_ShowsDeletedMatch()
    {
        var deleted = WorkTask.Create(Guid.NewGuid(), "Удаленная работа", null, Start).Delete();
        var active = WorkTask.Create(Guid.NewGuid(), "Другая работа", null, Start);
        var list = CreateList(new[] { deleted, active });

        var items = await list.GetAsync("Удаленная", null, null, TestContext.Current.CancellationToken, includeDeleted: true);

        items.Should().ContainSingle();
        items[0].IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task GetAsync_WithTagFilterAndIncludeDeleted_PlacesDeletedLast()
    {
        var active = WorkTask.Create(Guid.NewGuid(), "Действующая", null, Start).AddTag("Отчеты");
        var deleted = WorkTask.Create(Guid.NewGuid(), "Удаленная", null, Start).AddTag("Отчеты").Delete();
        var list = CreateList(new[] { deleted, active });

        var items = await list.GetAsync(null, "Отчеты", null, TestContext.Current.CancellationToken, includeDeleted: true);

        items.Select(item => item.Description)
            .Should().ContainInOrder("Действующая", "Удаленная");
    }

    private static TaskList CreateList(IEnumerable<WorkTask> tasks)
        => CreateList(new FakeTaskRepository(tasks));

    private static TaskList CreateList(FakeTaskRepository tasks)
        => new(tasks, new FakeProjectRepository(), new FakeTimeProvider(Start.AddHours(1)));

    private sealed class FakeTaskRepository : IWorkTaskRepository
    {
        private readonly List<WorkTask> _tasks;

        public FakeTaskRepository(IEnumerable<WorkTask> tasks)
        {
            _tasks = tasks.ToList();
        }

        public Task AddAsync(WorkTask task, CancellationToken cancellationToken = default)
        {
            _tasks.Add(task);

            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<WorkTask>> GetAllAsync(bool includeDeleted = false, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<WorkTask>>(
                _tasks.Where(task => includeDeleted || !task.IsDeleted).ToList());

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

        public Task DeletePermanentlyAsync(WorkTask task, CancellationToken cancellationToken = default)
        {
            _tasks.RemoveAll(existing => existing.Id == task.Id);

            return Task.CompletedTask;
        }
    }

    private sealed class FakeProjectRepository : IProjectRepository
    {
        private readonly List<Project> _projects;

        public FakeProjectRepository(params Project[] projects)
        {
            _projects = projects.ToList();
        }

        public Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Project>>(_projects.ToList());

        public Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_projects.FirstOrDefault(project => project.Id == id));

        public Task AddAsync(Project project, CancellationToken cancellationToken = default)
        {
            _projects.Add(project);

            return Task.CompletedTask;
        }

        public Task UpdateAsync(Project project, CancellationToken cancellationToken = default)
        {
            var index = _projects.FindIndex(existing => existing.Id == project.Id);

            if (index >= 0)
            {
                _projects[index] = project;
            }

            return Task.CompletedTask;
        }
    }
}
