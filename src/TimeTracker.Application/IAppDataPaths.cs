namespace TimeTracker.Application;

/// <summary>
/// Исходящий порт: пути к папке данных приложения.
/// </summary>
public interface IAppDataPaths
{
    /// <summary>
    /// Папка данных приложения.
    /// </summary>
    string DataDirectory { get; }
}
