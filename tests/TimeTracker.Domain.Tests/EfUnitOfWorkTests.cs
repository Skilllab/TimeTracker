using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Infrastructure;
using Xunit;

namespace TimeTracker.Domain.Tests;


public sealed class EfUnitOfWorkTests
{
    private static readonly DateTimeOffset Start = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task SaveChangesAsync_WithoutSaving_LeavesStoreEmpty()
    {
        using var connection = CreateConnection();
        await using var context = CreateContext(connection);

        await new TimeEntryRepository(context).AddAsync(CreateEntry(), TestContext.Current.CancellationToken);

        await using var other = CreateContext(connection);
        var stored = await new TimeEntryRepository(other).GetRangeAsync(Start.AddHours(-1), Start.AddHours(1), TestContext.Current.CancellationToken);

        stored.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveChangesAsync_AfterSaving_StoresEntry()
    {
        using var connection = CreateConnection();
        await using var context = CreateContext(connection);

        var repository = new TimeEntryRepository(context);
        var unitOfWork = new EfUnitOfWork(context);

        await repository.AddAsync(CreateEntry(), TestContext.Current.CancellationToken);
        await unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        await using var other = CreateContext(connection);
        var stored = await new TimeEntryRepository(other).GetRangeAsync(Start.AddHours(-1), Start.AddHours(1), TestContext.Current.CancellationToken);

        stored.Should().HaveCount(1);
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

    private static TimeEntry CreateEntry()
    {
        return new TimeEntry(Guid.NewGuid(), "Работа", Start, Start.AddMinutes(30), 0, false, null);
    }
}
