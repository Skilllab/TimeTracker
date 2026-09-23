using FluentAssertions;
using TimeTracker.Infrastructure;
using Xunit;

namespace TimeTracker.UI.Tests;

public sealed class InstanceGuardTests
{
    [Fact]
    public void TryAcquire_FirstTime_Succeeds()
    {
        using var guard = new SingleInstanceGuard();
        var name = $"TimeTracker.Tests.{Guid.NewGuid():N}";

        guard.TryAcquire(name).Should().BeTrue();
    }

    [Fact]
    public void TryAcquire_SecondGuardWithSameName_Fails()
    {
        var name = $"TimeTracker.Tests.{Guid.NewGuid():N}";
        using var first = new SingleInstanceGuard();
        using var second = new SingleInstanceGuard();

        first.TryAcquire(name).Should().BeTrue();
        second.TryAcquire(name).Should().BeFalse();
    }
}
