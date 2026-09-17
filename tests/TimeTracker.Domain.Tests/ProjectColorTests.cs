using FluentAssertions;
using TimeTracker.Domain.Common;
using TimeTracker.Domain.Projects;
using Xunit;

namespace TimeTracker.Domain.Tests;

public class ProjectColorTests
{
    [Theory]
    [InlineData("#FF0000")]
    [InlineData("#00ff00")]
    [InlineData("#0000FF")]
    [InlineData("#abcdef")]
    public void FromHex_ValidHex_Succeeds(string hex)
    {
        // Act
        var color = ProjectColor.FromHex(hex);

        // Assert
        color.Hex.Should().Be(hex.ToUpperInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("FF0000")]
    [InlineData("#FF00")]
    [InlineData("#FF00000")]
    [InlineData("#GGGGGG")]
    [InlineData("#FF00ZZ")]
    public void FromHex_Invalid_Throws(string hex)
    {
        // Act
        var act = () => ProjectColor.FromHex(hex);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void FromRgb_ProducesUppercaseHex()
    {
        // Act
        var color = ProjectColor.FromRgb(0xAB, 0xCD, 0xEF);

        // Assert
        color.Hex.Should().Be("#ABCDEF");
    }

    [Fact]
    public void Equality_SameHex_AreEqual()
    {
        // Act
        var a = ProjectColor.FromHex("#ABCDEF");
        var b = ProjectColor.FromHex("#abcdef");

        // Assert
        a.Should().Be(b);
    }
}
