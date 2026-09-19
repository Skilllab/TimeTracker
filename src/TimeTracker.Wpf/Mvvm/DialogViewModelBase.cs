using CommunityToolkit.Mvvm.Input;

namespace TimeTracker.Wpf.Mvvm;

/// <summary>
/// База для ViewModel модальных диалогов.
///
/// TResult — тип результата, который диалог возвращает при закрытии.
/// Например, для подтверждения удаления — bool, для редактора проекта —
/// ProjectEditorResult.
///
/// Закрытие инициируется через событие RequestClose. Окно диалога
/// подписывается на него и вызывает Close(). Это позволяет ViewModel
/// не знать про Window напрямую.
/// </summary>
public abstract partial class DialogViewModelBase<TResult> : ViewModelBase
{
    /// <summary>
    /// Поднимается, когда ViewModel хочет закрыть диалог.
    /// Окно подписывается на него и вызывает Close().
    /// </summary>
    public event EventHandler? RequestClose;

    /// <summary>
    /// Результат диалога. Устанавливается перед RequestClose.
    /// </summary>
    public TResult? Result
    {
        get; protected set;
    }

    /// <summary>
    /// Отмена диалога. По умолчанию — закрыть с результатом default.
    /// </summary>
    [RelayCommand]
    protected virtual void Cancel() => Close(default);

    /// <summary>
    /// Закрыть диалог с результатом.
    /// </summary>
    protected void Close(TResult? result)
    {
        Result = result;
        RequestClose?.Invoke(this, EventArgs.Empty);
    }
}
