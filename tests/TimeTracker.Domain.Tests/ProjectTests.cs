using FluentAssertions;
using TimeTracker.Domain.Clients;
using TimeTracker.Domain.Common;
using TimeTracker.Domain.Projects;
using Xunit;

namespace TimeTracker.Domain.Tests;

public class ProjectTests
{
    private static readonly ProjectColor Green = ProjectColor.FromRgb(0, 200, 100);

    [Fact]
    public void Create_WithValidName_Succeeds()
    {

        // Act
        var project = Project.Create("Мой супер проект", Green);

        // Assert
        project.Id.Value.Should().NotBeEmpty();
        project.Name.Should().Be("Мой супер проект");
        project.Color.Should().Be(Green);
        project.IsArchived.Should().BeFalse();
    }

    [Fact]
    public void Create_TrimsName()
    {
        // Act
        var project = Project.Create("  Мой супер проект  ", Green);

        // Assert
        project.Name.Should().Be("Мой супер проект");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_InvalidName_Throws(string? name)
    {
        // Act
        var act = () => Project.Create(name!, Green);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_TooLongName_Throws()
    {
        // Arrange
        var name = new string('a', 101);

        // Act
        var act = () => Project.Create(name, Green);

        //Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Rename_ChangesName()
    {
        // Arrange
        var project = Project.Create("Дед", Green);

        // Act
        project.Rename("Внук");

        //Assert
        project.Name.Should().Be("Внук");
    }

    [Fact]
    public void Rename_SameValue_NoOp()
    {
        // Arrange
        var project = Project.Create("Дед", Green);

        // Act
        project.Rename("Дед");

        //Assert
        project.Name.Should().Be("Дед");
    }

    [Fact]
    public void Recolor_ChangesColor()
    {
        // Arrange
        var project = Project.Create("Прожект", Green);
        var red = ProjectColor.FromRgb(255, 0, 0);

        // Act 
        project.Recolor(red);

        //Assert
        project.Color.Should().Be(red);
    }

    [Fact]
    public void AssignClient_ChangesClient()
    {
        // Arrange
        var project = Project.Create("Прожект", Green);
        var clientId = ClientId.New();

        // Act
        project.AssignClient(clientId);

        //Assert
        project.ClientId.Should().Be(clientId);
    }

    [Fact]
    public void Archive_SetsFlag()
    {
        // Arrange
        var project = Project.Create("Прожект", Green);

        // Act
        project.Archive();

        //Assert
        project.IsArchived.Should().BeTrue();
    }

    [Fact]
    public void Archive_AlreadyArchived_NoOp()
    {
        // Arrange
        var project = Project.Create("Прожект", Green);

        // Act
        project.Archive();
        project.Archive();

        //Assert
        project.IsArchived.Should().BeTrue();
    }

    [Fact]
    public void Unarchive_ResetsFlag()
    {
        // Arrange
        var project = Project.Create("Прожект", Green);

        // Act
        project.Archive();
        project.Unarchive();

        //Assert
        project.IsArchived.Should().BeFalse();
    }

    [Fact]
    public void Equality_SameId_AreEqual()
    {
        // Arrange
        var id = ProjectId.New();

        // Act
        var a = Project.Restore(id, "a", Green, null, false, DateTimeOffset.UtcNow);
        var b = Project.Restore(id, "b", Green, null, false, DateTimeOffset.UtcNow);

        //Assert
        a.Should().Be(b);
    }

    [Fact]
    public void Equality_DifferentId_AreNotEqual()
    {
        // Act
        var a = Project.Create("Прожект", Green);
        var b = Project.Create("Прожект", Green);

        // Assert
        a.Should().NotBe(b);
    }
}
