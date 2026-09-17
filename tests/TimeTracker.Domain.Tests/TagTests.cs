using FluentAssertions;
using TimeTracker.Domain.Common;
using TimeTracker.Domain.Tags;
using Xunit;

namespace TimeTracker.Domain.Tests;

public class TagTests
{
    [Theory]
    [InlineData("срочно", "срочно")]
    [InlineData("  Срочно  ", "срочно")]
    [InlineData("СРОЧНО", "срочно")]
    [InlineData("MixedCase", "mixedcase")]
    public void Create_Normalizes(string input, string expected)
    {
        // Act
        var tag = Tag.Create(input);

        // Assert
        tag.Name.Should().Be(expected);
    }

    // Сгенерируем данные заранее
    public static IEnumerable<object?[]> GetInvalidNames()
    {
        yield return new object?[] { "" };
        yield return new object?[] { "   " };
        yield return new object?[] { null };
        yield return new object?[] { new string('a', 51) };
    }

    [Theory]
    [MemberData(nameof(GetInvalidNames))]
    public void Create_Invalid_Throws(string? name)
    {

        // Act
        var act = () => Tag.Create(name!);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_MaxLengthExactly_Succeeds()
    {
        // Arrange
        var name = new string('a', 50);
        // Act
        var tag = Tag.Create(name);

        // Assert
        tag.Name.Length.Should().Be(50);
    }

    [Fact]
    public void Equality_SameNormalizedName_AreEqual()
    {

        // Act
        var a = Tag.Create("URGENT");
        var b = Tag.Create("  urgent  ");

        // Assert
        a.Should().Be(b);
    }
}
