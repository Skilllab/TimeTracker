namespace TimeTracker.Domain;

/// <summary>
/// Проект: именованная категория работ с цветовым маркером.
/// Имя обрезается по краям; пустое имя и имя длиннее предельного недопустимы.
/// Цвет обязан входить в фиксированный набор палитры.
/// Признак архивности отделяет проекты, скрытые из выбора, от действующих.
/// Свойства получают переданные значения без дальнейших изменений,
/// поэтому созданный проект считается неизменяемым, а операции возвращают новый экземпляр.
/// </summary>
public sealed class Project
{
    /// <summary>Предельная длина имени проекта.</summary>
    private const int MaxNameLength = 100;

    /// <summary>
    /// Создает проект, проверяя согласованность значений.
    /// Имя обрезается по краям, а <c>null</c> заменяется пустой строкой;
    /// пустое имя и имя длиннее предельного отвергаются.
    /// Цвет проверяется по набору палитры.
    /// При нарушении любого правила бросается <c>InvalidProjectException</c>
    /// и проект не создается.
    /// </summary>
    /// <param name="id">Идентификатор проекта.</param>
    /// <param name="name">Имя проекта; обрезается по краям.</param>
    /// <param name="color">Цвет маркера в виде #RRGGBB из набора палитры.</param>
    /// <param name="isArchived">Признак того, что проект скрыт из выбора; новый проект не архивный.</param>
    public Project(Guid id, string name, string color, bool isArchived = false)
    {
        var normalized = (name ?? string.Empty).Trim();

        if (normalized.Length == 0)
        {
            throw new InvalidProjectException("Имя проекта не может быть пустым.");
        }

        if (normalized.Length > MaxNameLength)
        {
            throw new InvalidProjectException($"Имя проекта длиннее {MaxNameLength} символов.");
        }

        if (!ProjectPalette.Contains(color))
        {
            throw new InvalidProjectException("Цвет проекта не входит в палитру.");
        }

        Id = id;
        Name = normalized;
        Color = color;
        IsArchived = isArchived;
    }

    /// <summary>Идентификатор проекта.</summary>
    public Guid Id { get; }

    /// <summary>Имя проекта.</summary>
    public string Name { get; }

    /// <summary>Цвет маркера в виде #RRGGBB.</summary>
    public string Color { get; }

    /// <summary>Признак того, что проект скрыт из выбора.</summary>
    public bool IsArchived { get; }

    /// <summary>
    /// Возвращает проект с новым именем.
    /// Имя проверяется конструктором: пустое имя и имя длиннее предельного отвергаются.
    /// Идентификатор, цвет и признак архивности переносятся без изменений.
    /// </summary>
    /// <param name="name">Новое имя проекта; обрезается по краям.</param>
    public Project Rename(string name) => new(Id, name, Color, IsArchived);

    /// <summary>
    /// Возвращает проект с новым цветом маркера.
    /// Цвет проверяется по набору палитры, поэтому значение вне набора отвергается.
    /// Остальные значения переносятся без изменений.
    /// </summary>
    /// <param name="color">Новый цвет маркера в виде #RRGGBB из набора палитры.</param>
    public Project ChangeColor(string color) => new(Id, Name, color, IsArchived);

    /// <summary>
    /// Возвращает архивный проект.
    /// Пометка архива не удаляет проект: старые записи продолжают ссылаться на него,
    /// а из выбора он исчезает.
    /// </summary>
    public Project Archive() => new(Id, Name, Color, true);

    /// <summary>
    /// Возвращает проект, возвращенный из архива.
    /// После возврата проект снова доступен для выбора при старте записи.
    /// </summary>
    public Project Unarchive() => new(Id, Name, Color, false);
}
