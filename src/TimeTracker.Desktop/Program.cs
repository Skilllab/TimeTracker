using System.Runtime.InteropServices;
using Avalonia;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Application;
using TimeTracker.Domain;
using TimeTracker.Infrastructure;
using TimeTracker.Infrastructure.Linux;
using TimeTracker.Infrastructure.Mac;
using TimeTracker.Infrastructure.Windows;
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
        using var guard = new SingleInstanceGuard();

        if (!guard.TryAcquire("TimeTracker.SingleInstance"))
        {
            return;
        }

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

        IProjectRepository projectRepository = new ProjectRepository(context);
        IProjectList projectList = new ProjectList(projectRepository);

        var session = new TimerSession();

        ITimerControl timerControl = new TimerControl(session, timeProvider, repository, unitOfWork);
        ITimeEntryList entryList = new TimeEntryList(repository, timeProvider);

        IHotKeyService hotKeyService;
        IIdleDetector idleDetector;

        if (OperatingSystem.IsWindows())
        {
            hotKeyService = new WindowsHotKeyService(timeProvider);
            idleDetector = new WindowsIdleDetector();
        }
        else
        {
            hotKeyService = new NoopHotKeyService();
            idleDetector = new NoopIdleDetector();
        }

        var idleSettings = new IdleSettings();
        var hotKeySettings = new HotKeySettings(hotKeyService);

        var themeManager = new ThemeManager();
        var localizationManager = new LocalizationManager();

        var timerViewModel = new TimerViewModel(timerControl, projectList, localizationManager);
        var entriesViewModel = new EntriesViewModel(entryList, projectList);
        var projectsViewModel = new ProjectsViewModel(projectList);
        var autoStartService = CreateAutoStartService();
        var settingsViewModel = new SettingsViewModel(idleSettings, hotKeySettings, localizationManager, autoStartService);

        var shellViewModel = new MainWindowViewModel(
            timerViewModel,
            entriesViewModel,
            projectsViewModel,
            settingsViewModel,
            themeManager,
            localizationManager);

        timerControl.RestoreAsync().GetAwaiter().GetResult();

        hotKeySettings.RegisterDefault();
        hotKeyService.Pressed += async (_, _) => await timerViewModel.ToggleCommand.ExecuteAsync(null);

        var idleWatcher = new IdleWatcher(
            idleDetector,
            timerControl,
            idleSettings,
            timeProvider,
            TimeSpan.FromSeconds(30));

        idleWatcher.Start();

        var trayPresenter = new TrayPresenter(localizationManager);

        return new App(
            () => new MainWindow(shellViewModel),
            themeManager,
            localizationManager,
            trayPresenter,
            idleWatcher);
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

    private static IAutoStartService CreateAutoStartService()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return new WindowsAutoStartService();
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return new LinuxAutoStartService();
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return new MacAutoStartService();
        }

        throw new PlatformNotSupportedException("Автозапуск не поддерживается на данной платформе.");
    }
}
