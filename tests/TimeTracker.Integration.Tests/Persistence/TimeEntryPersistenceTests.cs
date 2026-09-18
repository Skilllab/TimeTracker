using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Domain.Projects;
using TimeTracker.Domain.Tags;
using TimeTracker.Domain.TimeTracking;
using TimeTracker.Integration.Tests.Fixtures;
using Xunit;

namespace TimeTracker.Integration.Tests.Persistence;

public class TimeEntryPersistenceTests : IClassFixture<SqliteInMemoryFixture>
{
    private readonly SqliteInMemoryFixture _fixture;

    public TimeEntryPersistenceTests(SqliteInMemoryFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CanPersist_AndReadBack_StoppedEntry()
    {
        // Arrange
        var start = DateTimeOffset.UtcNow.AddHours(-1);
        var end = DateTimeOffset.UtcNow;

        // Act (write)
        await using (var db = _fixture.CreateDbContext())
        {
            var entry = TimeEntry.StartNew("Тестовая задача", startedAt: start);
            entry.Stop(end);
            db.TimeEntries.Add(entry);
            await db.SaveChangesAsync();
        }

        // Act (read back через новый DbContext)
        await using (var db = _fixture.CreateDbContext())
        {
            var loaded = await db.TimeEntries.FirstAsync(e => e.Description == "Тестовая задача");

            // Assert
            loaded.Range.Start.Should().Be(start);
            loaded.Range.End.Should().Be(end);
            loaded.IsBillable.Should().BeTrue();
        }
    }

    [Fact]
    public async Task CanPersist_RunningEntry()
    {
        // Act (write)
        await using (var db = _fixture.CreateDbContext())
        {
            var entry = TimeEntry.StartNew("Активная задача");
            db.TimeEntries.Add(entry);
            await db.SaveChangesAsync();
        }

        // Act (read back)
        await using (var db = _fixture.CreateDbContext())
        {
            var loaded = await db.TimeEntries.FirstAsync(e => e.Description == "Активная задача");

            // Assert
            loaded.Range.IsRunning.Should().BeTrue();
        }
    }

    [Fact]
    public async Task CanPersist_WithProjectAndTags()
    {
        // Arrange
        var project = Project.Create("Тестовый проект", ProjectColor.FromRgb(100, 150, 200));
        Guid entryId;

        // Act (write)
        await using (var db = _fixture.CreateDbContext())
        {
            db.Projects.Add(project);

            var entry = TimeEntry.StartNew("под проект", project.Id);
            entry.AddTag(Tag.Create("ASPA"));
            entry.AddTag(Tag.Create("платное"));
            entry.Stop(DateTimeOffset.UtcNow);

            db.TimeEntries.Add(entry);
            await db.SaveChangesAsync();

            entryId = entry.Id.Value;
        }

        // Act (read back)
        await using (var db = _fixture.CreateDbContext())
        {
            var loaded = await db.TimeEntries
                .Include(e => e.Tags)
                .FirstAsync(e => e.Id == TimeEntryId.From(entryId));

            // Assert
            loaded.ProjectId.Should().Be(project.Id);
            loaded.Tags.Should().HaveCount(2);
            loaded.Tags.Should().Contain(Tag.Create("ASAP"));
            loaded.Tags.Should().Contain(Tag.Create("платное"));
        }
    }

    [Fact]
    public async Task CanQuery_RunningEntry()
    {
        // Act (write)
        await using (var db = _fixture.CreateDbContext())
        {
            var running = TimeEntry.StartNew("Активная сессия");
            var stopped = TimeEntry.StartNew("Старая сессия", startedAt: DateTimeOffset.UtcNow.AddHours(-2));
            stopped.Stop(DateTimeOffset.UtcNow.AddHours(-1));

            db.TimeEntries.AddRange(running, stopped);
            await db.SaveChangesAsync();
        }

        // Act (query)
        await using (var db = _fixture.CreateDbContext())
        {
            var found = await db.TimeEntries
                .Where(e => e.Range.End == null)
                .FirstOrDefaultAsync();

            // Assert
            found.Should().NotBeNull();
            found!.Description.Should().Be("Активная сессия");
        }
    }
}
