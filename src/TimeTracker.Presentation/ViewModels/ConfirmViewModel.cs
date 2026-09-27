using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TimeTracker.Presentation.ViewModels;

/// <summary>
/// ViewModel окна подтверждения: запрашивает согласие перед необратимым действием.
/// </summary>
public sealed partial class ConfirmViewModel : ObservableObject
{
    /// <summary>
    /// Создает окно подтверждения.
    /// </summary>
    /// <param name="title">Заголовок окна.</param>
    /// <param name="question">Текст вопроса.</param>
    public ConfirmViewModel(string title, string question)
    {
        Title = title;
        Question = question;
    }

    /// <summary>
    /// Событие запроса закрытия окна.
    /// </summary>
    public event EventHandler? CloseRequested;

    /// <summary>
    /// Заголовок окна.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Текст вопроса.
    /// </summary>
    public string Question { get; }

    /// <summary>
    /// Признак того, что действие подтверждено.
    /// </summary>
    public bool IsConfirmed { get; private set; }

    /// <summary>
    /// Подтверждает действие и сообщает о закрытии окна.
    /// </summary>
    [RelayCommand]
    private void Confirm()
    {
        IsConfirmed = true;

        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Закрывает окно без подтверждения.
    /// </summary>
    [RelayCommand]
    private void Cancel() => CloseRequested?.Invoke(this, EventArgs.Empty);
}
