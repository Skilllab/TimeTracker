using Avalonia;
using TimeTracker.Application;
using TimeTracker.Domain;
using TimeTracker.Presentation;
using TimeTracker.Presentation.ViewModels;
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
    /// Создает приложение: сессия, входящий порт и ViewModel собираются в одном месте.
    /// </summary>
    private static App CreateApp()
    {
        var timeProvider = TimeProvider.System;
        var session = new TimerSession();

        ITimerControl timerControl = new TimerControl(session, timeProvider);

        return new App(() => new MainWindow(new MainWindowViewModel(timerControl)));
    }
}
