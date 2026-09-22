using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Infrastructure;
using Xunit;

namespace TimeTracker.Domain.Tests;


public sealed class TimeEntryRepositoryTests
{
    private static readonly DateTimeOffset Start = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task AddAsync_ThenSave_KeepsEntryForNewContext()
    {
        using var connection = CreateConnection();

        await using (var context = CreateContext(connection))
        {
            var repository = new TimeEntryRepository(context);
            var unitOfWork = new EfUnitOfWork(context);

            await repository.AddAsync(CreateEntry("Работа", Start, Start.AddMinutes(30)), TestContext.Current.CancellationToken);
            await unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using (var context = CreateContext(connection))
        {
            var repository = new TimeEntryRepository(context);

            var entries = await repository.GetRangeAsync(Start.AddHours(-1), Start.AddHours(1), TestContext.Current.CancellationToken);

            entries.Should().HaveCount(1);
            entries[0].Description.Should().Be("Работа");
        }
    }

    [Fact]
    public async Task GetActiveAsync_ReturnsOpenEntry()
    {
        using var connection = CreateConnection();
        await using var context = CreateContext(connection);

        var repository = new TimeEntryRepository(context);
        var unitOfWork = new EfUnitOfWork(context);

        await repository.AddAsync(CreateEntry("Идущая", Start, null), TestContext.Current.CancellationToken);
        await unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var active = await repository.GetActiveAsync(TestContext.Current.CancellationToken);

        active.Should().NotBeNull();
        active!.Description.Should().Be("Идущая");
    }

    [Fact]
    public async Task GetRangeAsync_ExcludesEntriesOutsideRange()
    {
        using var connection = CreateConnection();
        await using var context = CreateContext(connection);

        var repository = new TimeEntryRepository(context);
        var unitOfWork = new EfUnitOfWork(context);

        await repository.AddAsync(CreateEntry("Сегодня", Start, Start.AddMinutes(10)), TestContext.Current.CancellationToken);
        await repository.AddAsync(CreateEntry("Завтра", Start.AddDays(1), Start.AddDays(1).AddMinutes(10)), TestContext.Current.CancellationToken);
        await unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var entries = await repository.GetRangeAsync(Start.AddHours(-12), Start.AddHours(12), TestContext.Current.CancellationToken);

        entries.Should().HaveCount(1);
        entries[0].Description.Should().Be("Сегодня");
    }

    [Fact]
    public async Task GetRangeAsync_OrdersByStart()
    {
        using var connection = CreateConnection();
        await using var context = CreateContext(connection);

        var repository = new TimeEntryRepository(context);
        var unitOfWork = new EfUnitOfWork(context);

        await repository.AddAsync(CreateEntry("Позже", Start.AddHours(2), Start.AddHours(3)), TestContext.Current.CancellationToken);
        await repository.AddAsync(CreateEntry("Раньше", Start, Start.AddHours(1)), TestContext.Current.CancellationToken);
        await unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var entries = await repository.GetRangeAsync(Start.AddHours(-1), Start.AddHours(5), TestContext.Current.CancellationToken);

        entries.Should().HaveCount(2);
        entries[0].Description.Should().Be("Раньше");
        entries[1].Description.Should().Be("Позже");
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

    private static TimeEntry CreateEntry(string description, DateTimeOffset startedAt, DateTimeOffset? endedAt)
    {
        return new TimeEntry(Guid.NewGuid(), description, startedAt, endedAt, 0, false, null);
    }
}
