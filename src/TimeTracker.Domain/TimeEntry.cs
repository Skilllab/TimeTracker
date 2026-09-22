namespace TimeTracker.Domain;

/// <summary>
/// Запись времени: интервал работы с накопленным временем пауз.
/// </summary>
public sealed class TimeEntry
{
    /// <summary>
    /// Создает запись времени.
    /// </summary>
    /// <param name="id">Идентификатор записи.</param>
    /// <param name="description">Описание работы; обрезается по краям.</param>
    /// <param name="startedAt">Момент начала записи.</param>
    /// <param name="endedAt">Момент окончания; <c>null</c>, пока запись идет.</param>
    /// <param name="pausedSeconds">Накопленное время пауз в секундах.</param>
    /// <param name="pausedAt">Момент начала паузы; <c>null</c>, если запись не на паузе.</param>
    /// <param name="isBillable">Признак биллингуемости.</param>
    /// <param name="projectId">Идентификатор проекта; <c>null</c>, если категория не задана.</param>
    public TimeEntry(
        Guid id,
        string description,
        DateTimeOffset startedAt,
        DateTimeOffset? endedAt,
        int pausedSeconds,
        DateTimeOffset? pausedAt,
        bool isBillable,
        Guid? projectId)
    {
        if (endedAt < startedAt)
        {
            throw new InvalidTimeEntryException("Запись не может заканчиваться раньше начала.");
        }

        if (pausedSeconds < 0)
        {
            throw new InvalidTimeEntryException("Накопленное время пауз не может быть отрицательным.");
        }

        if (pausedAt < startedAt)
        {
            throw new InvalidTimeEntryException("Пауза не может начаться раньше начала записи.");
        }

        var normalized = (description ?? string.Empty).Trim();

        if (normalized.Length > MaxDescriptionLength)
        {
            throw new InvalidTimeEntryException($"Описание длиннее {MaxDescriptionLength} символов.");
        }

        Id = id;
        Description = normalized;
        StartedAt = startedAt;
        EndedAt = endedAt;
        PausedSeconds = pausedSeconds;
        PausedAt = pausedAt;
        IsBillable = isBillable;
        ProjectId = projectId;
    }

    /// <summary>Предельная длина описания.</summary>
    private const int MaxDescriptionLength = 500;

    /// <summary>Идентификатор записи.</summary>
    public Guid Id { get; }

    /// <summary>Описание работы.</summary>
    public string Description { get; }

    /// <summary>Момент начала записи.</summary>
    public DateTimeOffset StartedAt { get; }

    /// <summary>Момент окончания; <c>null</c>, пока запись идет.</summary>
    public DateTimeOffset? EndedAt { get; }

    /// <summary>Накопленное время пауз в секундах.</summary>
    public int PausedSeconds { get; }

    /// <summary>Признак биллингуемости.</summary>
    public bool IsBillable { get; }

    /// <summary>Идентификатор проекта; <c>null</c>, если категория не задана.</summary>
    public Guid? ProjectId { get; }

    /// <summary>Момент начала паузы; <c>null</c>, если запись не на паузе.</summary>
    public DateTimeOffset? PausedAt { get; }

    /// <summary>Признак того, что запись приостановлена.</summary>
    public bool IsPaused => PausedAt is not null;

    /// <summary>
    /// Признак того, что запись еще идет.
    /// </summary>
    public bool IsOpen => EndedAt is null;

    /// <summary>
    /// Считает длительность записи без накопленного времени пауз.
    /// Для идущей записи конец принимается равным <paramref name="now"/>.
    /// </summary>
    /// <param name="now">Текущий момент времени.</param>
    public Duration ElapsedAt(DateTimeOffset now)
    {
        var end = EndedAt ?? now;
        var total = end - StartedAt - TimeSpan.FromSeconds(PausedSeconds);

        return total <= TimeSpan.Zero ? Duration.Zero : Duration.From(total);
    }

    /// <summary>
    /// Возвращает запись, приостановленную с указанного момента.
    /// </summary>
    /// <param name="now">Момент начала паузы.</param>
    public TimeEntry Pause(DateTimeOffset now)
    {
        if (!IsOpen || IsPaused)
        {
            throw new InvalidTimeEntryException("Приостановить можно только идущую запись.");
        }

        return new TimeEntry(Id, Description, StartedAt, EndedAt, PausedSeconds, now, IsBillable, ProjectId);
    }

    /// <summary>
    /// Возвращает запись, возобновленную с указанного момента.
    /// </summary>
    /// <param name="now">Момент возобновления.</param>
    public TimeEntry Resume(DateTimeOffset now)
    {
        if (PausedAt is null)
        {
            throw new InvalidTimeEntryException("Возобновить можно только приостановленную запись.");
        }

        var paused = (int)(now - PausedAt.Value).TotalSeconds;

        return new TimeEntry(Id, Description, StartedAt, EndedAt, PausedSeconds + paused, null, IsBillable, ProjectId);
    }

    /// <summary>
    /// Возвращает завершенную запись.
    /// </summary>
    /// <param name="now">Момент завершения.</param>
    public TimeEntry Close(DateTimeOffset now)
    {
        if (!IsOpen)
        {
            throw new InvalidTimeEntryException("Завершить можно только идущую запись.");
        }

        var paused = PausedAt is null ? 0 : (int)(now - PausedAt.Value).TotalSeconds;

        return new TimeEntry(Id, Description, StartedAt, now, PausedSeconds + paused, null, IsBillable, ProjectId);
    }
}
