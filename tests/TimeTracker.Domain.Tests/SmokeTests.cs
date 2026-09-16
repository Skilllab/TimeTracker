using FluentAssertions;
using Xunit;

namespace TimeTracker.Domain.Tests;

public class SmokeTests
{
    [Fact]
    public void Project_Builds() => true.Should().BeTrue();
}
