namespace TimeTracker.Wpf.Mvvm;

/// <summary>
/// Реализуется ViewModel, которые хотят получать уведомления
/// о навигации (переход на страницу / уход со страницы).
///
/// NavigationService проверяет, реализует ли ViewModel этот интерфейс,
/// и если да — вызывает соответствующие методы.
/// </summary>
public interface INavigationAware
{
    Task OnNavigatedToAsync(object? parameter);
    Task OnNavigatedFromAsync();
}
