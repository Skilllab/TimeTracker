using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using TimeTracker.Application.TimeTracking;
using TimeTracker.Infrastructure.Repositories;
using TimeTracker.Integration.Tests.Fixtures;
using Xunit;

namespace TimeTracker.Integration.Tests.Timer;

/// <summary>
/// Интеграционные тесты TimerService с реальным SQLite in-memory.
///
/// Отдельный класс с IAsyncLifetime (не IClassFixture) — каждая
/// тест-инстанция получает свежую БД. Это устраняет проблему
/// разделяемого состояния, из-за которой ранее падали тесты
/// persistence.
/// </summary>
public class TimerServicePersistenceTests : IAsyncLifetime
{
    private SqliteInMemoryFixture _fixture = null!;

    public async Task InitializeAsync()
    {
        _fixture = new SqliteInMemoryFixture();
        await _fixture.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await _fixture.DisposeAsync();
    }

    [Fact]
    public async Task StartAsync_PersistsRunningEntry()
    {
        // Arrange
        var db = _fixture.CreateDbContext();
        var repo = new TimeEntryRepository(db);
        var uow = new UnitOfWork(db);
        var timeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);
        var service = new TimerService(timeProvider, repo, uow, NullLogger<TimerService>.Instance);

        // Act
        await service.StartTimerAsync("integration test", null);

        // Assert
        var running = await repo.GetRunningAsync();
        running.Should().NotBeNull();
        running!.Description.Should().Be("integration test");
    }

    [Fact]
    public async Task StopAsync_PersistsStoppedEntry()
    {
        // Arrange
        await using var db = _fixture.CreateDbContext();
        var repo = new TimeEntryRepository(db);
        var uow = new UnitOfWork(db);
        var timeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);
        var service = new TimerService(timeProvider, repo, uow, NullLogger<TimerService>.Instance);

        await service.StartTimerAsync("stop test", null);
        timeProvider.Advance(TimeSpan.FromMinutes(30));

        // Act
        await service.StopTimerAsync();

        // Assert
        var running = await repo.GetRunningAsync();
        running.Should().BeNull();

        var all = await db.TimeEntries.ToListAsync();
        all.Should().HaveCount(1);
        all[0].Range.Duration.Value.Should().Be(TimeSpan.FromMinutes(30));
    }

    [Fact]
    public async Task RestoreAsync_AfterRestart_ContinuesExistingEntry()
    {
        // Arrange: первая сессия — стартуем и «забываем» остановить
        var timeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);
        Guid entryId;

        await using (var db = _fixture.CreateDbContext())
        {
            var repo = new TimeEntryRepository(db);
            var uow = new UnitOfWork(db);
            var service = new TimerService(timeProvider, repo, uow, NullLogger<TimerService>.Instance);

            await service.StartTimerAsync("restart test", null);
            entryId = service.CurrentEntry!.Id.Value;
            // service не останавливаем — эмулируем падение приложения
        }

        // Act: новая сессия, восстанавливаем
        await using (var db = _fixture.CreateDbContext())
        {
            var repo = new TimeEntryRepository(db);
            var uow = new UnitOfWork(db);
            var service = new TimerService(timeProvider, repo, uow, NullLogger<TimerService>.Instance);

            await service.RestoreTimerAsync();

            // Assert
            service.State.Should().Be(TimerState.Running);
            service.CurrentEntry!.Id.Value.Should().Be(entryId);
        }
    }
}
