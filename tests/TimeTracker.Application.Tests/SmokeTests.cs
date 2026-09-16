using FluentAssertions;
using Xunit;

namespace TimeTracker.Application.Tests;

public class SmokeTests
{
    [Fact]
    public void Project_Builds() => true.Should().BeTrue();
}
