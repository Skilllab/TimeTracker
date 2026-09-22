using FluentAssertions;
using TimeTracker.Infrastructure;
using Xunit;

namespace TimeTracker.Domain.Tests;


public sealed class AppDataPathsTests
{
    [Fact]
    public void DataDirectory_IsAbsoluteAndNamedAfterApp()
    {
        var paths = new AppDataPaths();

        paths.DataDirectory.Should().NotBeNullOrWhiteSpace();
        Path.IsPathRooted(paths.DataDirectory).Should().BeTrue();
        Path.GetFileName(paths.DataDirectory).Should().Be("TimeTracker");
    }

    [Fact]
    public void DataDirectory_MatchesRulesOfCurrentSystem()
    {
        var paths = new AppDataPaths();

        if (OperatingSystem.IsWindows())
        {
            var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            paths.DataDirectory.Should().StartWith(local);
        }
        else
        {
            paths.DataDirectory.Should().NotContain("\\");
        }
    }
}
