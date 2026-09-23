using Avalonia;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Application;
using TimeTracker.Domain;
using TimeTracker.Infrastructure;
using TimeTracker.Presentation;
using TimeTracker.Presentation.Shell;
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
    /// Создает приложение: хранилище, порты, экраны и управления собираются в одном месте.
    /// </summary>
    private static App CreateApp()
    {
        var timeProvider = TimeProvider.System;
        var paths = new AppDataPaths();
        var context = CreateContext(paths);
        context.Database.Migrate();

        ITimeEntryRepository repository = new TimeEntryRepository(context);
        IUnitOfWork unitOfWork = new EfUnitOfWork(context);

        var session = new TimerSession();

        ITimerControl timerControl = new TimerControl(session, timeProvider, repository, unitOfWork);
        ITimeEntryList entryList = new TimeEntryList(repository, timeProvider);

        var themeManager = new ThemeManager();
        var localizationManager = new LocalizationManager();

        var timerViewModel = new TimerViewModel(timerControl, localizationManager);
        var entriesViewModel = new EntriesViewModel(entryList);
        var shellViewModel = new MainWindowViewModel(timerViewModel, entriesViewModel, themeManager, localizationManager);

        timerControl.RestoreAsync().GetAwaiter().GetResult();

        return new App(
            () => new MainWindow(shellViewModel),
            themeManager,
            localizationManager);
    }

    /// <summary>
    /// Создает контекст базы данных в папке данных приложения.
    /// </summary>
    /// <param name="paths">Пути к папке данных.</param>
    private static TimeTrackerDbContext CreateContext(IAppDataPaths paths)
    {
        Directory.CreateDirectory(paths.DataDirectory);

        var databasePath = Path.Combine(paths.DataDirectory, "timetracker.db");
        var options = new DbContextOptionsBuilder<TimeTrackerDbContext>()
            .UseSqlite($"Data Source={databasePath}")
            .Options;

        return new TimeTrackerDbContext(options);
    }
}
