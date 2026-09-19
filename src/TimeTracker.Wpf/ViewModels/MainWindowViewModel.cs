using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using TimeTracker.Wpf.Mvvm;

namespace TimeTracker.Wpf.ViewModels;

/// <summary>
/// ViewModel главного окна.
///
/// Связывает панель трекинга (TimerPanelViewModel) со списком
/// записей за сегодня (TodayEntriesViewModel).
///
/// При EntryStopped от TimerPanel — перезагружает список.
/// </summary>
public sealed partial class MainWindowViewModel : ViewModelBase
{
    private readonly ILogger<MainWindowViewModel> _logger;

    public TimerPanelViewModel Timer
    {
        get;
    }
    public TodayEntriesViewModel Entries
    {
        get;
    }

    public MainWindowViewModel(
        TimerPanelViewModel timer,
        TodayEntriesViewModel entries,
        ILogger<MainWindowViewModel> logger)
    {
        Timer = timer;
        Entries = entries;
        _logger = logger;

        Title = "TimeTracker";

        // Подписываемся на событие остановки таймера.
        Timer.EntryStopped += async (_, _) => await Entries.ReloadAsync();
    }

    /// <summary>
    /// Инициализация: загрузить проекты и список записей.
    /// Вызывается из MainWindow после Show.
    /// </summary>
    public async Task InitializeAsync()
    {
        await Timer.InitializeAsync();
        await Entries.ReloadAsync();
    }

    [RelayCommand]
    private void MinimizeToTray()
    {
        _logger.LogInformation("MinimizeToTray requested");
        System.Windows.Application.Current.MainWindow.Hide();
    }

    [RelayCommand]
    private void Exit()
    {
        _logger.LogInformation("Exit requested");
        System.Windows.Application.Current.Shutdown();
    }
}
