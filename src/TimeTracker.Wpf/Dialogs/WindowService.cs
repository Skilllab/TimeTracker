using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Wpf.Mvvm;
using TimeTracker.Wpf.Navigation;

namespace TimeTracker.Wpf.Dialogs;

/// <summary>
/// Реализация IWindowService.
///
/// Для создания View диалога использует ViewMapping.GetViewType,
/// затем резолвит View из DI (View зарегистрирован как Transient).
/// Устанавливает DataContext, Owner и возвращает окно.
/// </summary>
public sealed class WindowService(IServiceProvider serviceProvider) : IWindowService
{
    public Window CreateDialogWindow<TResult>(DialogViewModelBase<TResult> viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        var viewType = ViewMapping.GetViewType(viewModel.GetType())
                       ?? throw new InvalidOperationException(
                           $"No view registered for {viewModel.GetType().Name}.");

        var window = (Window)serviceProvider.GetRequiredService(viewType);
        window.DataContext = viewModel;
        window.Owner = System.Windows.Application.Current.MainWindow;

        return window;
    }
}
