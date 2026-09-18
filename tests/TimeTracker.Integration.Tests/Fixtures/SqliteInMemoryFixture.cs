using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Infrastructure.Persistence;
using TimeTracker.Infrastructure.Persistence.Interceptors;
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
public sealed class SqliteInMemoryFixture : IAsyncLifetime, IAsyncDisposable
{
    private SqliteConnection _connection = null!;

    public AppDbContext CreateDbContext()
    {
        var interceptor = new TimeEntryShadowPropertiesInterceptor();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .AddInterceptors(interceptor)
            .Options;

        return new AppDbContext(options);
    }

    public async Task InitializeAsync()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        await _connection.OpenAsync();

        await using var db = CreateDbContext();
        await db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        if (_connection != null)
            await _connection.DisposeAsync();
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        await DisposeAsync();
    }
}
