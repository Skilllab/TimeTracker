using FluentAssertions;
using TimeTracker.Application;
using Xunit;

namespace TimeTracker.Domain.Tests;

public sealed class IdleSettingsTests
{
    [Fact]
    public void NewSettings_UsesFiveMinutes()
    {
        var settings = new IdleSettings();

        settings.Threshold.Should().Be(TimeSpan.FromMinutes(5));
    }

    [Fact]
    public void SetThreshold_ChangesValueAndRaisesEvent()
    {
        var settings = new IdleSettings();
        var raised = 0;
        settings.Changed += (_, _) => raised++;

        settings.SetThreshold(TimeSpan.FromMinutes(10));

        settings.Threshold.Should().Be(TimeSpan.FromMinutes(10));
        raised.Should().Be(1);
    }

    [Fact]
    public void SetThreshold_SameValue_DoesNotRaiseEvent()
    {
        var settings = new IdleSettings();
        var raised = 0;
        settings.Changed += (_, _) => raised++;

        settings.SetThreshold(IdleSettings.DefaultThreshold);

        raised.Should().Be(0);
    }

    [Fact]
    public void SetThreshold_NotPositive_Throws()
    {
        var settings = new IdleSettings();

        var act = () => settings.SetThreshold(TimeSpan.Zero);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
