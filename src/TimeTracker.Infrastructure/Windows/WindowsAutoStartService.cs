using System.Diagnostics;
using System.Runtime.Versioning;
using Microsoft.Win32;
using TimeTracker.Application;

namespace TimeTracker.Infrastructure.Windows;

/// <summary>
/// Реализация порта автозапуска через реестр Windows.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class WindowsAutoStartService : IAutoStartService
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "TimeTracker";

    /// <inheritdoc />
    public void Enable()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true);
        if (key is null)
        {
            throw new InvalidOperationException("Не удалось открыть ключ реестра автозапуска.");
        }

        var executablePath = GetExecutablePath();
        key.SetValue(AppName, executablePath);
    }

    /// <inheritdoc />
    public void Disable()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true);
        if (key is null)
        {
            throw new InvalidOperationException("Не удалось открыть ключ реестра автозапуска.");
        }

        key.DeleteValue(AppName, false);
    }

    /// <inheritdoc />
    public bool IsEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, false);
        if (key is null)
        {
            return false;
        }

        var value = key.GetValue(AppName);
        return value is not null;
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
}
