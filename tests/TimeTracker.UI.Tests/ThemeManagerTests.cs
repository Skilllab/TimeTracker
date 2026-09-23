using FluentAssertions;
using TimeTracker.Presentation.Shell;
using Xunit;

namespace TimeTracker.Ui.Tests;

public sealed class ThemeManagerTests
{
    [Fact]
    public void NewManager_UsesSystemTheme()
    {
        var manager = new ThemeManager();

        manager.Current.Should().Be(AppTheme.System);
    }

    [Fact]
    public void Select_ChangesThemeAndRaisesEvent()
    {
        var manager = new ThemeManager();
        var raised = 0;
        manager.Changed += (_, _) => raised++;

        manager.Select(AppTheme.Dark);

        manager.Current.Should().Be(AppTheme.Dark);
        raised.Should().Be(1);
    }

    [Fact]
    public void Select_SameTheme_DoesNotRaiseEvent()
    {
        var manager = new ThemeManager();
        manager.Select(AppTheme.Light);
        var raised = 0;
        manager.Changed += (_, _) => raised++;

        manager.Select(AppTheme.Light);

        raised.Should().Be(0);
    }

    [Fact]
    public void AvailableThemes_ContainsThreeValues()
    {
        ThemeManager.AvailableThemes.Should().HaveCount(3);
    }
}
