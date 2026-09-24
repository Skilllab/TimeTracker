using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Domain;
using TimeTracker.Infrastructure;
using Xunit;

namespace TimeTracker.Domain.Tests;

public sealed class ProjectRepositoryTests
{
    private static readonly DateTimeOffset Start = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task AddProject_ThenSave_KeepsNameAndColor()
    {
        using var connection = CreateConnection();

        var project = new Project(Guid.NewGuid(), "Учебный курс", "#2E7D32");

        await using (var context = CreateContext(connection))
        {
            context.Projects.Add(project);
            await new EfUnitOfWork(context).SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using (var context = CreateContext(connection))
        {
            var projects = await new ProjectRepository(context).GetAllAsync(TestContext.Current.CancellationToken);

            projects.Should().HaveCount(1);
            projects[0].Name.Should().Be("Учебный курс");
            projects[0].Color.Should().Be("#2E7D32");
        }
    }

    [Fact]
    public async Task EntryWithProject_KeepsProjectId()
    {
        using var connection = CreateConnection();
        await using var context = CreateContext(connection);

        var project = new Project(Guid.NewGuid(), "Внутренние задачи", "#2F6FED");

        context.Projects.Add(project);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var entry = new TimeEntry(
            id: Guid.NewGuid(),
            description: "Работа",
            startedAt: Start,
            endedAt: Start.AddMinutes(30),
            pausedSeconds: 0,
            pausedAt: null,
            isBillable: false,
            projectId: project.Id);

        await new TimeEntryRepository(context).AddAsync(entry, TestContext.Current.CancellationToken);
        await new EfUnitOfWork(context).SaveChangesAsync(TestContext.Current.CancellationToken);

        await using var other = CreateContext(connection);
        var stored = await new TimeEntryRepository(other)
            .GetRangeAsync(Start.AddHours(-1), Start.AddHours(1), TestContext.Current.CancellationToken);

        stored.Should().HaveCount(1);
        stored[0].ProjectId.Should().Be(project.Id);
    }

    [Fact]
    public async Task EntryWithoutProject_KeepsEmptyLink()
    {
        using var connection = CreateConnection();
        await using var context = CreateContext(connection);

        var entry = new TimeEntry(
            id: Guid.NewGuid(),
            description: "Работа",
            startedAt: Start,
            endedAt: Start.AddMinutes(30),
            pausedSeconds: 0,
            pausedAt: null,
            isBillable: false,
            projectId: null);

        await new TimeEntryRepository(context).AddAsync(entry, TestContext.Current.CancellationToken);
        await new EfUnitOfWork(context).SaveChangesAsync(TestContext.Current.CancellationToken);

        await using var other = CreateContext(connection);
        var stored = await new TimeEntryRepository(other)
            .GetRangeAsync(Start.AddHours(-1), Start.AddHours(1), TestContext.Current.CancellationToken);

        stored[0].ProjectId.Should().BeNull();
    }

    private static SqliteConnection CreateConnection()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        return connection;
    }

    private static TimeTrackerDbContext CreateContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<TimeTrackerDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new TimeTrackerDbContext(options);
        context.Database.EnsureCreated();

        return context;
    }

    [Fact]
    public async Task UpdateAsync_AfterArchive_KeepsArchivedFlag()
    {
        using var connection = CreateConnection();

        var project = new Project(Guid.NewGuid(), "Работа", "#2F6FED");

        await using (var context = CreateContext(connection))
        {
            await new ProjectRepository(context).AddAsync(project, TestContext.Current.CancellationToken);
            await new EfUnitOfWork(context).SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using (var context = CreateContext(connection))
        {
            var stored = await new ProjectRepository(context).GetByIdAsync(project.Id, TestContext.Current.CancellationToken);
            stored.Should().NotBeNull();

            await new ProjectRepository(context).UpdateAsync(stored!.Archive(), TestContext.Current.CancellationToken);
            await new EfUnitOfWork(context).SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using (var context = CreateContext(connection))
        {
            var stored = await new ProjectRepository(context).GetByIdAsync(project.Id, TestContext.Current.CancellationToken);

            stored.Should().NotBeNull();
            stored!.IsArchived.Should().BeTrue();
            stored.Name.Should().Be("Работа");
        }
    }

    [Fact]
    public async Task GetByIdAsync_UnknownProject_ReturnsNull()
    {
        using var connection = CreateConnection();
        await using var context = CreateContext(connection);

        var stored = await new ProjectRepository(context).GetByIdAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        stored.Should().BeNull();
    }
}
