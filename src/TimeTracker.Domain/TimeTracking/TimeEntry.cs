using System.Collections.Immutable;
using System.Collections.ObjectModel;
using TimeTracker.Domain.Abstractions;
using TimeTracker.Domain.Common;
using TimeTracker.Domain.Projects;
using TimeTracker.Domain.Tags;
using TimeTracker.Domain.TimeTracking.Events;

namespace TimeTracker.Domain.TimeTracking;

/// <summary>
/// Запись времени.
///
/// Основная единица учета в проекте. Одна запись = один интервал
/// времени, в течение которого пользователь работал над чем-то.
/// </summary>
public sealed class TimeEntry : AggregateRoot<TimeEntryId>
{
    /// <summary>
    /// Максимальная длина описания
    /// </summary>
    public const int MaxDescriptionLength = 500;

    /// <summary>
    /// Список тегов записи
    /// </summary>
    private readonly List<Tag> _tags = [];

    /// <summary>
    /// Read-only-обертка над _tags.
    /// </summary>
    private readonly ReadOnlyCollection<Tag> _tagsView;


    /// <summary>
    /// Описание записи
    /// </summary>
    public string Description
    {
        get; private set;
    }

    /// <summary>
    /// Диапазон времени записи
    /// </summary>
    public TimeRange Range
    {
        get; private set;
    }

    /// <summary>
    /// Id проекта, к которому отнесена запись. null — запись без проекта.
    /// </summary>
    public ProjectId? ProjectId
    {
        get; private set;
    }

    /// <summary>
    /// Признак «биллингуемая запись». По умолчанию true — большинство
    /// записей работы идут в счет клиенту.

    /// </summary>
    public bool IsBillable
    {
        get; private set;
    }

    /// <summary>
    /// Теги записи. Возвращается read-only-обертка
    /// </summary>
    public IReadOnlyList<Tag> Tags => _tagsView;


    private TimeEntry(
        TimeEntryId id,
        string description,
        TimeRange range,
        ProjectId? projectId,
        bool isBillable) : base(id)
    {
        Description = description;
        Range = range;
        ProjectId = projectId;
        IsBillable = isBillable;
        _tagsView = _tags.AsReadOnly();
    }

    /// <summary>
    /// Создать и сразу запустить новую запись. Основная точка входа
    /// для начала трекинга.
    /// </summary>
    /// <param name="description">Описание записи</param>
    /// <param name="projectId">Id проекта</param>
    /// <param name="startedAt">Момент начала записи</param>
    public static TimeEntry StartNew(
        string? description = null,
        ProjectId? projectId = null,
        DateTimeOffset? startedAt = null)
    {
        var normalized = NormalizeDescription(description);
        var start = startedAt ?? DateTimeOffset.UtcNow;
        var entry = new TimeEntry(
            TimeEntryId.New(),
            normalized,
            new TimeRange(start, null),
            projectId,
            isBillable: true);

        entry.Raise(new TimerStartedEvent(entry.Id, start));
        return entry;
    }

    /// <summary>
    /// Восстановить запись из хранилища
    /// </summary>
    /// <param name="id">Id записи</param>
    /// <param name="description">Описание записи</param>
    /// <param name="range">Диапазон времени записи</param>
    /// <param name="projectId">Id проекта</param>
    /// <param name="isBillable">Признак «биллингуемая запись»</param>
    /// <param name="tags">Теги записи</param>
    public static TimeEntry Restore(
        TimeEntryId id,
        string description,
        TimeRange range,
        ProjectId? projectId,
        bool isBillable,
        IEnumerable<Tag>? tags = null)
    {
        var entry = new TimeEntry(id, description, range, projectId, isBillable);
        if (tags is not null)
            entry._tags.AddRange(tags);
        return entry;
    }

    /// <summary>
    /// Остановить запись. Проставляет Range.End и поднимает TimerStoppedEvent.
    /// </summary>
    /// <param name="stoppedAt">Момент остановки записи. Если null — берется DateTimeOffset.UtcNow</param>
    public void Stop(DateTimeOffset? stoppedAt = null)
    {
        if (!Range.IsRunning)
            throw new DomainException("Entry is not running.");

        var now = stoppedAt ?? DateTimeOffset.UtcNow;
        if (now < Range.Start)
            throw new DomainException("Stop time cannot be before Start time.");

        Range = new TimeRange(Range.Start, now);
        Raise(new TimerStoppedEvent(Id, now, Range.Duration));
    }

    /// <summary>
    /// Изменить описание записи
    /// </summary>
    /// <param name="description">Новое описание записи</param>
    public void ChangeDescription(string? description)
    {
        var normalized = NormalizeDescription(description);
        if (Description == normalized)
            return;

        Description = normalized;
        Raise(new EntryEditedEvent(Id, DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// Привязать запись к проекту или отвязать (projectId == null)
    /// </summary>
    /// <param name="projectId">Id проекта</param>
    public void AssignProject(ProjectId? projectId)
    {
        if (ProjectId == projectId)
            return;

        ProjectId = projectId;
        Raise(new EntryEditedEvent(Id, DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// Добавить тег к записи
    /// </summary>
    /// <param name="tag">Тег для добавления</param>
    public void AddTag(Tag tag)
    {
        if (_tags.Contains(tag))
            return;

        _tags.Add(tag);
        Raise(new EntryEditedEvent(Id, DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// Убрать тег с записи
    /// </summary>
    /// <param name="tag">Тег для удаления</param>
    public void RemoveTag(Tag tag)
    {
        if (!_tags.Remove(tag))
            return;

        Raise(new EntryEditedEvent(Id, DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// Нормализовать описание: обрезать пробелы, проверить длину.
    ///
    /// null превращается в пустую строку, а не в ошибку. Это осознанно:
    /// запись без описания — нормальный сценарий (пользователь запустил
    /// таймер и забыл написать, чем занимается).
    /// </summary>
    /// <param name="description">Описание для нормализации</param>
    /// <exception cref="DomainException">
    /// Если после trim длина больше MaxDescriptionLength.
    /// </exception>
    private static string NormalizeDescription(string? description)
    {
        var value = description?.Trim() ?? string.Empty;
        if (value.Length > MaxDescriptionLength)
            throw new DomainException($"Description exceeds maximum length of {MaxDescriptionLength}.");
        return value;
    }
}
