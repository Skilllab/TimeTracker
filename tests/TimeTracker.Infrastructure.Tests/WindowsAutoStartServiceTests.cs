using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using FluentAssertions;
using TimeTracker.Infrastructure.Windows;
using Xunit;

namespace TimeTracker.Infrastructure.Tests;

[SupportedOSPlatform("windows")]
public sealed class WindowsAutoStartServiceTests : IDisposable
{
    private const string TestAppName = "TimeTrackerTest";
    private readonly WindowsAutoStartService _service;

    public WindowsAutoStartServiceTests()
    {
        _service = new WindowsAutoStartService();
    }

    [Fact]
    public void Enable_CreatesRegistryEntry()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return; // Skip on non-Windows platforms
        }

        _service.Enable();

        _service.IsEnabled().Should().BeTrue();
    }

    [Fact]
    public void Disable_RemovesRegistryEntry()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return; // Skip on non-Windows platforms
        }

        _service.Enable();
        _service.Disable();

        _service.IsEnabled().Should().BeFalse();
    }

    [Fact]
    public void IsEnabled_WhenDisabled_ReturnsFalse()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return; // Skip on non-Windows platforms
        }

        _service.Disable();

        _service.IsEnabled().Should().BeFalse();
    }

    public void Dispose()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
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
