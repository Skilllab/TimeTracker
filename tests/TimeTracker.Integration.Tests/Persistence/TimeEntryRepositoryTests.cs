using FluentAssertions;
using TimeTracker.Domain.TimeTracking;
using TimeTracker.Infrastructure.Repositories;
using TimeTracker.Integration.Tests.Fixtures;
using Xunit;

namespace TimeTracker.Integration.Tests.Persistence;

public class TimeEntryRepositoryTests : IClassFixture<SqliteInMemoryFixture>
{
    private readonly SqliteInMemoryFixture _fixture;

    public TimeEntryRepositoryTests(SqliteInMemoryFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetRunningAsync_ReturnsRunningEntry()
    {
        // Arrange
        await using var db = _fixture.CreateDbContext();
        var repo = new TimeEntryRepository(db);
        var running = TimeEntry.StartNew($"активна-{Guid.NewGuid():N}");

        // Act
        await repo.AddAsync(running);
        await db.SaveChangesAsync();
        var found = await repo.GetRunningAsync();

        // Assert
        found.Should().NotBeNull();
        found!.Id.Should().Be(running.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsEntry()
    {
        // Arrange
        await using var db = _fixture.CreateDbContext();
        var repo = new TimeEntryRepository(db);
        var entry = TimeEntry.StartNew($"по_айди-{Guid.NewGuid():N}");

        // Act
        await repo.AddAsync(entry);
        await db.SaveChangesAsync();
        var found = await repo.GetByIdAsync(entry.Id);

        // Assert
        found.Should().NotBeNull();
        found!.Id.Should().Be(entry.Id);
    }

    [Fact]
    public async Task Remove_DeletesEntry()
    {
        // Arrange
        await using var db = _fixture.CreateDbContext();
        var repo = new TimeEntryRepository(db);
        var entry = TimeEntry.StartNew($"на_удаление-{Guid.NewGuid():N}");

        await repo.AddAsync(entry);
        await db.SaveChangesAsync();

        // Act
        repo.Remove(entry);
        await db.SaveChangesAsync();

        // Assert
        var found = await repo.GetByIdAsync(entry.Id);
        found.Should().BeNull();
    }
}
