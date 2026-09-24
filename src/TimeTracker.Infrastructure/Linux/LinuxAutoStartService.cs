using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using TimeTracker.Application;

namespace TimeTracker.Infrastructure.Linux;

/// <summary>
/// Реализация порта автозапуска через .desktop файл в Linux. 
/// </summary>
/// <remarks>Не тестировался, негде</remarks>
public sealed class LinuxAutoStartService : IAutoStartService
{
    private const string AppName = "timetracker";
    private const string DesktopFileName = $"{AppName}.desktop";

    /// <inheritdoc />
    public void Enable()
    {
        var autostartDir = GetAutostartDirectory();
        Directory.CreateDirectory(autostartDir);

        var desktopFile = Path.Combine(autostartDir, DesktopFileName);
        var executablePath = GetExecutablePath();

        var content = GenerateDesktopFile(executablePath);
        File.WriteAllText(desktopFile, content);
    }

    /// <inheritdoc />
    public void Disable()
    {
        var autostartDir = GetAutostartDirectory();
        var desktopFile = Path.Combine(autostartDir, DesktopFileName);

        if (File.Exists(desktopFile))
        {
            File.Delete(desktopFile);
        }
    }

    /// <inheritdoc />
    public bool IsEnabled()
    {
        var autostartDir = GetAutostartDirectory();
        var desktopFile = Path.Combine(autostartDir, DesktopFileName);
        return File.Exists(desktopFile);
    }

    private static string GetAutostartDirectory()
    {
        // XDG_CONFIG_HOME/autostart или ~/.config/autostart
        var xdgConfigHome = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
        if (!string.IsNullOrEmpty(xdgConfigHome))
        {
            return Path.Combine(xdgConfigHome, "autostart");
        }

        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(home, ".config", "autostart");
    }

    private static string GetExecutablePath()
    {
        var process = Process.GetCurrentProcess();
        var mainModule = process.MainModule;
        if (mainModule is null)
        {
            throw new InvalidOperationException("Не удалось получить путь к исполняемому файлу.");
        }

        return mainModule.FileName;
    }

    private static string GenerateDesktopFile(string executablePath)
    {
        return $"""
[Desktop Entry]
Type=Application
Name=TimeTracker
Comment=Time Tracker Application
Exec={executablePath}
Icon=timetracker
Terminal=false
Categories=Utility;
X-GNOME-Autostart-enabled=true
""";
    }
}
