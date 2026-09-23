using FluentAssertions;
using TimeTracker.Presentation.Shell;
using Xunit;

namespace TimeTracker.Ui.Tests;

public sealed class LocalizationManagerTests
{
    [Fact]
    public void NewManager_UsesRussian()
    {
        var manager = new LocalizationManager();

        manager.Language.Should().Be(AppLanguage.Russian);
    }

    [Fact]
    public void Select_ChangesLanguageAndRaisesEvent()
    {
        var manager = new LocalizationManager();
        var raised = 0;
        manager.Changed += (_, _) => raised++;

        manager.Select(AppLanguage.English);

        manager.Language.Should().Be(AppLanguage.English);
        raised.Should().Be(1);
    }

    [Fact]
    public void Select_SameLanguage_DoesNotRaiseEvent()
    {
        var manager = new LocalizationManager();
        var raised = 0;
        manager.Changed += (_, _) => raised++;

        manager.Select(AppLanguage.Russian);

        raised.Should().Be(0);
    }

    [Fact]
    public void Current_ReturnsLatestManager()
    {
        var manager = new LocalizationManager();

        LocalizationManager.Current.Should().BeSameAs(manager);
    }
}
