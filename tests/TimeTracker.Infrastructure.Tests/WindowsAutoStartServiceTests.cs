using FluentAssertions;
using TimeTracker.Infrastructure.Windows;
using Xunit;

namespace TimeTracker.Infrastructure.Tests;

public sealed class WindowsAutoStartServiceTests : IDisposable
{
    private const string TestAppName = "TimeTrackerTest";
    private readonly TestRegistryWrapper _testRegistry;
    private readonly WindowsAutoStartService _service;

    public WindowsAutoStartServiceTests()
    {
        _testRegistry = new TestRegistryWrapper();
        _service = new WindowsAutoStartService(_testRegistry);
    }

    [Fact]
    public void Enable_CreatesRegistryEntry()
    {
        _service.Enable();

        _service.IsEnabled().Should().BeTrue();
    }

    [Fact]
    public void Disable_RemovesRegistryEntry()
    {
        _service.Enable();
        _service.Disable();

        _service.IsEnabled().Should().BeFalse();
    }

    [Fact]
    public void IsEnabled_WhenDisabled_ReturnsFalse()
    {
        _service.Disable();

        _service.IsEnabled().Should().BeFalse();
    }

    public void Dispose()
    {
        _testRegistry.Clear();
    }
}
