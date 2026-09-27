using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeTracker.Presentation.Views;

namespace TimeTracker.Presentation.ViewModels;

/// <summary>
/// ViewModel оболочки: показывает единственный экран — список задач.
/// Навигации нет, а настройки и отчеты открываются отдельными модальными окнами.
/// </summary>
public sealed partial class MainWindowViewModel : ObservableObject
{
    private readonly SettingsViewModel _settingsViewModel;
    private readonly ReportsViewModel _reportsViewModel;
    private Func<SettingsViewModel, SettingsWindow> _settingsWindowFactory = null!;
    private Func<ReportsViewModel, ReportsWindow> _reportsWindowFactory = null!;
    private Control _owner = null!;

    /// <summary>
    /// Создает оболочку.
    /// </summary>
    /// <param name="tasksViewModel">Экран списка задач.</param>
    /// <param name="settingsViewModel">Модель окна настроек.</param>
    /// <param name="reportsViewModel">Модель окна отчетов.</param>
    public MainWindowViewModel(
        TasksViewModel tasksViewModel,
        SettingsViewModel settingsViewModel,
        ReportsViewModel reportsViewModel)
    {
        Page = tasksViewModel ?? throw new ArgumentNullException(nameof(tasksViewModel));
        _settingsViewModel = settingsViewModel ?? throw new ArgumentNullException(nameof(settingsViewModel));
        _reportsViewModel = reportsViewModel ?? throw new ArgumentNullException(nameof(reportsViewModel));
    }

    /// <summary>
    /// Единственный экран приложения: список задач виден сразу при запуске.
    /// </summary>
    public TasksViewModel Page { get; }

    /// <summary>
    /// Передает модели фабрику окна настроек и владельца окна.
    /// Окно создает код разметки: показ окна — работа представления,
    /// а владелец берется из визуального дерева, поэтому окно открывается модально.
    /// </summary>
    /// <param name="settingsWindowFactory">Фабрика окна настроек.</param>
    /// <param name="reportsWindowFactory">Фабрика окна отчетов.</param>
    /// <param name="owner">Элемент управления, по которому ищется владелец окна.</param>
    public void AttachWindowFactories(
        Func<SettingsViewModel, SettingsWindow> settingsWindowFactory,
        Func<ReportsViewModel, ReportsWindow> reportsWindowFactory,
        Control owner)
    {
        _settingsWindowFactory = settingsWindowFactory ?? throw new ArgumentNullException(nameof(settingsWindowFactory));
        _reportsWindowFactory = reportsWindowFactory ?? throw new ArgumentNullException(nameof(reportsWindowFactory));
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
    }

    /// <summary>
    /// Открывает окно настроек.
    /// После закрытия список задач перечитывается: в настройках правят проекты,
    /// поэтому имена и цвета проектов на плашках могли измениться.
    /// </summary>
    [RelayCommand]
    private async Task OpenSettings()
    {
        var window = _settingsWindowFactory(_settingsViewModel);

        if (TopLevel.GetTopLevel(_owner) is Window owner)
        {
            await window.ShowDialog(owner);
        }
        else
        {
            window.Show();
        }

        await Page.RefreshFiltersAsync();
    }

    /// <summary>
    /// Открывает окно отчетов модально.
    /// </summary>
    [RelayCommand]
    private async Task OpenReports()
    {
        var window = _reportsWindowFactory(_reportsViewModel);

        if (TopLevel.GetTopLevel(_owner) is Window owner)
        {
            await window.ShowDialog(owner);
        }
        else
        {
            window.Show();
        }
    }
}
