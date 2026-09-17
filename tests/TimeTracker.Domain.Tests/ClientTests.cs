using FluentAssertions;
using TimeTracker.Domain.Clients;
using TimeTracker.Domain.Common;
using Xunit;

namespace TimeTracker.Domain.Tests;

public class ClientTests
{
    [Fact]
    public void Create_ValidName_Succeeds()
    {
        // Act
        var client = Client.Create("Хакеры нашего двора");

        // Assert
        client.Id.Value.Should().NotBeEmpty();
        client.Name.Should().Be("Хакеры нашего двора");
        client.IsArchived.Should().BeFalse();
    }

    [Fact]
    public void Create_TrimsName()
    {
        // Act
        var client = Client.Create("  Хакеры    ");

        // Assert
        client.Name.Should().Be("Хакеры");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_InvalidName_Throws(string? name)
    {
        // Act
        var act = () => Client.Create(name!);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_TooLongName_Throws()
    {
        // Arrange
        var name = new string('a', 101);

        // Act
        var act = () => Client.Create(name);

        //Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Rename_ChangesName()
    {
        // Arrange
        var client = Client.Create("Дядя");

        // Act
        client.Rename("Петя");

        //Assert
        client.Name.Should().Be("Петя");
    }

    [Fact]
    public void Archive_SetsFlag()
    {
        // Arrange
        var client = Client.Create("Дядя");

        //Act
        client.Archive();

        //Assert
        client.IsArchived.Should().BeTrue();
    }

    [Fact]
    public void Unarchive_ResetsFlag()
    {
        // Arrange
        var client = Client.Create("Петя");

        //Act
        client.Archive();
        client.Unarchive();

        //Assert
        client.IsArchived.Should().BeFalse();
    }

    [Fact]
    public void Equality_SameId_AreEqual()
    {
        //Arrange
        var id = ClientId.New();

        //Act
        var a = Client.Restore(id, "a", false, DateTimeOffset.UtcNow);
        var b = Client.Restore(id, "b", false, DateTimeOffset.UtcNow);

        //Assert
        a.Should().Be(b);
    }

    [Fact]
    public void Equality_DifferentId_AreNotEqual()
    {
        // Act
        var a = Client.Create("x");
        var b = Client.Create("x");

        // Assert
        a.Should().NotBe(b);
    }
}
