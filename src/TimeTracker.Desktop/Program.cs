using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Application;
using TimeTracker.Application.ReportExport;
using TimeTracker.Application.Reports;
using TimeTracker.Infrastructure;
using TimeTracker.Infrastructure.Linux;
using TimeTracker.Infrastructure.Mac;
using TimeTracker.Infrastructure.Windows;
using TimeTracker.Presentation;
using TimeTracker.Presentation.Shell;
using TimeTracker.Presentation.ViewModels;
using TimeTracker.Presentation.Views;
using Velopack;
using Velopack.Sources;

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

        VelopackApp.Build().Run();

        ApplyUpdatesIfAvailable();

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    /// <summary>
    /// Адрес репозитория: из него установленное приложение берет обновления.
    /// </summary>
    private const string RepositoryUrl = "https://github.com/Skilllab/TimeTracker";

    /// <summary>
    /// Проверяет обновления и запускает установку найденной версии.
    /// Проверка выполняется только для установленного приложения: запуск из исходников
    /// и из конвейера сборки не обновляется, поэтому там проверка пропускается.
    /// Ошибка проверки не прерывает запуск: без сети приложение должно открываться.
    /// Обновление заменяет только файлы приложения: база и настройки лежат в папке данных,
    /// поэтому установка новой версии их не затрагивает.
    /// </summary>
    private static void ApplyUpdatesIfAvailable()
    {
        try
        {
            var updateManager = new UpdateManager(new GithubSource(RepositoryUrl, null, false));

            if (!updateManager.IsInstalled)
            {
                return;
            }

            var updateInfo = updateManager.CheckForUpdatesAsync().GetAwaiter().GetResult();

            if (updateInfo is null)
            {
                return;
            }

            updateManager.DownloadUpdatesAsync(updateInfo).GetAwaiter().GetResult();

            updateManager.ApplyUpdatesAndRestart(updateInfo);
        }
        catch (Exception exception)
        {
            Trace.TraceError(exception.ToString());
        }
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
    /// Задачи и их записи работают через один контекст базы данных,
    /// поэтому задача и сегмент ее работы фиксируются одной операцией сохранения.
    /// </summary>
    private static App CreateApp()
    {
        var timeProvider = TimeProvider.System;
        var paths = new AppDataPaths();
        var context = CreateContext(paths);
        context.Database.Migrate();

        ITimeEntryRepository entryRepository = new TimeEntryRepository(context);
        IWorkTaskRepository taskRepository = new EfWorkTaskRepository(context);
        IUnitOfWork unitOfWork = new EfUnitOfWork(context);

        IProjectRepository projectRepository = new ProjectRepository(context);
        IProjectList projectList = new ProjectList(projectRepository, entryRepository);
        IProjectEditor projectEditor = new ProjectEditor(projectRepository, unitOfWork);

        ITaskControl taskControl = new TaskControl(taskRepository, entryRepository, unitOfWork, timeProvider);
        ITaskList taskList = new TaskList(taskRepository, projectRepository, timeProvider);

        IReportReader reportReader = new EfReportReader(context, timeProvider);
        IReportService reportService = new ReportService(reportReader);
        IReportExporter reportExporter = new CsvReportExporter();
        ITaskTagSuggestions tagSuggestions = new EfTaskTagSuggestions(context);

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

        var tasksViewModel = new TasksViewModel(taskList, taskControl, projectList, localizationManager, tagSuggestions);
        var projectsViewModel = new ProjectsViewModel(projectList, projectEditor);
        var autoStartService = CreateAutoStartService();
        var settingsViewModel = new SettingsViewModel(
            idleSettings,
            hotKeySettings,
            localizationManager,
            autoStartService,
            themeManager,
            projectsViewModel);

        var reportsViewModel = new ReportsViewModel(reportService, reportExporter, timeProvider);
        var shellViewModel = new MainWindowViewModel(tasksViewModel, settingsViewModel, reportsViewModel);

        taskControl.RestoreAsync().GetAwaiter().GetResult();

        hotKeySettings.RegisterDefault();

        // Горячая клавиша приостанавливает идущую задачу: какой именно задачей управлять,
        // решает список, а не сочетание клавиш.
        hotKeyService.Pressed += async (_, _) =>
        {
            await taskControl.PauseRunningAsync();
            await tasksViewModel.RefreshAsync();
        };

        var idleWatcher = new IdleWatcher(
            idleDetector,
            taskControl,
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
