using FluentAssertions;
using Xunit;

namespace TimeTracker.Integration.Tests;

public class SmokeTests
{
    [Fact]
    public void Project_Builds() => true.Should().BeTrue();
}
