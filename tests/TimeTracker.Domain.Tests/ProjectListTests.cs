using FluentAssertions;
using TimeTracker.Application;
using TimeTracker.Domain;
using Xunit;

namespace TimeTracker.Domain.Tests;

public sealed class ProjectListTests
{
    private static readonly DateTimeOffset Start = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task GetAvailableAsync_OrdersByName()
    {
        var repository = new FakeProjectRepository(
            new Project(Guid.NewGuid(), "Ядро", "#2F6FED"),
            new Project(Guid.NewGuid(), "Администрирование", "#2E7D32"));
        var scenario = new ProjectList(repository, new FakeEntryRepository());

        var projects = await scenario.GetAvailableAsync(TestContext.Current.CancellationToken);

        projects.Should().HaveCount(2);
        projects[0].Name.Should().Be("Администрирование");
        projects[1].Name.Should().Be("Ядро");
    }

    [Fact]
    public async Task GetAvailableAsync_WithoutProjects_ReturnsEmptyList()
    {
        var scenario = new ProjectList(new FakeProjectRepository(), new FakeEntryRepository());

        var projects = await scenario.GetAvailableAsync(TestContext.Current.CancellationToken);

        projects.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithoutRepository_Throws()
    {
        var act = () => new ProjectList(null!, new FakeEntryRepository());

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task GetAvailableAsync_ExcludesArchived()
    {
        var repository = new FakeProjectRepository(
            new Project(Guid.NewGuid(), "Работа", "#2F6FED"),
            new Project(Guid.NewGuid(), "Закрытый", "#2E7D32", isArchived: true));
        var scenario = new ProjectList(repository, new FakeEntryRepository());

        var projects = await scenario.GetAvailableAsync(TestContext.Current.CancellationToken);

        projects.Should().HaveCount(1);
        projects[0].Name.Should().Be("Работа");
    }

    [Fact]
    public async Task GetAllAsync_WithArchived_ReturnsBoth()
    {
        var repository = new FakeProjectRepository(
            new Project(Guid.NewGuid(), "Работа", "#2F6FED"),
            new Project(Guid.NewGuid(), "Закрытый", "#2E7D32", isArchived: true));
        var scenario = new ProjectList(repository, new FakeEntryRepository());

        var projects = await scenario.GetAllAsync(includeArchived: true, TestContext.Current.CancellationToken);

        projects.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_WithoutArchived_HidesArchived()
    {
        var repository = new FakeProjectRepository(
            new Project(Guid.NewGuid(), "Работа", "#2F6FED"),
            new Project(Guid.NewGuid(), "Закрытый", "#2E7D32", isArchived: true));
        var scenario = new ProjectList(repository, new FakeEntryRepository());

        var projects = await scenario.GetAllAsync(includeArchived: false, TestContext.Current.CancellationToken);

        projects.Should().HaveCount(1);
        projects[0].Name.Should().Be("Работа");
    }

    [Fact]
    public async Task GetActiveProjectIdAsync_WithRunningEntry_ReturnsProjectId()
    {
        var project = new Project(Guid.NewGuid(), "Работа", "#2F6FED");
        var entryRepository = new FakeEntryRepository { Active = CreateEntry(project.Id) };
        var scenario = new ProjectList(new FakeProjectRepository(project), entryRepository);

        var activeProjectId = await scenario.GetActiveProjectIdAsync(TestContext.Current.CancellationToken);

        activeProjectId.Should().Be(project.Id);
    }

    [Fact]
    public async Task GetActiveProjectIdAsync_WithoutEntries_ReturnsNull()
    {
        var scenario = new ProjectList(new FakeProjectRepository(), new FakeEntryRepository());

        var activeProjectId = await scenario.GetActiveProjectIdAsync(TestContext.Current.CancellationToken);

        activeProjectId.Should().BeNull();
    }

    private static TimeEntry CreateEntry(Guid? projectId)
    {
        return new TimeEntry(
            id: Guid.NewGuid(),
            description: string.Empty,
            startedAt: Start,
            endedAt: null,
            pausedSeconds: 0,
            pausedAt: null,
            isBillable: false,
            projectId: projectId);
    }

    private sealed class FakeProjectRepository : IProjectRepository
    {
        private readonly List<Project> _projects;

        public FakeProjectRepository(params Project[] projects)
        {
            _projects = projects.ToList();
        }

        public Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Project>>(_projects);

        public Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_projects.FirstOrDefault(project => project.Id == id));

        public Task AddAsync(Project project, CancellationToken cancellationToken = default)
        {
            _projects.Add(project);

            return Task.CompletedTask;
        }

        public Task UpdateAsync(Project project, CancellationToken cancellationToken = default)
        {
            var index = _projects.FindIndex(item => item.Id == project.Id);

            if (index >= 0)
            {
                _projects[index] = project;
            }

            return Task.CompletedTask;
        }
    }

    private sealed class FakeEntryRepository : ITimeEntryRepository
    {
        public TimeEntry? Active { get; set; }

        public Task AddAsync(TimeEntry entry, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<TimeEntry?> GetActiveAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(Active);

        public Task<IReadOnlyList<TimeEntry>> GetRangeAsync(
            DateTimeOffset from,
            DateTimeOffset to,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<TimeEntry>>(Array.Empty<TimeEntry>());

        public Task UpdateAsync(TimeEntry entry, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
