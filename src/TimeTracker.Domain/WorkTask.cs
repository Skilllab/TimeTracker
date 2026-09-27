namespace TimeTracker.Domain;

/// <summary>
/// Задача: работа с именем, проектом и тегами, которую можно запускать, приостанавливать и завершать.
/// Задача существует с момента создания, поэтому может быть еще не начата: начало отсчета
/// появляется только при первом запуске, а длительность считается от него за вычетом пауз.
/// Каждый запуск и продолжение обновляют момент последнего запуска: по нему задачи
/// упорядочиваются в списке, поэтому давно не запускавшиеся уходят вниз.
/// Все операции возвращают новую задачу, а свойства остаются доступными только для чтения.
/// </summary>
public sealed class WorkTask
{
    private string _tags;

    /// <summary>
    /// Создает задачу, проверяя имя и согласованность моментов с состоянием.
    /// Имя обрезается по краям; пустое имя и имя из одних пробелов отвергаются,
    /// а слишком длинное считается ошибкой данных и не сохраняется.
    /// Накопленное время пауз не может быть отрицательным; завершенная задача обязана
    /// иметь момент завершения; выполняемая не может находиться на паузе;
    /// приостановленная обязана иметь момент паузы; неначатая не может иметь момент запуска.
    /// </summary>
    /// <param name="id">Идентификатор задачи.</param>
    /// <param name="description">Наименование задачи; обрезается по краям.</param>
    /// <param name="projectId">Идентификатор проекта; <c>null</c>, если проект не задан.</param>
    /// <param name="createdAt">Момент создания задачи.</param>
    /// <param name="startedAt">Момент первого запуска; <c>null</c>, если задача не начиналась.</param>
    /// <param name="lastStartedAt">Момент последнего запуска; <c>null</c>, если задача не начиналась.</param>
    /// <param name="finishedAt">Момент завершения; <c>null</c>, если задача не завершена.</param>
    /// <param name="pausedAt">Момент начала текущей паузы; <c>null</c>, если паузы нет.</param>
    /// <param name="pausedSeconds">Накопленное время пауз в секундах; не может быть отрицательным.</param>
    /// <param name="isBillable">Признак биллингуемости.</param>
    /// <param name="status">Текущее состояние задачи.</param>
    public WorkTask(
        Guid id,
        string description,
        Guid? projectId,
        DateTimeOffset createdAt,
        DateTimeOffset? startedAt,
        DateTimeOffset? lastStartedAt,
        DateTimeOffset? finishedAt,
        DateTimeOffset? pausedAt,
        int pausedSeconds,
        bool isBillable,
        TaskStatus status)
        : this(id, description, projectId, createdAt, startedAt, lastStartedAt, finishedAt, pausedAt, pausedSeconds, isBillable, status, null)
    {
    }

    /// <summary>
    /// Создает задачу вместе с тегами.
    /// Теги передаются при чтении задачи из хранилища и при доменных операциях:
    /// они переносят набор тегов без изменений.
    /// </summary>
    /// <param name="id">Идентификатор задачи.</param>
    /// <param name="description">Наименование задачи; обрезается по краям.</param>
    /// <param name="projectId">Идентификатор проекта; <c>null</c>, если проект не задан.</param>
    /// <param name="createdAt">Момент создания задачи.</param>
    /// <param name="startedAt">Момент первого запуска; <c>null</c>, если задача не начиналась.</param>
    /// <param name="lastStartedAt">Момент последнего запуска; <c>null</c>, если задача не начиналась.</param>
    /// <param name="finishedAt">Момент завершения; <c>null</c>, если задача не завершена.</param>
    /// <param name="pausedAt">Момент начала текущей паузы; <c>null</c>, если паузы нет.</param>
    /// <param name="pausedSeconds">Накопленное время пауз в секундах; не может быть отрицательным.</param>
    /// <param name="isBillable">Признак биллингуемости.</param>
    /// <param name="status">Текущее состояние задачи.</param>
    /// <param name="tags">Имена тегов задачи одной строкой через разделитель; <c>null</c> заменяется пустой строкой.</param>
    private WorkTask(
        Guid id,
        string description,
        Guid? projectId,
        DateTimeOffset createdAt,
        DateTimeOffset? startedAt,
        DateTimeOffset? lastStartedAt,
        DateTimeOffset? finishedAt,
        DateTimeOffset? pausedAt,
        int pausedSeconds,
        bool isBillable,
        TaskStatus status,
        string? tags)
    {
        if (pausedSeconds < 0)
        {
            throw new InvalidTaskStateException("Накопленное время пауз не может быть отрицательным.");
        }

        if (status == TaskStatus.Finished && finishedAt is null)
        {
            throw new InvalidTaskStateException("Завершенная задача должна иметь момент завершения.");
        }

        if (status == TaskStatus.Running && pausedAt is not null)
        {
            throw new InvalidTaskStateException("Выполняемая задача не может находиться на паузе.");
        }

        if (status == TaskStatus.Paused && pausedAt is null)
        {
            throw new InvalidTaskStateException("Приостановленная задача должна иметь момент паузы.");
        }

        if (status == TaskStatus.NotStarted && startedAt is not null)
        {
            throw new InvalidTaskStateException("Неначатая задача не может иметь момент запуска.");
        }

        Id = id;
        Description = NormalizeName(description);
        ProjectId = projectId;
        CreatedAt = createdAt;
        StartedAt = startedAt;
        LastStartedAt = lastStartedAt;
        FinishedAt = finishedAt;
        PausedAt = pausedAt;
        PausedSeconds = pausedSeconds;
        IsBillable = isBillable;
        Status = status;
        _tags = NormalizeTags(tags);
    }

