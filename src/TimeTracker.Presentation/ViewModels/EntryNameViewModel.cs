using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TimeTracker.Presentation.ViewModels;

/// <summary>
/// ViewModel окна имени задачи: вводит имя и сообщает о подтверждении.
/// </summary>
public sealed partial class EntryNameViewModel : ObservableObject
{
    /// <summary>
    /// Создает окно имени задачи.
    /// </summary>
    /// <param name="title">Заголовок окна.</param>
    /// <param name="name">Исходное имя задачи.</param>
    public EntryNameViewModel(string title, string name)
    {
        Title = title;
        Name = name;
        ConfirmCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// Событие запроса закрытия окна.
    /// </summary>
    public event EventHandler? CloseRequested;

    /// <summary>
    /// Заголовок окна: ввод или правка.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Признак того, что имя подтверждено.
    /// </summary>
    public bool IsConfirmed { get; private set; }

    /// <summary>
    /// Введенное имя задачи.
    /// </summary>
    [ObservableProperty]
    private string _name = string.Empty;

    partial void OnNameChanged(string value)
    {
        ConfirmCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// Возвращает введенное имя без крайних пробелов.
    /// </summary>
    public string TrimmedName => (Name ?? string.Empty).Trim();

    /// <summary>
    /// Подтверждает введенное имя и сообщает о закрытии окна.
    /// Без запроса закрытия вызывающий код не получил бы управление:
    /// показ окна завершается только после его закрытия.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanConfirm))]
    private void Confirm()
    {
        IsConfirmed = true;

        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Разрешает подтверждение, если имя не пустое после обрезки пробелов.
    /// </summary>
    private bool CanConfirm() => !string.IsNullOrWhiteSpace(Name?.Trim());

    /// <summary>
    /// Закрывает окно без сохранения.
    /// </summary>
    [RelayCommand]
    private void Cancel() => CloseRequested?.Invoke(this, EventArgs.Empty);
}
