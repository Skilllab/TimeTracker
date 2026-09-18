using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Domain.Clients;
using TimeTracker.Domain.Projects;
using TimeTracker.Integration.Tests.Fixtures;
using Xunit;

namespace TimeTracker.Integration.Tests.Persistence;

public class ProjectPersistenceTests : IClassFixture<SqliteInMemoryFixture>
{
    private readonly SqliteInMemoryFixture _fixture;

    public ProjectPersistenceTests(SqliteInMemoryFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CanPersist_AndReadBack_Project()
    {
        // Arrange
        var color = ProjectColor.FromRgb(0xAB, 0xCD, 0xEF);

        // Act (write)
        await using (var db = _fixture.CreateDbContext())
        {
            var project = Project.Create("Прожект", color);
            db.Projects.Add(project);
            await db.SaveChangesAsync();
        }

        // Act (read back)
        await using (var db = _fixture.CreateDbContext())
        {
            var loaded = await db.Projects.FirstAsync(p => p.Name == "Прожект");

            // Assert
            loaded.Color.Should().Be(color);
            loaded.IsArchived.Should().BeFalse();
        }
    }

    [Fact]
    public async Task CanPersist_ProjectWithClient()
    {
        // Arrange
        var clientName = $"Клиент-{Guid.NewGuid():N}";
        var projectName = $"Прожект-{Guid.NewGuid():N}";

        // Act (write)
        await using (var db = _fixture.CreateDbContext())
        {
            var client = Client.Create(clientName);
            db.Clients.Add(client);

            var project = Project.Create(
                projectName,
                ProjectColor.FromRgb(0, 0, 0),
                client.Id);
            db.Projects.Add(project);

            await db.SaveChangesAsync();
        }

        // Act (read back)
        await using (var db = _fixture.CreateDbContext())
        {
            var loaded = await db.Projects.FirstAsync(p => p.Name == projectName);

            // Assert
            loaded.ClientId.Should().NotBeNull();
        }
    }
}