    /// <summary>
    /// Создает задачу, которая еще не начиналась.
    /// Наименование обязательно, проект и теги можно задать позже.
    /// Момент создания фиксируется, а отсчет времени начнется только при первом запуске.
    /// </summary>
    /// <param name="id">Идентификатор задачи.</param>
    /// <param name="description">Наименование задачи; обрезается по краям.</param>
    /// <param name="projectId">Идентификатор проекта; <c>null</c>, если проект не задан.</param>
    /// <param name="now">Момент создания задачи.</param>
    public static WorkTask Create(Guid id, string description, Guid? projectId, DateTimeOffset now)
    {
        return new WorkTask(
            id,
            description,
            projectId,
            now,
            null,
            null,
            null,
            null,
            0,
            false,
            TaskStatus.NotStarted,
            null);
    }

    /// <summary>Предельная длина наименования задачи.</summary>
    private const int MaxDescriptionLength = 500;

    /// <summary>Разделитель имен тегов в строке задачи.</summary>
    private const string TagSeparator = "|";

    /// <summary>Идентификатор задачи.</summary>
    public Guid Id { get; }

    /// <summary>Наименование задачи.</summary>
    public string Description { get; }

    /// <summary>Идентификатор проекта; <c>null</c>, если проект не задан.</summary>
    public Guid? ProjectId { get; }

    /// <summary>Момент создания задачи.</summary>
    public DateTimeOffset CreatedAt { get; }

    /// <summary>Момент первого запуска; <c>null</c>, если задача не начиналась.</summary>
    public DateTimeOffset? StartedAt { get; }

    /// <summary>Момент последнего запуска; <c>null</c>, если задача не начиналась.</summary>
    public DateTimeOffset? LastStartedAt { get; }

    /// <summary>Момент завершения; <c>null</c>, если задача не завершена.</summary>
    public DateTimeOffset? FinishedAt { get; }

    /// <summary>Момент начала текущей паузы; <c>null</c>, если паузы нет.</summary>
    public DateTimeOffset? PausedAt { get; }

    /// <summary>Накопленное время пауз в секундах.</summary>
    public int PausedSeconds { get; }

    /// <summary>Признак биллингуемости.</summary>
    public bool IsBillable { get; }

    /// <summary>Текущее состояние задачи.</summary>
    public TaskStatus Status { get; }

    /// <summary>
    /// Имена тегов задачи одной строкой через разделитель.
    /// Теги хранятся прямо в задаче: отдельного справочника тегов нет,
    /// поэтому удаление имени из справочника не может ничего сломать.
    /// </summary>
    public string Tags => _tags;

    /// <summary>
    /// Имена тегов задачи по отдельности, в том же порядке, что и в строке.
    /// Разбор нужен для показа тегов, отбора по тегам и отчетов.
    /// </summary>
    public IReadOnlyList<string> TagNames
        => _tags.Length == 0
            ? Array.Empty<string>()
            : _tags.Split(TagSeparator, StringSplitOptions.RemoveEmptyEntries);

    /// <summary>Признак того, что задача еще не начиналась.</summary>
    public bool IsNotStarted => Status == TaskStatus.NotStarted;

    /// <summary>Признак того, что задача выполняется.</summary>
    public bool IsRunning => Status == TaskStatus.Running;

    /// <summary>Признак того, что задача приостановлена.</summary>
    public bool IsPaused => Status == TaskStatus.Paused;

