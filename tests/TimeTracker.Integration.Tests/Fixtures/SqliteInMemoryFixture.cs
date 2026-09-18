using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Infrastructure.Persistence;
using Xunit;

namespace TimeTracker.Integration.Tests.Fixtures;

/// <summary>
/// Фикстура для тестов на SQLite in-memory.
///
/// Соединение открывается один раз в InitializeAsync и живёт
/// до DisposeAsync. Все CreateDbContext() используют одно и то же
/// соединение — поэтому все тесты в классе видят одну БД.
///
/// ВАЖНО: если открывать новое соединение на каждый DbContext,
/// каждый получит свою изолированную БД в памяти, и persistence
/// не будет проверяться.
/// </summary>
public sealed class SqliteInMemoryFixture : IAsyncLifetime
{
    private SqliteConnection _connection = null!;

    /// <summary>
    /// Создать новый DbContext, привязанный к общему in-memory
    /// соединению. Каждый вызов — новый экземпляр, но одна БД.
    /// </summary>
    public AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        return new AppDbContext(options);
    }

    public async Task InitializeAsync()
    {
        // Arrange (lifecycle): открыть соединение и создать схему.
        _connection = new SqliteConnection("DataSource=:memory:");
        await _connection.OpenAsync();

        await using var db = CreateDbContext();
        await db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}