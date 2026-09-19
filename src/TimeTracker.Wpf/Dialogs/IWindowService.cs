using System.Windows;
using TimeTracker.Wpf.Mvvm;

namespace TimeTracker.Wpf.Dialogs;

/// <summary>
/// Сервис создания окон. Отделяет ViewModel от прямого new Window().
///
/// CreateDialogWindow находит View для DialogViewModelBase по
/// ViewMapping, настраивает Owner и возвращает готовое к ShowDialog()
/// окно.
/// </summary>
public interface IWindowService
{
    /// <summary>
    /// Создать модальное окно для указанной ViewModel диалога.
    /// </summary>
    Window CreateDialogWindow<TResult>(DialogViewModelBase<TResult> viewModel);
}
