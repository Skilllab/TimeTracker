using TimeTracker.Wpf.Mvvm;

namespace TimeTracker.Wpf.Dialogs;

/// <summary>
/// Сервис диалогов. Показывает MessageBox-подобные окна
/// (информация, ошибка, подтверждение) и кастомные модальные
/// окна с ViewModel.
///
/// Используется в ViewModels вместо прямого MessageBox.Show,
/// что позволяет мокать в тестах.
/// </summary>
public interface IDialogService
{
    /// <summary>Показать информационное сообщение.</summary>
    void ShowInfo(string message, string title = "Info");

    /// <summary>Показать сообщение об ошибке.</summary>
    void ShowError(string message, string title = "Error");

    /// <summary>Запросить подтверждение. Возвращает true при Yes/OK.</summary>
    bool Confirm(string message, string title = "Confirm");

    /// <summary>
    /// Показать кастомный модальный диалог по ViewModel.
    /// Возвращает результат типа TResult? из DialogViewModelBase.
    /// </summary>
    Task<TResult?> ShowDialogAsync<TViewModel, TResult>()
        where TViewModel : DialogViewModelBase<TResult>;
}
