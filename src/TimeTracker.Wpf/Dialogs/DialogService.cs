using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Wpf.Mvvm;
using TimeTracker.Wpf.Navigation;

namespace TimeTracker.Wpf.Dialogs;

/// <summary>
/// Реализация IDialogService.
///
/// Для MessageBox-подобных диалогов использует стандартный MessageBox.
/// Для кастомных — резолвит ViewModel из DI, находит View через
/// ViewMapping, создаёт окно, подписывается на RequestClose,
/// показывает ShowDialog() и возвращает Result.
///
/// IWindowService используется для создания окна, чтобы не плодить
/// new Window() в сервисе.
/// </summary>
public sealed class DialogService(IServiceProvider serviceProvider, IWindowService windowService)
    : IDialogService
{
    public void ShowInfo(string message, string title = "Info") =>
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);

    public void ShowError(string message, string title = "Error") =>
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);

    public bool Confirm(string message, string title = "Confirm") =>
        MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question)
        == MessageBoxResult.Yes;

    public async Task<TResult?> ShowDialogAsync<TViewModel, TResult>()
        where TViewModel : DialogViewModelBase<TResult>
    {
        var viewModel = serviceProvider.GetRequiredService<TViewModel>();
        var window = windowService.CreateDialogWindow(viewModel);

        var tcs = new TaskCompletionSource<TResult?>();
        viewModel.RequestClose += (_, _) =>
        {
            tcs.TrySetResult(viewModel.Result);
            window.Close();
        };

        window.ShowDialog();
        return await tcs.Task;
    }
}
