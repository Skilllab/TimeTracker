using TimeTracker.Infrastructure.Windows;

namespace TimeTracker.Infrastructure.Tests;

/// <summary>
/// Фейковый ключ реестра для тестов.
/// </summary>
public sealed class TestRegistryKey : IRegistryKey
{
    private readonly Dictionary<string, object?> _registry;
    private readonly string _keyPath;

    /// <summary>
    /// Создает фейковый ключ реестра.
    /// </summary>
    /// <param name="registry">Словарь реестра.</param>
    /// <param name="keyPath">Путь к ключу.</param>
    public TestRegistryKey(Dictionary<string, object?> registry, string keyPath)
    {
        _registry = registry;
        _keyPath = keyPath;
    }

    /// <summary>
    /// Устанавливает значение в фейковом реестре.
    /// </summary>
    public void SetValue(string name, object value)
    {
        if (!_registry.ContainsKey(_keyPath))
        {
            _registry[_keyPath] = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        }

        var key = (Dictionary<string, object?>)_registry[_keyPath]!;
        key[name] = value;
    }

    /// <summary>
    /// Получает значение из фейкового реестра.
    /// </summary>
    public object? GetValue(string name)
    {
        if (_registry.TryGetValue(_keyPath, out var keyData))
        {
            var key = (Dictionary<string, object?>)keyData!;
            if (key.TryGetValue(name, out var value))
            {
                return value;
            }
        }

        return null;
    }

    /// <summary>
    /// Удаляет значение из фейкового реестра.
    /// </summary>
    public void DeleteValue(string name)
    {
        if (_registry.TryGetValue(_keyPath, out var keyData))
        {
            var key = (Dictionary<string, object?>)keyData!;
            key.Remove(name);
        }
    }

    /// <summary>
    /// Закрывает фейковый ключ (ничего не делает).
    /// </summary>
    public void Dispose()
    {
        // Ничего не делаем для тестов
    }
}
