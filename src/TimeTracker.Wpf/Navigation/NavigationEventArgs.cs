using TimeTracker.Wpf.Mvvm;

namespace TimeTracker.Wpf.Navigation;

/// <summary>
/// Аргументы события Navigated. Поднимается после успешного перехода.
/// </summary>
public sealed class NavigationEventArgs(ViewModelBase viewModel, object? parameter) : EventArgs
{
    public ViewModelBase ViewModel { get; } = viewModel;
    public object? Parameter { get; } = parameter;
}
