using CommunityToolkit.Mvvm.ComponentModel;
using TimeTracker.Application;
using TimeTracker.Domain;
using TimeTracker.Presentation.Shell;

namespace TimeTracker.Presentation.ViewModels;

/// <summary>
/// Плашка задачи: показывает наименование, время, проект, теги и моменты,
/// а также сообщает, какие действия доступны задаче в текущем состоянии.
/// </summary>
public sealed class TaskCardViewModel : ObservableObject
{
    private TaskListItem _item;

    /// <summary>
    /// Создает плашку по строке списка задач.
    /// </summary>
    /// <param name="item">Строка списка задач.</param>
    public TaskCardViewModel(TaskListItem item)
    {
        _item = item ?? throw new ArgumentNullException(nameof(item));
    }

    /// <summary>Строка списка задач.</summary>
    public TaskListItem Item => _item;

    /// <summary>Идентификатор задачи.</summary>
    public Guid Id => _item.Id;

    /// <summary>Наименование задачи.</summary>
    public string Title => _item.Description;

    /// <summary>Время работы по задаче словами.</summary>
    public string TimeText => _item.Elapsed.ToClockString();

    /// <summary>Имя проекта; пустая строка, если проект не задан.</summary>
    public string ProjectName => _item.ProjectName;

    /// <summary>Цвет проекта как цвет текста его имени; пустая строка, если проект не задан.</summary>
    public string ProjectColor => _item.ProjectColor;

    /// <summary>Признак того, что у задачи есть проект.</summary>
    public bool HasProject => !string.IsNullOrEmpty(_item.ProjectName);

    /// <summary>Признак того, что у задачи нет проекта.</summary>
    public bool IsWithoutProject => !HasProject;

    /// <summary>Идентификатор проекта; <c>null</c>, если проект не задан.</summary>
    public Guid? ProjectId => _item.ProjectId;

    /// <summary>Имена тегов задачи.</summary>
    public IReadOnlyList<string> TagNames => _item.TagNames;

    /// <summary>Признак того, что у задачи есть теги.</summary>
    public bool HasTags => _item.TagNames.Count > 0;

    /// <summary>Признак того, что у задачи нет тегов: тогда показывается надпись о добавлении.</summary>
    public bool HasNoTags => !HasTags;

    /// <summary>
    /// Метки жизненного цикла задачи словами: создана, начата и завершена.
    /// Подписи берутся из словаря, потому что зависят от языка.
    /// </summary>
    public IReadOnlyList<string> LabelTexts
        => _item.Labels.Select(LabelText).ToList();

    /// <summary>
    /// Возвращает подпись метки на текущем языке.
    /// </summary>
    /// <param name="label">Метка жизненного цикла.</param>
    private static string LabelText(TaskLifecycle label)
    {
        var key = label switch
        {
            TaskLifecycle.Started => "Tasks.LabelStarted",
            TaskLifecycle.Finished => "Tasks.LabelFinished",
            _ => "Tasks.LabelCreated"
        };

        return LocalizationManager.Current?[key] ?? string.Empty;
    }

    /// <summary>Дата и время создания задачи.</summary>
    public string CreatedText => _item.CreatedAt.ToLocalTime().ToString("g");

    /// <summary>Дата и время первого запуска; пустая строка, если задача не начиналась.</summary>
    public string StartedText => _item.StartedAt is DateTimeOffset started
        ? started.ToLocalTime().ToString("g")
        : string.Empty;

    /// <summary>Признак того, что задача уже начиналась.</summary>
    public bool HasStarted => _item.StartedAt is not null;

    /// <summary>Признак того, что задача еще не начиналась.</summary>
    public bool IsNotStarted => _item.IsNotStarted;

    /// <summary>Признак того, что задача выполняется.</summary>
    public bool IsPaused => _item.IsPaused;

    /// <summary>Признак того, что задача выполняется.</summary>
    public bool IsRunning => _item.IsRunning;

    /// <summary>Признак того, что задачу можно завершить.</summary>
    public bool CanFinish => _item.CanFinish;

    /// <summary>Признак того, что завершенную задачу можно вернуть в работу.</summary>
    public bool CanReopen => _item.CanReopen;

    /// <summary>
    /// Признак того, что задачу можно запустить, приостановить или продолжить.
    /// </summary>
    public bool CanToggle => IsNotStarted || IsRunning || IsPaused;

    /// <summary>
    /// Признак того, что проект задачи можно сменить.
    /// У выполняемой задачи проект не меняют: запись о работе уже открыта с прежним проектом.
    /// </summary>
    public bool CanChangeProject => !IsRunning;

    /// <summary>
    /// Надпись на кнопке переключения: называет действие, доступное задаче сейчас.
    /// </summary>
    public string ToggleCaption
    {
        get
        {
            var key = IsNotStarted ? "Tasks.Start" : IsRunning ? "Tasks.Pause" : "Tasks.Resume";

            return LocalizationManager.Current?[key] ?? string.Empty;
        }
    }

    /// <summary>
    /// Обновляет плашку по свежей строке списка.
    /// Уведомление передается без имени свойства: обновляются все показанные значения,
    /// поэтому перечислять их по одному не требуется.
    /// </summary>
    /// <param name="item">Свежая строка списка задач.</param>
    public void Update(TaskListItem item)
    {
        _item = item ?? throw new ArgumentNullException(nameof(item));

        OnPropertyChanged(string.Empty);
    }
}
