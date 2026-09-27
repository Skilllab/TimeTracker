using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeTracker.Application;
using TimeTracker.Domain;

namespace TimeTracker.Presentation.ViewModels;

/// <summary>
/// ViewModel окна тегов задачи: показывает теги самой задачи и имена, которые уже
/// встречались у других задач, и меняет теги через входящий порт управления задачами.
/// Снятие тега затрагивает только текущую задачу: справочника тегов нет,
/// поэтому у других задач имена остаются прежними.
/// </summary>
public sealed partial class TaskTagsViewModel : ObservableObject
{
    private readonly Guid _taskId;
    private readonly ITaskControl _taskControl;
    private readonly ITaskTagSuggestions _tagSuggestions;

    /// <summary>
    /// Создает окно тегов задачи.
    /// </summary>
    /// <param name="title">Заголовок окна.</param>
    /// <param name="taskTitle">Наименование задачи.</param>
    /// <param name="taskId">Идентификатор задачи.</param>
    /// <param name="assignedTags">Теги, назначенные задаче.</param>
    /// <param name="taskControl">Входящий порт управления задачами.</param>
    /// <param name="tagSuggestions">Входящий порт подсказки имен тегов.</param>
    public TaskTagsViewModel(
        string title,
        string taskTitle,
        Guid taskId,
        IReadOnlyList<string> assignedTags,
        ITaskControl taskControl,
        ITaskTagSuggestions tagSuggestions)
    {
        Title = title;
        TaskTitle = taskTitle;
        _taskId = taskId;
        _taskControl = taskControl ?? throw new ArgumentNullException(nameof(taskControl));
        _tagSuggestions = tagSuggestions ?? throw new ArgumentNullException(nameof(tagSuggestions));

        foreach (var tag in assignedTags ?? Array.Empty<string>())
        {
            AssignedTags.Add(tag);
        }

        _ = RefreshSuggestionsAsync();
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
    /// Наименование задачи.
    /// </summary>
    public string TaskTitle { get; }

    /// <summary>
    /// Теги, назначенные задаче.
    /// </summary>
    public ObservableCollection<string> AssignedTags { get; } = new();

    /// <summary>
    /// Имена тегов, которые уже встречались у других задач.
    /// </summary>
    public ObservableCollection<string> AvailableTags { get; } = new();

    /// <summary>
    /// Выбранный тег задачи.
    /// </summary>
    [ObservableProperty]
    private string? _selectedAssignedTag;

    /// <summary>
    /// Выбранное имя из подсказок.
    /// </summary>
    [ObservableProperty]
    private string? _selectedAvailableTag;

    /// <summary>
    /// Имя нового тега.
    /// </summary>
    [ObservableProperty]
    private string _newTagName = string.Empty;

    /// <summary>
    /// Текст ошибки; пустая строка, если ошибки нет.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string _errorText = string.Empty;

    /// <summary>
    /// Признак того, что теги задачи изменились.
    /// </summary>
    [ObservableProperty]
    private bool _isChanged;

    /// <summary>
    /// Признак того, что есть текст ошибки.
    /// </summary>
    public bool HasError => ErrorText.Length > 0;

    /// <summary>
    /// Назначает задаче выбранный тег.
    /// </summary>
    [RelayCommand]
    private async Task AddAsync()
    {
        var tag = SelectedAvailableTag;

        if (string.IsNullOrWhiteSpace(tag))
        {
            return;
        }

        if (!await ApplyAsync(() => _taskControl.AddTagAsync(_taskId, tag)))
        {
            return;
        }

        AssignedTags.Add(tag);
        AvailableTags.Remove(tag);
        SelectedAvailableTag = null;
    }

    /// <summary>
    /// Снимает выбранный тег только с текущей задачи.
    /// </summary>
    [RelayCommand]
    private async Task RemoveAsync()
    {
        var tag = SelectedAssignedTag;

        if (string.IsNullOrWhiteSpace(tag))
        {
            return;
        }

        if (!await ApplyAsync(() => _taskControl.RemoveTagAsync(_taskId, tag)))
        {
            return;
        }

        AssignedTags.Remove(tag);
        SelectedAssignedTag = null;

        if (!AvailableTags.Contains(tag, StringComparer.OrdinalIgnoreCase))
        {
            AvailableTags.Add(tag);
        }
    }

    /// <summary>
    /// Создает тег с введенным именем и сразу назначает его задаче.
    /// </summary>
    [RelayCommand]
    private async Task CreateAsync()
    {
        var tag = (NewTagName ?? string.Empty).Trim();

        if (tag.Length == 0)
        {
            return;
        }

        if (!await ApplyAsync(() => _taskControl.AddTagAsync(_taskId, tag)))
        {
            return;
        }

        if (!AssignedTags.Contains(tag, StringComparer.OrdinalIgnoreCase))
        {
            AssignedTags.Add(tag);
        }

        AvailableTags.Remove(tag);
        NewTagName = string.Empty;
        SelectedAvailableTag = null;
    }

    /// <summary>
    /// Закрывает окно.
    /// </summary>
    [RelayCommand]
    private void Close() => CloseRequested?.Invoke(this, EventArgs.Empty);

    /// <summary>
    /// Выполняет изменение и переводит нарушение правила в текст ошибки.
    /// </summary>
    /// <param name="action">Действие над тегами задачи.</param>
    private async Task<bool> ApplyAsync(Func<Task> action)
    {
        ErrorText = string.Empty;

        try
        {
            await action();
        }
        catch (InvalidTaskStateException exception)
        {
            ErrorText = exception.Message;

            return false;
        }

        IsChanged = true;

        return true;
    }

    /// <summary>
    /// Заполняет подсказки именами, которые уже встречались, кроме назначенных задаче.
    /// </summary>
    private async Task RefreshSuggestionsAsync()
    {
        var names = await _tagSuggestions.SuggestAsync(null);

        AvailableTags.Clear();

        foreach (var name in names)
        {
            if (!AssignedTags.Contains(name, StringComparer.OrdinalIgnoreCase))
            {
                AvailableTags.Add(name);
            }
        }
    }
}
