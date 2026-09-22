namespace TimeTracker.Domain;

/// <summary>
/// Запись времени: интервал работы с накопленным временем пауз.
/// </summary>
public sealed class TimeEntry
{
    /// <summary>
    /// Создает запись времени, проверяя согласованность значений.
    /// Описание обрезается по краям, а <c>null</c> заменяется пустой строкой;
    /// длина описания ограничена 500 символами после обрезки.
    /// Окончание не может быть раньше начала, накопленное время пауз не может быть
    /// отрицательным, а момент начала паузы не может быть раньше начала записи:
    /// при нарушении любого правила бросается <c>InvalidTimeEntryException</c>
    /// и запись не создается.
    /// Все свойства получают переданные значения без дальнейших изменений,
    /// поэтому созданная запись считается неизменяемой.
    /// </summary>
    /// <param name="id">Идентификатор записи.</param>
    /// <param name="description">Описание работы; обрезается по краям, отсутствие заменяется пустой строкой.</param>
    /// <param name="startedAt">Момент начала записи; он же нижняя граница для остальных моментов.</param>
    /// <param name="endedAt">Момент окончания; <c>null</c>, пока запись идет.</param>
    /// <param name="pausedSeconds">Накопленное время пауз в секундах; не может быть отрицательным.</param>
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
    /// Конец интервала берется из записи, а для идущей записи принимается
    /// равным переданному моменту.
    /// Из полученного времени вычитается накопленное время пауз; незакрытая пауза
    /// в это значение еще не входит, поэтому на паузе длительность не растет.
    /// Результат ограничен нулем снизу, поэтому отрицательная длительность невозможна.
    /// Метод ничего не меняет и допускает повторные вызовы.
    /// </summary>
    /// <param name="now">Текущий момент времени; учитывается только пока запись идет.</param>
    public Duration ElapsedAt(DateTimeOffset now)
    {
        var end = EndedAt ?? now;
        var total = end - StartedAt - TimeSpan.FromSeconds(PausedSeconds);

        return total <= TimeSpan.Zero ? Duration.Zero : Duration.From(total);
    }

    /// <summary>
    /// Возвращает запись, приостановленную с указанного момента.
    /// Запоминает момент начала паузы и не меняет накопленное время пауз:
    /// незакрытая пауза начнет учитываться только при возобновлении или завершении.
    /// Начало, окончание, описание и признаки переносятся без изменений,
    /// идентификатор сохраняется, поэтому возвращается та же по смыслу запись.
    /// Приостановить можно только идущую запись: для завершенной или уже
    /// приостановленной бросается <c>InvalidTimeEntryException</c>.
    /// Момент начала паузы, переданный раньше начала записи, отсекается конструктором.
    /// </summary>
    /// <param name="now">Момент начала паузы; он же момент приостановки.</param>
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
    /// Время от начала паузы прибавляется к накопленному времени пауз и округляется
    /// вниз до целых секунд, поэтому простой перестает учитываться в длительности.
    /// Момент начала паузы снимается, а окончание не меняется: запись снова считается идущей.
    /// Возобновить можно только приостановленную запись, и момент возобновления
    /// не может быть раньше начала паузы: в обоих случаях бросается
    /// <c>InvalidTimeEntryException</c>.
    /// </summary>
    /// <param name="now">Момент возобновления; не раньше момента начала паузы.</param>
    public TimeEntry Resume(DateTimeOffset now)
    {
        if (PausedAt is null)
        {
            throw new InvalidTimeEntryException("Возобновить можно только приостановленную запись.");
        }
        if (now < PausedAt.Value)
        {
            throw new InvalidTimeEntryException("Момент возобновления не может быть раньше начала паузы.");
        }

        var paused = (int)(now - PausedAt.Value).TotalSeconds;

        return new TimeEntry(Id, Description, StartedAt, EndedAt, PausedSeconds + paused, null, IsBillable, ProjectId);
    }

    /// <summary>
    /// Возвращает запись, возобновленную с указанного момента.
    /// Время от начала паузы прибавляется к накопленному времени пауз и округляется
    /// вниз до целых секунд, поэтому простой перестает учитываться в длительности.
    /// Момент начала паузы снимается, а окончание не меняется: запись снова считается идущей.
    /// Возобновить можно только приостановленную запись, и момент возобновления
    /// не может быть раньше начала паузы: в обоих случаях бросается
    /// <c>InvalidTimeEntryException</c>.
    /// </summary>
    /// <param name="now">Момент возобновления; не раньше момента начала паузы.</param>
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
