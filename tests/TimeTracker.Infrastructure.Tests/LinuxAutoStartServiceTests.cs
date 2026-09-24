using System.Runtime.InteropServices;
using FluentAssertions;
using TimeTracker.Infrastructure.Linux;
using Xunit;

namespace TimeTracker.Infrastructure.Tests;

public sealed class LinuxAutoStartServiceTests : IDisposable
{
    private readonly LinuxAutoStartService _service;
    private readonly string _testAutostartDir;

    public LinuxAutoStartServiceTests()
    {
        _service = new LinuxAutoStartService();
        _testAutostartDir = Path.Combine(Path.GetTempPath(), "test-autostart");
    }

    [Fact]
    public void Enable_CreatesDesktopFile()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return; // Skip on non-Linux platforms
        }

        _service.Enable();

        _service.IsEnabled().Should().BeTrue();
    }

    [Fact]
    public void Disable_RemovesDesktopFile()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return; // Skip on non-Linux platforms
        }

        _service.Enable();
        _service.Disable();

        _service.IsEnabled().Should().BeFalse();
    }

    [Fact]
    public void IsEnabled_WhenDisabled_ReturnsFalse()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return; // Skip on non-Linux platforms
        }

        _service.Disable();

        _service.IsEnabled().Should().BeFalse();
    }

    [Fact]
    public void DesktopFile_HasCorrectContent()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return; // Skip on non-Linux platforms
        }

        _service.Enable();

        var autostartDir = GetTestAutostartDirectory();
        var desktopFile = Path.Combine(autostartDir, "timetracker.desktop");

        File.Exists(desktopFile).Should().BeTrue();
        var content = File.ReadAllText(desktopFile);
        content.Should().Contain("[Desktop Entry]");
        content.Should().Contain("Type=Application");
        content.Should().Contain("Name=TimeTracker");
    }

    public void Dispose()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            try
            {
                _service.Disable();
                if (Directory.Exists(_testAutostartDir))
                {
                    Directory.Delete(_testAutostartDir, true);
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    private string GetTestAutostartDirectory()
    {
        var xdgConfigHome = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
        if (!string.IsNullOrEmpty(xdgConfigHome))
        {
            return Path.Combine(xdgConfigHome, "autostart");
        }

        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(home, ".config", "autostart");
    }
}