    /// <summary>Признак того, что задача завершена.</summary>
    public bool IsFinished => Status == TaskStatus.Finished;

    /// <summary>
    /// Метки жизненного цикла задачи: создана, начата и завершена.
    /// Метки выводятся из моментов и состояния, а не хранятся в базе,
    /// поэтому они не могут разойтись с самими данными.
    /// Неначатая задача имеет только метку создания; начатая — еще и метку начала;
    /// завершенная — все три, потому что она и создана, и была начата.
    /// Порядок меток повторяет ход работы задачи.
    /// </summary>
    public IReadOnlyList<TaskLifecycle> Labels
    {
        get
        {
            var labels = new List<TaskLifecycle> { TaskLifecycle.Created };

            if (StartedAt is not null)
            {
                labels.Add(TaskLifecycle.Started);
            }

            if (Status == TaskStatus.Finished)
            {
                labels.Add(TaskLifecycle.Finished);
            }

            return labels;
        }
    }

    /// <summary>
    /// Считает время работы по задаче без накопленных пауз.
    /// Конец отсчета берется по состоянию: для приостановленной — момент паузы,
    /// для завершенной — момент завершения, для выполняемой — переданный момент.
    /// Неначатая задача времени не имеет, поэтому результат равен нулю.
    /// Незакрытая пауза в результат еще не входит, поэтому на паузе время не растет.
    /// Результат ограничен нулем снизу, поэтому отрицательное значение невозможно.
    /// Метод ничего не меняет и допускает повторные вызовы.
    /// </summary>
    /// <param name="now">Текущий момент времени; учитывается только для выполняемой задачи.</param>
    public Duration ElapsedAt(DateTimeOffset now)
    {
        if (StartedAt is not DateTimeOffset start)
        {
            return Duration.Zero;
        }

        var end = Status switch
        {
            TaskStatus.Paused => PausedAt ?? now,
            TaskStatus.Finished => FinishedAt ?? now,
            _ => now
        };

        var total = end - start - TimeSpan.FromSeconds(PausedSeconds);

        return total <= TimeSpan.Zero ? Duration.Zero : Duration.From(total);
    }

    /// <summary>
    /// Возвращает задачу, запущенную с указанного момента.
    /// Запоминает момент первого запуска и момент последнего запуска:
    /// по последнему задача поднимается вверх в списке.
    /// Запустить можно только задачу, которая еще не начиналась:
    /// повторный запуск идущей задачи бросает <c>InvalidTaskStateException</c>,
    /// а приостановленная продолжается операцией продолжения.
    /// </summary>
    /// <param name="now">Момент запуска задачи.</param>
    public WorkTask Start(DateTimeOffset now)
    {
        if (Status != TaskStatus.NotStarted)
        {
            throw new InvalidTaskStateException("Начать можно только задачу, которая еще не начиналась.");
        }

        return new WorkTask(
            Id,
            Description,
            ProjectId,
            CreatedAt,
            now,
            now,
            null,
            null,
            PausedSeconds,
            IsBillable,
            TaskStatus.Running,
            _tags);
    }

    /// <summary>
    /// Возвращает задачу, приостановленную с указанного момента.
    /// Запоминает момент начала паузы и не меняет накопленное время пауз:
    /// незакрытая пауза начнет учитываться только при продолжении или завершении,
    /// поэтому на паузе время задачи замирает.
    /// Приостановить можно только выполняемую задачу: в остальных состояниях
    /// бросается <c>InvalidTaskStateException</c>.
    /// </summary>
    /// <param name="now">Момент приостановки задачи; он же момент начала паузы.</param>
    public WorkTask Pause(DateTimeOffset now)
    {
        if (Status != TaskStatus.Running)
        {
            throw new InvalidTaskStateException("Приостановить можно только выполняемую задачу.");
        }

        return new WorkTask(
            Id,
            Description,
            ProjectId,
            CreatedAt,
            StartedAt,
            LastStartedAt,
            null,
            now,
            PausedSeconds,
            IsBillable,
            TaskStatus.Paused,
            _tags);
    }

