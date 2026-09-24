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
}
