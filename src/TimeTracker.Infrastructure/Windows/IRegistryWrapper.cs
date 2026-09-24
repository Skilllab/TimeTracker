namespace TimeTracker.Infrastructure.Windows;

/// <summary>
/// Абстракция для ключа реестра.
/// </summary>
public interface IRegistryKey : IDisposable
{
    /// <summary>
    /// Устанавливает значение в ключе реестра.
    /// </summary>
    void SetValue(string name, object value);

    /// <summary>
    /// Получает значение из ключа реестра.
    /// </summary>
    object? GetValue(string name);

    /// <summary>
    /// Удаляет значение из ключа реестра.
    /// </summary>
    void DeleteValue(string name);
}

/// <summary>
/// Абстракция для работы с реестром Windows.
/// </summary>
public interface IRegistryWrapper
{
    /// <summary>
    /// Открывает ключ реестра для чтения или записи.
    /// </summary>
    /// <param name="keyPath">Путь к ключу реестра.</param>
    /// <param name="writable">true для записи, false для чтения.</param>
    /// <returns>Открытый ключ или null, если ключ не существует.</returns>
    IRegistryKey? OpenSubKey(string keyPath, bool writable);
}
