using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeTracker.Domain;
using TimeTracker.Presentation.Shell;

namespace TimeTracker.Presentation.ViewModels;

/// <summary>
/// ViewModel окна выбора проекта задачи: показывает доступные проекты
/// и сообщает, какой проект выбран или что задача остается без проекта.
/// </summary>
public sealed partial class ProjectPickerViewModel : ObservableObject
{
    private readonly Func<Task<bool>> _canConfirmAsync;
    private readonly string _blockedMessage;

    /// <summary>
    /// Создает окно выбора проекта.
    /// </summary>
    /// <param name="title">Заголовок окна.</param>
    /// <param name="projects">Проекты, доступные для выбора.</param>
    /// <param name="selectedProjectId">Идентификатор текущего проекта; <c>null</c>, если проекта нет.</param>
    /// <param name="canConfirmAsync">Проверка перед сохранением: пока окно открыто, состояние задачи могло измениться.</param>
    /// <param name="blockedMessage">Сообщение о запрете сохранения.</param>
    public ProjectPickerViewModel(
        string title,
        IReadOnlyList<Project> projects,
        Guid? selectedProjectId,
        Func<Task<bool>> canConfirmAsync,
        string blockedMessage)
    {
        Title = title;
        _canConfirmAsync = canConfirmAsync ?? throw new ArgumentNullException(nameof(canConfirmAsync));
        _blockedMessage = blockedMessage;

        Projects = new ObservableCollection<Project>(projects ?? throw new ArgumentNullException(nameof(projects)));

        SelectedProject = selectedProjectId is Guid id
            ? Projects.FirstOrDefault(project => project.Id == id)
            : null;
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
    /// Проекты, доступные для выбора: архивные сюда не попадают.
    /// </summary>
    public ObservableCollection<Project> Projects { get; }

    /// <summary>
    /// Выбранный проект; <c>null</c>, если задача остается без проекта.
    /// </summary>
    [ObservableProperty]
    private Project? _selectedProject;

    /// <summary>
    /// Сообщение о запрете сохранения; пустая строка, если запрета нет.
    /// </summary>
    [ObservableProperty]
    private string _errorText = string.Empty;

    /// <summary>
    /// Признак того, что сохранение запрещено и показано сообщение.
    /// </summary>
    public bool HasError => !string.IsNullOrEmpty(ErrorText);

    /// <summary>
    /// Признак того, что выбор подтвержден.
    /// </summary>
    public bool IsConfirmed { get; private set; }

    /// <summary>
    /// Идентификатор выбранного проекта; <c>null</c>, если задача остается без проекта.
    /// </summary>
    public Guid? SelectedProjectId => SelectedProject?.Id;

    /// <summary>
    /// Надпись о текущем выборе: имя проекта или подпись об отсутствии проекта.
    /// </summary>
    public string SelectedText
        => SelectedProject?.Name ?? LocalizationManager.Current?["Tasks.WithoutProject"] ?? string.Empty;

    /// <summary>
    /// Обновляет надпись о текущем выборе и снимает прежнее сообщение о запрете.
    /// </summary>
    /// <param name="value">Выбранный проект; <c>null</c>, если проекта нет.</param>
    partial void OnSelectedProjectChanged(Project? value)
    {
        ErrorText = string.Empty;

        OnPropertyChanged(nameof(SelectedText));
    }

    /// <summary>
    /// Обновляет признак показа сообщения о запрете.
    /// </summary>
    /// <param name="value">Текст сообщения.</param>
    partial void OnErrorTextChanged(string value) => OnPropertyChanged(nameof(HasError));

    /// <summary>
    /// Снимает выбор проекта: задача остается без проекта.
    /// </summary>
    [RelayCommand]
    private void ClearProject() => SelectedProject = null;

    /// <summary>
    /// Подтверждает выбор и сообщает о закрытии окна.
    /// Перед сохранением состояние задачи проверяется заново: у выполняемой задачи проект менять нельзя.
    /// Без запроса закрытия вызывающий код не получил бы управление:
    /// показ окна завершается только после его закрытия.
    /// </summary>
    [RelayCommand]
    private async Task Confirm()
    {
        if (!await _canConfirmAsync())
        {
            ErrorText = _blockedMessage;

            return;
        }

        IsConfirmed = true;

        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Закрывает окно без сохранения.
    /// </summary>
    [RelayCommand]
    private void Cancel() => CloseRequested?.Invoke(this, EventArgs.Empty);
}
