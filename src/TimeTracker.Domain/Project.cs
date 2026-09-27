namespace TimeTracker.Domain;

/// <summary>
/// Проект: именованная категория работ с цветовым маркером.
/// Имя обрезается по краям; пустое имя и имя длиннее предельного недопустимы.
/// Цвет задается в виде #RRGGBB, а палитра лишь предлагает готовые значения,
/// поэтому выбрать можно любой цвет.
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
    /// Цвет проверяется по формату #RRGGBB.
    /// При нарушении любого правила бросается <c>InvalidProjectException</c>
    /// и проект не создается.
    /// </summary>
    /// <param name="id">Идентификатор проекта.</param>
    /// <param name="name">Имя проекта; обрезается по краям.</param>
    /// <param name="color">Цвет маркера в виде #RRGGBB.</param>
    public Project(Guid id, string name, string color)
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

        if (!ProjectPalette.IsValid(color))
        {
            throw new InvalidProjectException("Цвет проекта должен быть записан в виде #RRGGBB.");
        }

        Id = id;
        Name = normalized;
        Color = color;
    }

    /// <summary>Идентификатор проекта.</summary>
    public Guid Id { get; }

    /// <summary>Имя проекта.</summary>
    public string Name { get; }

    /// <summary>Цвет маркера в виде #RRGGBB.</summary>
    public string Color { get; }

    /// <summary>
    /// Возвращает проект с новым именем.
    /// Имя проверяется конструктором: пустое имя и имя длиннее предельного отвергаются.
    /// Идентификатор и цвет переносятся без изменений.
    /// </summary>
    /// <param name="name">Новое имя проекта; обрезается по краям.</param>
    public Project Rename(string name) => new(Id, name, Color);

    /// <summary>
    /// Возвращает проект с новым цветом маркера.
    /// Цвет проверяется по формату #RRGGBB.
    /// Остальные значения переносятся без изменений.
    /// </summary>
    /// <param name="color">Новый цвет маркера в виде #RRGGBB.</param>
    public Project ChangeColor(string color) => new(Id, Name, color);
}
