using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using TimeTracker.Application.TimeTracking;
using TimeTracker.Infrastructure;
using TimeTracker.Infrastructure.Persistence;
using TimeTracker.Wpf.Bootstrapping;
using TimeTracker.Wpf.Views;

namespace TimeTracker.Wpf;

/// <summary>
/// Application-класс WPF.
///
/// Использует Microsoft.Extensions.Hosting для DI и конфигурации.
/// Создаёт IHost в OnStartup, регистрирует сервисы, создаёт
/// долгоживущий scope для scoped-сервисов, показывает MainWindow.
///
/// Глобальные обработчики исключений подписаны на три события:
///   - DispatcherUnhandledException — исключения в UI-потоке;
///   - AppDomain.UnhandledException — исключения в фоновых потоках;
///   - TaskScheduler.UnobservedTaskException — необработанные
///     исключения в Task.
///
/// Все три логируются через Serilog и (для UI) показывают MessageBox.
///
/// Lifetime:
///   - _host создаётся в OnStartup, dispose в OnExit.
///   - _rootScope создаётся в OnStartup, dispose в OnExit.
///     Один scope на всё приложение — все scoped-сервисы живут в нём.
///     Это стандартный паттерн для desktop-приложений.
/// </summary>
public partial class App : System.Windows.Application
{
    private IHost? _host;
    private IServiceScope? _rootScope;

    /// <summary>
    /// Провайдер сервисов для ViewModelAutoWireBehavior.
    /// Резолвится из корневого scope.
    /// </summary>
    public IServiceProvider? Services => _rootScope?.ServiceProvider;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var logPath = Path.Combine(AppPaths.LogsFolder, "timetracker-.log");
        _host = Host.CreateDefaultBuilder()
            .UseSerilog((context, services, configuration) =>
            {
                configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext()
                    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
                    .WriteTo.File(
                        logPath,
                        formatProvider: CultureInfo.InvariantCulture,
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 14);
            })
            .ConfigureServices((context, services) =>
            {
                services.AddInfrastructure();
                services.AddWpf();
            })
            .Build();

        RegisterGlobalExceptionHandlers(_host.Services);

        await _host.StartAsync();

        // Один долгоживущий scope для всего приложения.
        _rootScope = _host.Services.CreateScope();

        // Восстанавливаем running-сессию, если есть.
        var timerService = _rootScope.ServiceProvider.GetRequiredService<ITimerService>();
        await timerService.RestoreTimerAsync();

        // Показываем главное окно.
        var mainWindow = _rootScope.ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        _rootScope?.Dispose();

        if (_host is not null)
        {
            await _host.StopAsync(TimeSpan.FromSeconds(5));
            _host.Dispose();
        }

        Log.CloseAndFlush();

        base.OnExit(e);
    }

    /// <summary>
    /// Три хендлера ловят исключения из разных источников:
    ///   - UI thread — DispatcherUnhandledException;
    ///   - фоновые потоки — AppDomain.UnhandledException;
    ///   - Task без await — TaskScheduler.UnobservedTaskException.
    ///
    /// В CI/тестах логи не нужны — там просто не будет исключений.
    /// </summary>
    private void RegisterGlobalExceptionHandlers(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILogger<App>>();

        DispatcherUnhandledException += (_, args) =>
        {
            logger.LogError(args.Exception, "Unhandled UI exception");
            MessageBox.Show(
                args.Exception.Message,
                "Unhandled error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            args.Handled = true;
        };

        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            logger.LogCritical(
                args.ExceptionObject as Exception,
                "Unhandled AppDomain exception (IsTerminating={IsTerminating})",
                args.IsTerminating);
        };

        TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            logger.LogError(args.Exception, "Unobserved task exception");
            args.SetObserved();
        };
    }
}
