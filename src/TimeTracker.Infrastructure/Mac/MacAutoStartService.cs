using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Runtime.Versioning;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using TimeTracker.Application;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TimeTracker.Infrastructure.Mac;

/// <summary>
/// Реализация порта автозапуска через LaunchAgent в macOS.
/// </summary>
/// <remarks>Не тестировался, негде</remarks>
public sealed class MacAutoStartService : IAutoStartService
{
    private const string BundleId = "com.timetracker";
    private const string PlistFileName = $"{BundleId}.plist";

    /// <inheritdoc />
    public void Enable()
    {
        var launchAgentsDir = GetLaunchAgentsDirectory();
        Directory.CreateDirectory(launchAgentsDir);

        var plistFile = Path.Combine(launchAgentsDir, PlistFileName);
        var executablePath = GetExecutablePath();

        var content = GeneratePlist(executablePath);
        File.WriteAllText(plistFile, content);
    }

    /// <inheritdoc />
    public void Disable()
    {
        var launchAgentsDir = GetLaunchAgentsDirectory();
        var plistFile = Path.Combine(launchAgentsDir, PlistFileName);

        if (File.Exists(plistFile))
        {
            File.Delete(plistFile);
        }
    }

    /// <inheritdoc />
    public bool IsEnabled()
    {
        var launchAgentsDir = GetLaunchAgentsDirectory();
        var plistFile = Path.Combine(launchAgentsDir, PlistFileName);
        return File.Exists(plistFile);
    }

    private static string GetLaunchAgentsDirectory()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(home, "Library", "LaunchAgents");
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

    private static string GeneratePlist(string executablePath)
    {
        return "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
               "<!DOCTYPE plist PUBLIC \"-//Apple//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\">" +
               "<plist version=\"1.0\">" +
               "<dict>" +
               "    <key>Label</key>" +
               "    <string>{BundleId}</string>" +
               "    <key>ProgramArguments</key>" +
               "    <array>" +
               "		<string>{executablePath}</string>" +
               "    </array>" +
               "    <key>RunAtLoad</key>" +
               "    <true/>" +
               "</dict>" +
               "</plist>";
    }
}
