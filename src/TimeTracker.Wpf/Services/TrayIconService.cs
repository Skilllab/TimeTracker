using System.Windows;
using H.NotifyIcon;
using Microsoft.Extensions.Logging;
using TimeTracker.Application.TimeTracking;

namespace TimeTracker.Wpf.Services;

/// <summary>
/// Реализация ITrayIconService на H.NotifyIcon.Wpf.
///
/// Создает TaskbarIcon программно — иконка берется из ресурсов
/// приложения (pack://application:,,,/Resources/icon.ico).
///
/// Контекстное меню:
///   - Start/Stop (toggle) — через ITimerService;
///   - Open — показать MainWindow;
///   - Exit — завершить приложение.
///
/// Двойной клик — показать окно.
///
/// Обновляет ToolTipText при изменении состояния таймера.
/// </summary>
public sealed class TrayIconService : ITrayIconService
{
    private readonly ITimerService _timerService;
    private readonly ILogger<TrayIconService> _logger;
    private TaskbarIcon? _icon;

    public TrayIconService(ITimerService timerService, ILogger<TrayIconService> logger)
    {
        _timerService = timerService;
        _logger = logger;
    }

    public void Initialize()
    {
        _icon = new TaskbarIcon
        {
            IconSource = new System.Windows.Media.Imaging.BitmapImage(
                new Uri("pack://application:,,,/Resources/icon.ico")),
            ToolTipText = "TimeTracker",
            ContextMenu = BuildContextMenu()
        };

        _icon.TrayMouseDoubleClick += (_, _) => ShowMainWindow();
        _icon.ForceCreate();

        _timerService.StateChanged += OnStateChanged;

        _logger.LogInformation("Tray icon initialized");
    }

    public void ShowNotification(string title, string message)
    {
        _icon?.ShowNotification(title, message);
    }

    private System.Windows.Controls.ContextMenu BuildContextMenu()
    {
        var menu = new System.Windows.Controls.ContextMenu();

        var toggle = new System.Windows.Controls.MenuItem { Header = "Start/Stop" };
        toggle.Click += async (_, _) =>
        {
            if (_timerService.State == TimerState.Idle)
                await _timerService.StartTimerAsync(null, null);
            else
                await _timerService.StopTimerAsync();
        };

        var open = new System.Windows.Controls.MenuItem { Header = "Open" };
        open.Click += (_, _) => ShowMainWindow();

        var exit = new System.Windows.Controls.MenuItem { Header = "Exit" };
        exit.Click += (_, _) => System.Windows.Application.Current.Shutdown();

        menu.Items.Add(toggle);
        menu.Items.Add(open);
        menu.Items.Add(new System.Windows.Controls.Separator());
        menu.Items.Add(exit);

        return menu;
    }

    private static void ShowMainWindow()
    {
        if (System.Windows.Application.Current.MainWindow is { } window)
        {
            window.Show();
            window.WindowState = WindowState.Normal;
            window.Activate();
        }
    }

    private void OnStateChanged(object? sender, TimerStateChangedEventArgs e)
    {
        if (_icon is null)
            return;

        _icon.ToolTipText = e.NewState == TimerState.Running
            ? "TimeTracker — running"
            : "TimeTracker";
    }

    public void Dispose()
    {
        _timerService.StateChanged -= OnStateChanged;
        _icon?.Dispose();
    }
}