    /// <summary>
    /// Возвращает задачу, продолженную с указанного момента.
    /// Время от начала паузы прибавляется к накопленному времени пауз и округляется
    /// вниз до целых секунд, поэтому простой не попадает в длительность задачи.
    /// Момент паузы снимается, а момент последнего запуска обновляется:
    /// продолженная задача возвращается наверх списка.
    /// Продолжить можно только приостановленную задачу, и момент продолжения
    /// не может быть раньше начала паузы: в обоих случаях бросается
    /// <c>InvalidTaskStateException</c>.
    /// </summary>
    /// <param name="now">Момент продолжения; не раньше момента начала паузы.</param>
    public WorkTask Resume(DateTimeOffset now)
    {
        if (Status != TaskStatus.Paused || PausedAt is null)
        {
            throw new InvalidTaskStateException("Продолжить можно только приостановленную задачу.");
        }

        if (now < PausedAt.Value)
        {
            throw new InvalidTaskStateException("Момент продолжения не может быть раньше приостановки.");
        }

        var paused = PausedSeconds + (int)(now - PausedAt.Value).TotalSeconds;

        return new WorkTask(
            Id,
            Description,
            ProjectId,
            CreatedAt,
            StartedAt,
            now,
            null,
            null,
            paused,
            IsBillable,
            TaskStatus.Running,
            _tags);
    }

    /// <summary>
    /// Возвращает завершенную задачу.
    /// Незакрытая пауза при завершении закрывается: ее время прибавляется
    /// к накопленному, поэтому простой не попадает в длительность.
    /// Момент завершения фиксируется, а момент последнего запуска не меняется.
    /// Завершить можно только выполняемую или приостановленную задачу: иначе
    /// бросается <c>InvalidTaskStateException</c>.
    /// </summary>
    /// <param name="now">Момент завершения задачи.</param>
    public WorkTask Finish(DateTimeOffset now)
    {
        if (Status != TaskStatus.Running && Status != TaskStatus.Paused)
        {
            throw new InvalidTaskStateException("Завершить можно только выполняемую или приостановленную задачу.");
        }

        var paused = PausedAt is null
            ? PausedSeconds
            : PausedSeconds + (int)(now - PausedAt.Value).TotalSeconds;

        return new WorkTask(
            Id,
            Description,
            ProjectId,
            CreatedAt,
            StartedAt,
            LastStartedAt,
            now,
            null,
            paused,
            IsBillable,
            TaskStatus.Finished,
            _tags);
    }

    /// <summary>
    /// Возвращает завершенную задачу снова в работе.
    /// Момент завершения снимается, а задача становится приостановленной:
    /// так время после возврата не идет, пока задачу не продолжат.
    /// Промежуток между завершением и возвратом уходит в накопленные паузы,
    /// иначе время простоя попало бы в длительность задачи.
    /// Вернуть в работу можно только завершенную задачу: иначе бросается
    /// <c>InvalidTaskStateException</c>.
    /// </summary>
    /// <param name="now">Момент возврата задачи в работу; он же момент начала паузы.</param>
    public WorkTask Reopen(DateTimeOffset now)
    {
        if (Status != TaskStatus.Finished)
        {
            throw new InvalidTaskStateException("Вернуть в работу можно только завершенную задачу.");
        }

        var idle = FinishedAt is DateTimeOffset finished && now > finished
            ? (int)(now - finished).TotalSeconds
            : 0;

        return new WorkTask(
            Id,
            Description,
            ProjectId,
            CreatedAt,
            StartedAt,
            LastStartedAt,
            null,
            now,
            PausedSeconds + idle,
            IsBillable,
            TaskStatus.Paused,
            _tags);
    }

    /// <summary>
    /// Возвращает задачу с новым наименованием.
    /// Наименование обязательно: пустое значение и значение из одних пробелов отвергаются.
    /// Состояние, моменты, накопленное время пауз и теги переносятся без изменений.
    /// </summary>
    /// <param name="name">Новое наименование задачи; обрезается по краям.</param>
    public WorkTask Rename(string name)
    {
        var normalized = NormalizeName(name);

        return new WorkTask(
            Id,
            normalized,
            ProjectId,
            CreatedAt,
            StartedAt,
            LastStartedAt,
            FinishedAt,
            PausedAt,
            PausedSeconds,
            IsBillable,
            Status,
            _tags);
    }

    /// <summary>
    /// Возвращает задачу с указанным проектом; <c>null</c> — задача без проекта.
    /// Имя, состояние, моменты, накопленное время пауз и теги переносятся без изменений,
    /// поэтому смена проекта не затрагивает ход выполнения задачи.
    /// </summary>
    /// <param name="projectId">Идентификатор проекта; <c>null</c> — задача без проекта.</param>
    public WorkTask WithProject(Guid? projectId)
    {
        return new WorkTask(
            Id,
            Description,
            projectId,
            CreatedAt,
            StartedAt,
            LastStartedAt,
            FinishedAt,
            PausedAt,
            PausedSeconds,
            IsBillable,
            Status,
            _tags);
    }

