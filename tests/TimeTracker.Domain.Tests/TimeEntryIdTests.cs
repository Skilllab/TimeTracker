using FluentAssertions;
using TimeTracker.Domain.Common;
using TimeTracker.Domain.TimeTracking;
using Xunit;

namespace TimeTracker.Domain.Tests;

public class TimeEntryIdTests
{
    [Fact]
    public void New_CreatesNonEmptyId()
    {
        // Act
        var id = TimeEntryId.New();

        // Assert
        id.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void New_CreatesUniqueIds()
    {
        // Act
        var a = TimeEntryId.New();
        var b = TimeEntryId.New();

        // Assert
        a.Should().NotBe(b);
    }

    [Fact]
    public void From_EmptyGuid_Throws()
    {
        // Act
        var act = () => TimeEntryId.From(Guid.Empty);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void From_ValidGuid_Creates()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var id = TimeEntryId.From(guid);

        // Assert
        id.Value.Should().Be(guid);
    }

    [Fact]
    public void Equality_SameGuid_AreEqual()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var a = TimeEntryId.From(guid);
        var b = TimeEntryId.From(guid);

        // Assert
        a.Should().Be(b);
    }
}
