using FluentAssertions;
using TimeTracker.Domain;
using Xunit;

namespace TimeTracker.Domain.Tests;

public sealed class ProjectTests
{
    [Fact]
    public void Constructor_TrimsName()
    {
        var project = new Project(Guid.NewGuid(), "  Учебный курс  ", "#2F6FED");

        project.Name.Should().Be("Учебный курс");
    }

    [Fact]
    public void Constructor_EmptyName_Throws()
    {
        var act = () => new Project(Guid.NewGuid(), "   ", "#2F6FED");

        act.Should().Throw<InvalidProjectException>()
            .WithMessage("Имя проекта не может быть пустым.");
    }

    [Fact]
    public void Constructor_NameTooLong_Throws()
    {
        var name = new string('а', 101);

        var act = () => new Project(Guid.NewGuid(), name, "#2F6FED");

        act.Should().Throw<InvalidProjectException>()
            .WithMessage("Имя проекта длиннее 100 символов.");
    }

    [Theory]
    [InlineData("#2F6FED")]
    [InlineData("#2E7D32")]
    [InlineData("#B26A00")]
    [InlineData("#C62828")]
    [InlineData("#2f6fed")]
    public void Constructor_PaletteColor_KeepsColor(string color)
    {
        var project = new Project(Guid.NewGuid(), "Работа", color);

        project.Color.Should().Be(color);
    }

    [Fact]
    public void Constructor_ColorOutsidePalette_Throws()
    {
        var act = () => new Project(Guid.NewGuid(), "Работа", "#123456");

        act.Should().Throw<InvalidProjectException>()
            .WithMessage("Цвет проекта не входит в палитру.");
    }

    [Fact]
    public void NewProject_IsNotArchived()
    {
        var project = new Project(Guid.NewGuid(), "Работа", "#2F6FED");

        project.IsArchived.Should().BeFalse();
    }

    [Fact]
    public void Rename_ReturnsNewProjectWithNewName()
    {
        var project = new Project(Guid.NewGuid(), "Работа", "#2F6FED");

        var renamed = project.Rename("  Учебный курс  ");

        renamed.Name.Should().Be("Учебный курс");
        renamed.Id.Should().Be(project.Id);
        renamed.Color.Should().Be(project.Color);
        renamed.IsArchived.Should().BeFalse();
        project.Name.Should().Be("Работа");
    }

    [Fact]
    public void Rename_EmptyName_Throws()
    {
        var project = new Project(Guid.NewGuid(), "Работа", "#2F6FED");

        var act = () => project.Rename("   ");

        act.Should().Throw<InvalidProjectException>()
            .WithMessage("Имя проекта не может быть пустым.");
    }

    [Fact]
    public void ChangeColor_ColorOutsidePalette_Throws()
    {
        var project = new Project(Guid.NewGuid(), "Работа", "#2F6FED");

        var act = () => project.ChangeColor("#123456");

        act.Should().Throw<InvalidProjectException>()
            .WithMessage("Цвет проекта не входит в палитру.");
    }

    [Fact]
    public void Archive_ThenUnarchive_RestoresAvailability()
    {
        var project = new Project(Guid.NewGuid(), "Работа", "#2F6FED");

        var archived = project.Archive();

        archived.IsArchived.Should().BeTrue();
        archived.Unarchive().IsArchived.Should().BeFalse();
    }

    [Fact]
    public void Archive_KeepsNameAndColor()
    {
        var project = new Project(Guid.NewGuid(), "Работа", "#2E7D32");

        var archived = project.Archive();

        archived.Name.Should().Be("Работа");
        archived.Color.Should().Be("#2E7D32");
    }
}
