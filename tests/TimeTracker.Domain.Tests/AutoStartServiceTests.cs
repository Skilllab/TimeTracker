using FluentAssertions;
using TimeTracker.Application;
using Xunit;

namespace TimeTracker.Domain.Tests;

public sealed class AutoStartServiceTests
{
    private sealed class FakeAutoStartService : IAutoStartService
    {
        public bool Enabled { get; private set; }

        public void Enable() => Enabled = true;
        public void Disable() => Enabled = false;
        public bool IsEnabled() => Enabled;
    }

    [Fact]
    public void Enable_SetsEnabledToTrue()
    {
        var service = new FakeAutoStartService();

        service.Enable();

        service.IsEnabled().Should().BeTrue();
    }

    [Fact]
    public void Disable_SetsEnabledToFalse()
    {
        var service = new FakeAutoStartService();
        service.Enable();

        service.Disable();

        service.IsEnabled().Should().BeFalse();
    }

    [Fact]
    public void IsEnabled_ReturnsCurrentState()
    {
        var service = new FakeAutoStartService();

        service.IsEnabled().Should().BeFalse();

        service.Enable();
        service.IsEnabled().Should().BeTrue();

        service.Disable();
        service.IsEnabled().Should().BeFalse();
    }

    [Fact]
    public void Enable_Twice_DoesNotThrow()
    {
        var service = new FakeAutoStartService();

        service.Enable();
        service.Enable(); // Should not throw
    }

    [Fact]
    public void Disable_WhenDisabled_DoesNotThrow()
    {
        var service = new FakeAutoStartService();

        service.Disable(); // Should not throw
    }
}
