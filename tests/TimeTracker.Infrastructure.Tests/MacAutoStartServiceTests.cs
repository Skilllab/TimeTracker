using System.Runtime.InteropServices;
using FluentAssertions;
using TimeTracker.Infrastructure.Mac;
using Xunit;

namespace TimeTracker.Infrastructure.Tests;

public sealed class MacAutoStartServiceTests : IDisposable
{
    private readonly MacAutoStartService _service;

    public MacAutoStartServiceTests()
    {
        _service = new MacAutoStartService();
    }

    [Fact]
    public void Enable_CreatesPlistFile()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return; // Skip on non-macOS platforms
        }

        _service.Enable();

        _service.IsEnabled().Should().BeTrue();
    }

    [Fact]
    public void Disable_RemovesPlistFile()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return; // Skip on non-macOS platforms
        }

        _service.Enable();
        _service.Disable();

        _service.IsEnabled().Should().BeFalse();
    }

    [Fact]
    public void IsEnabled_WhenDisabled_ReturnsFalse()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return; // Skip on non-macOS platforms
        }

        _service.Disable();

        _service.IsEnabled().Should().BeFalse();
    }

    [Fact]
    public void PlistFile_HasCorrectContent()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return; // Skip on non-macOS platforms
        }

        _service.Enable();

        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var plistFile = Path.Combine(home, "Library", "LaunchAgents", "com.timetracker.plist");

        File.Exists(plistFile).Should().BeTrue();
        var content = File.ReadAllText(plistFile);
        content.Should().Contain("<?xml version=\"1.0\"");
        content.Should().Contain("<plist version=\"1.0\">");
        content.Should().Contain("<key>Label</key>");
        content.Should().Contain("com.timetracker");
        content.Should().Contain("<key>RunAtLoad</key>");
        content.Should().Contain("<true/>");
    }

    public void Dispose()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            try
            {
                _service.Disable();
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}
