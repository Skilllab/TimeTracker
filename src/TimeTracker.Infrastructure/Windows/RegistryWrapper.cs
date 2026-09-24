using System.Runtime.Versioning;
using Microsoft.Win32;

namespace TimeTracker.Infrastructure.Windows;

/// <summary>
/// Обертка для реального ключа реестра.
/// </summary>
[SupportedOSPlatform("windows")]
internal sealed class RealRegistryKey : IRegistryKey
{
    private readonly RegistryKey _key;

    public RealRegistryKey(RegistryKey key)
    {
        _key = key;
    }

    public void SetValue(string name, object value) => _key.SetValue(name, value);
    public object? GetValue(string name) => _key.GetValue(name);
    public void DeleteValue(string name) => _key.DeleteValue(name, false);
    public void Dispose() => _key.Dispose();
}

/// <summary>
/// Реализация абстракции для работы с реестром Windows.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class RegistryWrapper : IRegistryWrapper
{
    /// <inheritdoc />
    public IRegistryKey? OpenSubKey(string keyPath, bool writable)
    {
        var key = Registry.CurrentUser.OpenSubKey(keyPath, writable);
        return key is null ? null : new RealRegistryKey(key);
    }
}
