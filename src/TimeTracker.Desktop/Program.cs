using Avalonia;
using TimeTracker.Application;
using TimeTracker.Infrastructure;
using TimeTracker.Presentation;
using TimeTracker.Presentation.Views;

namespace TimeTracker.Desktop;

/// <summary>
/// Точка входа приложения TimeTracker — единственное место, где известны все слои (composition root).
/// </summary>
internal static class Program
{
    /// <summary>
    /// Запускает приложение Avalonia с классическим жизненным циклом настольного приложения.
    /// </summary>
    /// <param name="args">Аргументы командной строки.</param>
    [STAThread]
    public static void Main(string[] args)
    {
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    /// <summary>
    /// Создает и настраивает построитель приложения Avalonia.
    /// Используется также дизайнером XAML — поэтому метод публичный.
    /// </summary>
    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure(CreateApp)
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
    }

    /// <summary>
    /// Создает приложение: порт <see cref="ITimerService"/> получает реализацию из Infrastructure.
    /// </summary>
    private static App CreateApp()
    {
        ITimerService timerService = new InMemoryTimerService(TimeProvider.System);
        return new App(() => new MainWindow(timerService));
    }
}
