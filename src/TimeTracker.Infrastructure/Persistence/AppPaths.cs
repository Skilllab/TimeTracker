namespace TimeTracker.Infrastructure.Persistence;

/// <summary>
/// Пути приложения. Всё, что пишется на диск, идет в %LocalAppData%\TimeTracker\.
/// Путь одинаков для рантайма и design-time (dotnet ef)
/// </summary>
public static class AppPaths
{
    /// <summary>Корневая папка данных: %LocalAppData%\TimeTracker\</summary>
    public static string AppDataFolder =>
        Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "TimeTracker");

    /// <summary>Путь к файлу базы данных SQLite</summary>
    public static string DatabaseFile => Path.Combine(AppDataFolder, "timetracker.db");

    /// <summary>Папка для логов</summary>
    public static string LogsFolder => Path.Combine(AppDataFolder, "logs");

    /// <summary>
    /// Создать директории, если их нет
    /// </summary>
    public static void EnsureCreated()
    {
        Directory.CreateDirectory(AppDataFolder);
        Directory.CreateDirectory(LogsFolder);
    }
}
