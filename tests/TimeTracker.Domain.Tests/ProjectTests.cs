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
}
