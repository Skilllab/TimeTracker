using System.Xml.Linq;
using FluentAssertions;
using Xunit;

namespace TimeTracker.Ui.Tests;

public sealed class StringDictionariesTests
{
    [Fact]
    public void RussianAndEnglish_HaveSameKeys()
    {
        var russian = ReadKeys("Strings.ru.axaml");
        var english = ReadKeys("Strings.en.axaml");

        english.Should().BeEquivalentTo(russian);
    }

    [Fact]
    public void EveryDictionary_HasApplicationTitle()
    {
        ReadKeys("Strings.ru.axaml").Should().Contain("App.Title");
        ReadKeys("Strings.en.axaml").Should().Contain("App.Title");
    }

    private static IReadOnlyCollection<string> ReadKeys(string fileName)
    {
        var path = Path.Combine(FindThemesDirectory(), fileName);
        var xaml = (XNamespace)"http://schemas.microsoft.com/winfx/2006/xaml";

        return XDocument.Load(path).Root!
            .Elements()
            .Select(element => element.Attribute(xaml + "Key")?.Value)
            .Where(key => key is not null)
            .Select(key => key!)
            .ToList();
    }

    private static string FindThemesDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "TimeTracker.slnx")))
        {
            directory = directory.Parent;
        }

        return Path.Combine(directory!.FullName, "src", "TimeTracker.Presentation", "Themes");
    }
}
