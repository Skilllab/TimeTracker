using TimeTracker.Domain.Abstractions;
using TimeTracker.Domain.Clients;
using TimeTracker.Domain.Common;

namespace TimeTracker.Domain.Projects;

/// <summary>
/// Проект
///
/// Категория, к которой относятся записи времени. Например, «TimeTracker»,
/// «Client X — backend», «Внутренние задачи»
///
/// Каждый проект может быть привязан к клиенту (или быть без клиента).
/// Имеет цвет — используется в UI для визуальной группировки записей
/// по проектам.
/// </summary>
public sealed class Project : AggregateRoot<ProjectId>
{
    /// <summary>
    /// Максимальная длина имени проекта
    /// </summary>
    public const int MaxNameLength = 100;

    /// <summary>
    /// Имя проекта
    /// </summary>
    public string Name
    {
        get; private set;
    }

    /// <summary>
    /// Цвет проекта в UI
    /// </summary>
    public ProjectColor Color
    {
        get; private set;
    }

    /// <summary>
    /// Id клиента, если проект привязан к клиенту. null — проект
    /// без клиента (личные задачи, внутренние дела).
    /// </summary>
    public ClientId? ClientId
    {
        get; private set;
    }

    /// <summary>
    /// Признак «архивный». Архивные проекты не показываются в выборе
    /// при старте таймера, но остаются в старых записях и отчетах.
    ///
    /// Удаления проектов в проекте нет — только архивация, чтобы
    /// не терять историческую привязку записей.
    /// </summary>
    public bool IsArchived
    {
        get; private set;
    }

    /// <summary>
    /// Момент создания
    /// </summary>
    public DateTimeOffset CreatedAt
    {
        get; private init;
    }

    private Project(
        ProjectId id,
        string name,
        ProjectColor color,
        ClientId? clientId,
        bool isArchived,
        DateTimeOffset createdAt) : base(id)
    {
        Name = name;
        Color = color;
        ClientId = clientId;
        IsArchived = isArchived;
        CreatedAt = createdAt;
    }

    /// <summary>
    /// Создать новый активный проект
    /// </summary>
    /// <param name="name">Имя проекта</param>
    /// <param name="color">Цвет проекта</param>
    /// <param name="clientId">Id клиента</param>
    /// <param name="createdAt">Момент создания</param>
    public static Project Create(
        string name,
        ProjectColor color,
        ClientId? clientId = null,
        DateTimeOffset? createdAt = null)
    {
        Guard.AgainstNullOrWhiteSpace(name, nameof(name));

        var normalized = NormalizeName(name);
        return new Project(
            ProjectId.New(),
            normalized,
            color,
            clientId,
            isArchived: false,
            createdAt: createdAt ?? DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// Восстановить проект из хранилища
    /// </summary>
    /// <param name="id">Идентификатор проекта</param>
    /// <param name="name">Имя проекта</param>
    /// <param name="color">Цвет проекта</param>
    /// <param name="clientId">Id клиента</param>
    public static Project Restore(
        ProjectId id,
        string name,
        ProjectColor color,
        ClientId? clientId,
        bool isArchived,
        DateTimeOffset createdAt)
        => new(id, name, color, clientId, isArchived, createdAt);

    /// <summary>
    /// Переименовать проект
    /// </summary>
    /// <param name="name">Новое имя проекта</param>
    public void Rename(string name)
    {
        Guard.AgainstNullOrWhiteSpace(name, nameof(name));

        var normalized = NormalizeName(name);
        if (Name == normalized)
            return;

        Name = normalized;
    }

    /// <summary>
    /// Сменить цвет проекта
    /// </summary>
    /// <param name="color">Новый цвет проекта</param>
    public void Recolor(ProjectColor color)
    {
        if (Color == color)
            return;

        Color = color;
    }

    /// <summary>
    /// Привязать проект к клиенту или отвязать (clientId == null).
    /// </summary>
    /// <param name="clientId">Идентификатор клиента</param>
    public void AssignClient(ClientId? clientId)
    {
        if (ClientId == clientId)
            return;

        ClientId = clientId;
    }

    /// <summary>
    /// Поместить проект в архив
    /// </summary>
    public void Archive()
    {
        if (IsArchived)
            return;
        IsArchived = true;
    }

    /// <summary>
    /// Вернуть проект из архива
    /// </summary>
    public void Unarchive()
    {
        if (!IsArchived)
            return;
        IsArchived = false;
    }

    /// <summary>
    /// Нормализовать имя проекта: trim + проверка длины.
    /// </summary>
    /// <param name="name">Имя проекта</param>
    private static string NormalizeName(string name)
    {
        Guard.AgainstNullOrWhiteSpace(name, nameof(name));
        var normalized = name.Trim();

        if (normalized.Length > MaxNameLength)
            throw new DomainException($"Project name exceeds maximum length of {MaxNameLength}.");

        return normalized;
    }
}
