using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Domain.Clients;
using TimeTracker.Integration.Tests.Fixtures;
using Xunit;

namespace TimeTracker.Integration.Tests.Persistence;

public class ClientPersistenceTests : IClassFixture<SqliteInMemoryFixture>
{
    private readonly SqliteInMemoryFixture _fixture;

    public ClientPersistenceTests(SqliteInMemoryFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CanPersist_AndReadBack_Client()
    {
        // Arrange
        var name = $"Котофейня-{Guid.NewGuid():N}";

        // Act (write)
        await using (var db = _fixture.CreateDbContext())
        {
            db.Clients.Add(Client.Create(name));
            await db.SaveChangesAsync();
        }

        // Act (read back)
        await using (var db = _fixture.CreateDbContext())
        {
            var loaded = await db.Clients.FirstAsync(c => c.Name == name);

            // Assert
            loaded.Name.Should().Be(name);
        }
    }
}
