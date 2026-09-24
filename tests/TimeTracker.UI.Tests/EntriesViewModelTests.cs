using FluentAssertions;
using TimeTracker.Application;
using TimeTracker.Domain;
using TimeTracker.Presentation.ViewModels;
using Xunit;

namespace TimeTracker.UI.Tests;

public sealed class EntriesViewModelTests
{
    private static readonly DateTimeOffset Start = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task RefreshAsync_EntryWithProject_FillsNameAndColor()
    {
        var project = new Project(Guid.NewGuid(), "Учебный курс", "#2E7D32");
        var viewModel = new EntriesViewModel(
            new FakeEntryList(CreateEntry(project.Id)),
            new FakeProjectList(project));

        await viewModel.RefreshAsync();

        viewModel.Entries.Should().HaveCount(1);
        viewModel.Entries[0].ProjectName.Should().Be("Учебный курс");
        viewModel.Entries[0].ProjectColor.Should().Be("#2E7D32");
    }

    [Fact]
    public async Task RefreshAsync_EntryWithoutProject_LeavesMarkerEmpty()
    {
        var viewModel = new EntriesViewModel(
            new FakeEntryList(CreateEntry(null)),
            new FakeProjectList());

        await viewModel.RefreshAsync();

        viewModel.Entries.Should().HaveCount(1);
        viewModel.Entries[0].ProjectName.Should().BeEmpty();
        viewModel.Entries[0].ProjectColor.Should().BeEmpty();
    }

    [Fact]
    public async Task RefreshAsync_EntryWithUnknownProject_LeavesMarkerEmpty()
    {
        var viewModel = new EntriesViewModel(
            new FakeEntryList(CreateEntry(Guid.NewGuid())),
            new FakeProjectList());

        await viewModel.RefreshAsync();

        viewModel.Entries.Should().HaveCount(1);
        viewModel.Entries[0].Project.Should().BeNull();
        viewModel.Entries[0].ProjectName.Should().BeEmpty();
    }

    [Fact]
    public async Task RefreshAsync_KeepsEntryOrder()
    {
        var project = new Project(Guid.NewGuid(), "Работа", "#2F6FED");
        var viewModel = new EntriesViewModel(
            new FakeEntryList(CreateEntry(project.Id), CreateEntry(null)),
            new FakeProjectList(project));

        await viewModel.RefreshAsync();

        viewModel.Entries.Should().HaveCount(2);
        viewModel.Entries[0].ProjectName.Should().Be("Работа");
        viewModel.Entries[1].ProjectName.Should().BeEmpty();
    }

    private static TimeEntry CreateEntry(Guid? projectId)
    {
        return new TimeEntry(
            id: Guid.NewGuid(),
            description: "Работа",
            startedAt: Start,
            endedAt: Start.AddMinutes(30),
            pausedSeconds: 0,
            pausedAt: null,
            isBillable: false,
            projectId: projectId);
    }

    private sealed class FakeEntryList : ITimeEntryList
    {
        private readonly IReadOnlyList<TimeEntry> _entries;

        public FakeEntryList(params TimeEntry[] entries)
        {
            _entries = entries;
        }

        public Task<IReadOnlyList<TimeEntry>> GetTodayAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(_entries);
    }

    private sealed class FakeProjectList : IProjectList
    {
        private readonly IReadOnlyList<Project> _projects;

        public FakeProjectList(params Project[] projects)
        {
            _projects = projects;
        }

        public bool LastIncludeArchived { get; private set; }

        public Task<IReadOnlyList<Project>> GetAvailableAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(_projects);

        public Task<IReadOnlyList<Project>> GetAllAsync(bool includeArchived, CancellationToken cancellationToken = default)
        {
            LastIncludeArchived = includeArchived;

            return Task.FromResult(_projects);
        }

        public Task<Guid?> GetActiveProjectIdAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<Guid?>(null);
    }

    [Fact]
    public async Task RefreshAsync_ArchivedProject_KeepsNameAndMarksArchive()
    {
        var project = new Project(Guid.NewGuid(), "Учебный курс", "#2E7D32", isArchived: true);
        var viewModel = new EntriesViewModel(
            new FakeEntryList(CreateEntry(project.Id)),
            new FakeProjectList(project));

        await viewModel.RefreshAsync();

        viewModel.Entries.Should().HaveCount(1);
        viewModel.Entries[0].ProjectName.Should().Be("Учебный курс");
        viewModel.Entries[0].ProjectColor.Should().Be("#2E7D32");
        viewModel.Entries[0].IsProjectArchived.Should().BeTrue();
    }

    [Fact]
    public async Task RefreshAsync_ProjectListRequestedWithArchived()
    {
        var projectList = new FakeProjectList();
        var viewModel = new EntriesViewModel(new FakeEntryList(), projectList);

        await viewModel.RefreshAsync();

        projectList.LastIncludeArchived.Should().BeTrue();
    }
}
