using Avalonia;

namespace TimeTracker.Desktop;

/// <summary>
/// Точка входа приложения TimeTracker.
/// </summary>
internal static class Program
{
    /// <summary>
    /// Запускает приложение Avalonia с классическим жизненным циклом настольного приложения
    /// </summary>
    /// <param name="args">Аргументы командной строки</param>
    [STAThread]
    public static void Main(string[] args)
    {
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    /// <summary>
    /// Создает и настраивает билдер приложения Avalonia.
    /// </summary>
    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
    }
}
