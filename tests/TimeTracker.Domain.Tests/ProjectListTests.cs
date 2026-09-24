using FluentAssertions;
using TimeTracker.Application;
using TimeTracker.Domain;
using Xunit;

namespace TimeTracker.Domain.Tests;

public sealed class ProjectListTests
{
    [Fact]
    public async Task GetAvailableAsync_OrdersByName()
    {
        var repository = new FakeProjectRepository(
            new Project(Guid.NewGuid(), "Ядро", "#2F6FED"),
            new Project(Guid.NewGuid(), "Администрирование", "#2E7D32"));
        var scenario = new ProjectList(repository);

        var projects = await scenario.GetAvailableAsync(TestContext.Current.CancellationToken);

        projects.Should().HaveCount(2);
        projects[0].Name.Should().Be("Администрирование");
        projects[1].Name.Should().Be("Ядро");
    }

    [Fact]
    public async Task GetAvailableAsync_WithoutProjects_ReturnsEmptyList()
    {
        var scenario = new ProjectList(new FakeProjectRepository());

        var projects = await scenario.GetAvailableAsync(TestContext.Current.CancellationToken);

        projects.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithoutRepository_Throws()
    {
        var act = () => new ProjectList(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    private sealed class FakeProjectRepository : IProjectRepository
    {
        private readonly IReadOnlyList<Project> _projects;

        public FakeProjectRepository(params Project[] projects)
        {
            _projects = projects;
        }

        public Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(_projects);
    }
}
