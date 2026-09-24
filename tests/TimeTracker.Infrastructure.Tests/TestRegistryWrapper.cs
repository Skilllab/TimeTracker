using TimeTracker.Infrastructure.Windows;

namespace TimeTracker.Infrastructure.Tests;

/// <summary>
/// Заглушка для работы с реестром в тестах.
/// </summary>
public sealed class TestRegistryWrapper : IRegistryWrapper
{
    private readonly Dictionary<string, object?> _registry = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public IRegistryKey? OpenSubKey(string keyPath, bool writable)
    {
        // Для тестов возвращаем фейковый ключ
        return new TestRegistryKey(_registry, keyPath);
    }

    /// <summary>
    /// Устанавливает фейковое значение в реестре.
    /// </summary>
    public void SetValue(string keyPath, string valueName, object? value)
    {
        if (!_registry.ContainsKey(keyPath))
        {
            _registry[keyPath] = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        }

        var key = (Dictionary<string, object?>)_registry[keyPath]!;
        key[valueName] = value;
    }

    /// <summary>
    /// Получает фейковое значение из реестра.
    /// </summary>
    public object? GetValue(string keyPath, string valueName)
    {
        if (_registry.TryGetValue(keyPath, out var keyData))
        {
            var key = (Dictionary<string, object?>)keyData!;
            if (key.TryGetValue(valueName, out var value))
            {
                return value;
            }
        }

        return null;
    }

    /// <summary>
    /// Удаляет фейковое значение из реестра.
    /// </summary>
    public void DeleteValue(string keyPath, string valueName)
    {
        if (_registry.TryGetValue(keyPath, out var keyData))
        {
            var key = (Dictionary<string, object?>)keyData!;
            key.Remove(valueName);
        }
    }

    /// <summary>
    /// Очищает фейковый реестр.
    /// </summary>
    public void Clear()
    {
        _registry.Clear();
    }
}
