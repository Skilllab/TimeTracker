using TimeTracker.Domain;

namespace TimeTracker.Presentation.ViewModels;

/// <summary>
/// Строка списка записей: запись времени и проект, к которому она отнесена.
/// </summary>
public sealed class EntryRowViewModel
{
    /// <summary>
    /// Создает строку списка.
    /// </summary>
    /// <param name="entry">Запись времени.</param>
    /// <param name="project">Проект записи; <c>null</c>, если категория не задана.</param>
    public EntryRowViewModel(TimeEntry entry, Project? project)
    {
        Entry = entry ?? throw new ArgumentNullException(nameof(entry));
        Project = project;
    }
    /// <summary>
    /// Имя задачи записи.
    /// </summary>
    public string TaskName => Entry.Description;

    /// <summary>
    /// Признак того, что запись завершена и ее имя можно править из списка.
    /// Идущая запись правится на экране таймера: ее состоянием владеет сценарий таймера.
    /// </summary>
    public bool CanRename => !Entry.IsOpen;

    /// <summary>
    /// Запись времени.
    /// </summary>
    public TimeEntry Entry { get; }

    /// <summary>
    /// Проект записи; <c>null</c>, если категория не задана.
    /// </summary>
    public Project? Project { get; }

    /// <summary>
    /// Имя проекта; пустая строка, если категория не задана.
    /// </summary>
    public string ProjectName => Project?.Name ?? string.Empty;

    /// <summary>
    /// Цвет маркера проекта; пустая строка, если категория не задана.
    /// </summary>
    public string ProjectColor => Project?.Color ?? string.Empty;

    /// <summary>
    /// Признак того, что проект записи архивный.
    /// Архивный проект показывается приглушенно: он остается в старых записях,
    /// но выбрать его для новой записи нельзя.
    /// </summary>
    public bool IsProjectArchived => Project?.IsArchived ?? false;
}
