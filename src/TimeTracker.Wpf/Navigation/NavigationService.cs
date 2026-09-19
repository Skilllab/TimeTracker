using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Wpf.Mvvm;

namespace TimeTracker.Wpf.Navigation;

/// <summary>
/// Реализация INavigationService.
///
/// Хранит стек предыдущих ViewModel. GoBackAsync выталкивает
/// верхнюю и поднимает Navigated с ней. Вперёд — резолвит ViewModel
/// из DI, кладёт текущую в стек, вызывает OnNavigatedToAsync,
/// поднимает Navigated.
///
/// Пока ViewModel не создаёт View — этим занимается Shell
/// (MainWindow), который подписан на Navigated и сам рендерит View
/// через DataTemplate.
/// </summary>
public sealed class NavigationService(IServiceProvider serviceProvider) : INavigationService
{
    private readonly Stack<Mvvm.ViewModelBase> _history = new();
    private Mvvm.ViewModelBase? _current;

    public bool CanGoBack => _history.Count > 0;

    public event EventHandler<NavigationEventArgs>? Navigated;

    public async Task NavigateToAsync<TViewModel>(object? parameter = null)
        where TViewModel : Mvvm.ViewModelBase
    {
        var viewModel = serviceProvider.GetRequiredService<TViewModel>();

        if (_current is INavigationAware currentAware)
            await currentAware.OnNavigatedFromAsync();

        if (_current is not null)
            _history.Push(_current);

        _current = viewModel;

        if (viewModel is INavigationAware aware)
            await aware.OnNavigatedToAsync(parameter);

        Navigated?.Invoke(this, new NavigationEventArgs(viewModel, parameter));
    }

    public async Task GoBackAsync()
    {
        if (_history.Count == 0)
            return;

        if (_current is INavigationAware currentAware)
            await currentAware.OnNavigatedFromAsync();

        var previous = _history.Pop();

        if (previous is INavigationAware aware)
            await aware.OnNavigatedToAsync(null);

        _current = previous;
        Navigated?.Invoke(this, new NavigationEventArgs(previous, null));
    }
}