    /// <summary>
    /// Возвращает задачу с добавленным тегом.
    /// Повторное добавление того же имени ничего не меняет: одинаковых тегов у задачи не бывает.
    /// Имя проверяется по допустимым символам, поэтому неверное значение отвергается.
    /// </summary>
    /// <param name="tag">Добавляемое имя тега.</param>
    public WorkTask AddTag(string tag)
    {
        var names = new List<string>(TagNames) { tag };

        return Copy(string.Join(TagSeparator, names));
    }

    /// <summary>
    /// Возвращает задачу без указанного тега.
    /// Отсутствующий тег ничего не меняет: повторное удаление не считается ошибкой,
    /// потому что теги правятся и у завершенных задач.
    /// Сравнение имен идет без учета регистра: регистр не создает новый тег.
    /// </summary>
    /// <param name="tag">Удаляемое имя тега.</param>
    public WorkTask RemoveTag(string tag)
    {
        var names = TagNames.Where(existing => !string.Equals(existing, tag, StringComparison.OrdinalIgnoreCase));

        return Copy(string.Join(TagSeparator, names));
    }

    /// <summary>
    /// Возвращает задачу с указанной строкой тегов.
    /// Строка приводится к согласованному виду, поэтому пустая строка означает отсутствие тегов.
    /// </summary>
    /// <param name="tags">Имена тегов одной строкой; <c>null</c> заменяется пустой строкой.</param>
    public WorkTask WithTags(string? tags) => Copy(NormalizeTags(tags));

    /// <summary>
    /// Возвращает копию задачи с указанной строкой тегов.
    /// Состояние, моменты и признаки переносятся без изменений.
    /// </summary>
    /// <param name="tags">Строка тегов новой задачи; <c>null</c> — теги текущей задачи.</param>
    private WorkTask Copy(string? tags = null)
    {
        return new WorkTask(
            Id,
            Description,
            ProjectId,
            CreatedAt,
            StartedAt,
            LastStartedAt,
            FinishedAt,
            PausedAt,
            PausedSeconds,
            IsBillable,
            Status,
            tags ?? _tags);
    }

    /// <summary>
    /// Приводит строку тегов к согласованному виду.
    /// Имена обрезаются по краям, пустые отбрасываются, повторы убираются
    /// с сохранением порядка первого появления.
    /// Допустимы только буквы русского и латинского алфавитов и цифры:
    /// имя с другим символом считается ошибкой данных и отвергается.
    /// </summary>
    /// <param name="tags">Имена тегов одной строкой; <c>null</c> заменяется пустой строкой.</param>
    private static string NormalizeTags(string? tags)
    {
        if (string.IsNullOrWhiteSpace(tags))
        {
            return string.Empty;
        }

        var names = new List<string>();

        foreach (var candidate in tags.Split(TagSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            var name = candidate.Trim();

            if (name.Length == 0)
            {
                continue;
            }

            foreach (var symbol in name)
            {
                if (!IsAllowedTagSymbol(symbol))
                {
                    throw new InvalidTaskStateException($"Имя тега может содержать только буквы и цифры: {name}.");
                }
            }

            if (!names.Contains(name, StringComparer.OrdinalIgnoreCase))
            {
                names.Add(name);
            }
        }

        return string.Join(TagSeparator, names);
    }

    /// <summary>
    /// Проверяет, что символ допустим в имени тега.
    /// Разрешены только цифры и буквы русского и латинского алфавитов.
    /// </summary>
    /// <param name="symbol">Проверяемый символ.</param>
    private static bool IsAllowedTagSymbol(char symbol)
    {
        if (symbol >= '0' && symbol <= '9')
        {
            return true;
        }

        if ((symbol >= 'a' && symbol <= 'z') || (symbol >= 'A' && symbol <= 'Z'))
        {
            return true;
        }

        return (symbol >= 'а' && symbol <= 'я') || (symbol >= 'А' && symbol <= 'Я');
    }

    /// <summary>
    /// Обрезает наименование по краям и проверяет, что оно не пустое и не слишком длинное.
    /// </summary>
    /// <param name="name">Проверяемое наименование задачи.</param>
    private static string NormalizeName(string name)
    {
        var normalized = (name ?? string.Empty).Trim();

        if (normalized.Length == 0)
        {
            throw new InvalidTaskStateException("Наименование задачи не может быть пустым.");
        }

        if (normalized.Length > MaxDescriptionLength)
        {
            throw new InvalidTaskStateException($"Наименование задачи длиннее {MaxDescriptionLength} символов.");
        }

        return normalized;
    }
}
