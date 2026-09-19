using TimeTracker.Wpf.Mvvm;

namespace TimeTracker.Wpf.Navigation;

/// <summary>
/// Сервис навигации между страницами (ViewModel).
///
/// В этапе 4 ещё не используется — заготовка для этапа 5.
/// Поддерживает переходы вперёд и назад (стек).
///
/// Navigated — событие, на которое подписывается Shell (MainWindow),
/// чтобы переключить отображаемую View.
/// </summary>
public interface INavigationService
{
    /// <summary>Можно ли вернуться назад.</summary>
    bool CanGoBack
    {
        get;
    }

    /// <summary>Поднимается после успешного перехода.</summary>
    event EventHandler<NavigationEventArgs>? Navigated;

    /// <summary>
    /// Перейти к ViewModel типа TViewModel.
    /// </summary>
    /// <param name="parameter">Параметр, передаваемый в OnNavigatedToAsync.</param>
    Task NavigateToAsync<TViewModel>(object? parameter = null) where TViewModel : Mvvm.ViewModelBase;

    /// <summary>Вернуться на предыдущую страницу.</summary>
    Task GoBackAsync();
}
