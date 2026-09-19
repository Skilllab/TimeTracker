using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using TimeTracker.Wpf.Mvvm;

namespace TimeTracker.Wpf.ViewModels;

/// <summary>
/// ViewModel главного окна.
///
/// В этапе 4 — минимальная заготовка: заголовок и пара команд.
/// В этапе 5 будет содержать TimerPanelViewModel и TodayEntriesViewModel.
/// </summary>
public sealed partial class MainWindowViewModel : ViewModelBase
{
    private readonly ILogger<MainWindowViewModel> _logger;

    public MainWindowViewModel(ILogger<MainWindowViewModel> logger)
    {
        _logger = logger;
        Title = "TimeTracker";
    }

    [RelayCommand]
    private void MinimizeToTray()
    {
        // Заглушка. Реализация — этап 5.
        _logger.LogInformation("MinimizeToTray requested");
    }

    [RelayCommand]
    private void Exit()
    {
        _logger.LogInformation("Exit requested");
        System.Windows.Application.Current.Shutdown();
    }
}
