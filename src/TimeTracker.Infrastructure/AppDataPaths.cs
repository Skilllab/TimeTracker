using TimeTracker.Application;

namespace TimeTracker.Infrastructure;

/// <summary>
/// Кроссплатформенная папка данных приложения.
/// </summary>
public sealed class AppDataPaths : IAppDataPaths
{
    /// <summary>
    /// Папка данных приложения.
    /// </summary>
    public string DataDirectory { get; } = ResolveDataDirectory();

    /// <summary>
    /// Выбирает папку данных по правилам операционной системы.
    /// </summary>
    private static string ResolveDataDirectory()
    {
        if (OperatingSystem.IsWindows())
        {
            var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(local, "TimeTracker");
        }

        if (OperatingSystem.IsMacOS())
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(home, "Library", "Application Support", "TimeTracker");
        }

        var xdg = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
        var root = string.IsNullOrWhiteSpace(xdg)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share")
            : xdg;

        return Path.Combine(root, "TimeTracker");
    }
}
