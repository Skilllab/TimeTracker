using FluentAssertions;
using TimeTracker.Application;
using TimeTracker.Domain;
using Xunit;

namespace TimeTracker.Domain.Tests;

public sealed class ProjectEditorTests
{
    private static readonly DateTimeOffset Start = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task CreateAsync_AddsActiveProject()
    {
        var repository = new FakeProjectRepository();
        var editor = Create(repository, new FakeEntryRepository());

        await editor.CreateAsync("Учебный курс", "#2E7D32", TestContext.Current.CancellationToken);

        repository.Added.Should().HaveCount(1);
        repository.Added[0].Name.Should().Be("Учебный курс");
        repository.Added[0].IsArchived.Should().BeFalse();
    }

    [Fact]
    public async Task RenameAsync_SavesNewName()
    {
        var project = new Project(Guid.NewGuid(), "Работа", "#2F6FED");
        var repository = new FakeProjectRepository(project);
        var editor = Create(repository, new FakeEntryRepository());

        await editor.RenameAsync(project.Id, "Учебный курс", TestContext.Current.CancellationToken);

        repository.Updated.Should().HaveCount(1);
        repository.Updated[0].Name.Should().Be("Учебный курс");
        repository.Updated[0].Id.Should().Be(project.Id);
    }

    [Fact]
    public async Task ChangeColorAsync_SavesNewColor()
    {
        var project = new Project(Guid.NewGuid(), "Работа", "#2F6FED");
        var repository = new FakeProjectRepository(project);
        var editor = Create(repository, new FakeEntryRepository());

        await editor.ChangeColorAsync(project.Id, "#B26A00", TestContext.Current.CancellationToken);

        repository.Updated.Should().HaveCount(1);
        repository.Updated[0].Color.Should().Be("#B26A00");
    }

    [Fact]
    public async Task ArchiveAsync_WithoutRunningEntry_ArchivesProject()
    {
        var project = new Project(Guid.NewGuid(), "Работа", "#2F6FED");
        var repository = new FakeProjectRepository(project);
        var editor = Create(repository, new FakeEntryRepository());

        await editor.ArchiveAsync(project.Id, TestContext.Current.CancellationToken);

        repository.Updated.Should().HaveCount(1);
        repository.Updated[0].IsArchived.Should().BeTrue();
    }

    [Fact]
    public async Task ArchiveAsync_WithRunningEntryOnProject_Throws()
    {
        var project = new Project(Guid.NewGuid(), "Работа", "#2F6FED");
        var repository = new FakeProjectRepository(project);
        var entryRepository = new FakeEntryRepository
        {
            Active = CreateEntry(project.Id)
        };
        var editor = Create(repository, entryRepository);

        var act = async () => await editor.ArchiveAsync(project.Id, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidProjectException>()
            .WithMessage("Нельзя архивировать проект незавершенной записи.");
        repository.Updated.Should().BeEmpty();
    }

    [Fact]
    public async Task ArchiveAsync_WithRunningEntryOnAnotherProject_ArchivesProject()
    {
        var project = new Project(Guid.NewGuid(), "Работа", "#2F6FED");
        var repository = new FakeProjectRepository(project);
        var entryRepository = new FakeEntryRepository
        {
            Active = CreateEntry(Guid.NewGuid())
        };
        var editor = Create(repository, entryRepository);

        await editor.ArchiveAsync(project.Id, TestContext.Current.CancellationToken);

        repository.Updated.Should().HaveCount(1);
        repository.Updated[0].IsArchived.Should().BeTrue();
    }

    [Fact]
    public async Task UnarchiveAsync_ReturnsProjectFromArchive()
    {
        var project = new Project(Guid.NewGuid(), "Работа", "#2F6FED", isArchived: true);
        var repository = new FakeProjectRepository(project);
        var editor = Create(repository, new FakeEntryRepository());

        await editor.UnarchiveAsync(project.Id, TestContext.Current.CancellationToken);

        repository.Updated.Should().HaveCount(1);
        repository.Updated[0].IsArchived.Should().BeFalse();
    }

    [Fact]
    public async Task RenameAsync_UnknownProject_Throws()
    {
        var repository = new FakeProjectRepository();
        var editor = Create(repository, new FakeEntryRepository());

        var act = async () => await editor.RenameAsync(Guid.NewGuid(), "Работа", TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidProjectException>()
            .WithMessage("Проект не найден.");
    }

    private static ProjectEditor Create(FakeProjectRepository repository, FakeEntryRepository entryRepository)
    {
        return new ProjectEditor(repository, entryRepository, new FakeUnitOfWork());
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

        public List<Project> Added { get; } = new();

        public List<Project> Updated { get; } = new();

        public Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Project>>(_projects);

        public Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_projects.FirstOrDefault(project => project.Id == id));

        public Task AddAsync(Project project, CancellationToken cancellationToken = default)
        {
            Added.Add(project);
            _projects.Add(project);

            return Task.CompletedTask;
        }

        public Task UpdateAsync(Project project, CancellationToken cancellationToken = default)
        {
            Updated.Add(project);

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

        public Task<IReadOnlyList<TimeEntry>> GetRangeAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<TimeEntry>>(Array.Empty<TimeEntry>());

        public Task UpdateAsync(TimeEntry entry, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
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
