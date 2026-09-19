namespace TimeTracker.Wpf.Mvvm;

/// <summary>
/// База для ViewModel, отображаемых на странице (внутри Shell).
///
/// Отличается от ViewModelBase тем, что поддерживает навигацию:
/// OnNavigatedToAsync вызывается, когда ViewModel становится
/// активной, OnNavigatedFromAsync — когда уходит.
///
/// В этапе 5 будет использоваться для TodayEntriesViewModel,
/// ReportsViewModel и других страниц.
/// </summary>
public abstract class PageViewModelBase : ViewModelBase, INavigationAware
{
    /// <summary>
    /// Вызывается при переходе на страницу. Здесь можно загрузить данные.
    /// </summary>
    public virtual Task OnNavigatedToAsync(object? parameter) => Task.CompletedTask;

    /// <summary>
    /// Вызывается при уходе со страницы. Здесь можно отписаться от событий,
    /// сохранить состояние.
    /// </summary>
    public virtual Task OnNavigatedFromAsync() => Task.CompletedTask;
}
