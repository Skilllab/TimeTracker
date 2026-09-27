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
    [InlineData("#123456")]
    public void Constructor_ValidColor_KeepsColor(string color)
    {
        var project = new Project(Guid.NewGuid(), "Работа", color);

        project.Color.Should().Be(color);
    }

    [Fact]
    public void Constructor_ColorWithWrongFormat_Throws()
    {
        var act = () => new Project(Guid.NewGuid(), "Работа", "красный");

        act.Should().Throw<InvalidProjectException>()
            .WithMessage("Цвет проекта должен быть записан в виде #RRGGBB.");
    }

    [Fact]
    public void Rename_ReturnsNewProjectWithNewName()
    {
        var project = new Project(Guid.NewGuid(), "Работа", "#2F6FED");

        var renamed = project.Rename("  Учебный курс  ");

        renamed.Name.Should().Be("Учебный курс");
        renamed.Id.Should().Be(project.Id);
        renamed.Color.Should().Be(project.Color);
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
    public void ChangeColor_WithWrongFormat_Throws()
    {
        var project = new Project(Guid.NewGuid(), "Работа", "#2F6FED");

        var act = () => project.ChangeColor("красный");

        act.Should().Throw<InvalidProjectException>()
            .WithMessage("Цвет проекта должен быть записан в виде #RRGGBB.");
    }
}
