using TimeTracker.Domain.Abstractions;
using TimeTracker.Domain.Common;

namespace TimeTracker.Domain.Clients;

/// <summary>
/// Клиент
///
/// Компания или человек, для которого ведется работа. Проекты могут
/// привязываться к клиенту, а через проекты — записи времени.
/// </summary>
public sealed class Client : AggregateRoot<ClientId>
{
    /// <summary>
    /// Максимальная длина имени клиента
    /// </summary>
    public const int MaxNameLength = 100;

    /// <summary>
    /// Имя клиента
    /// </summary>
    public string Name
    {
        get; private set;
    }

    /// <summary>
    /// Признак «архивный». Архивные клиенты скрываются из выбора
    /// при создании проекта, но остаются в отчетах и на проектах.
    ///
    /// Удаления клиентов нет, только архивация, чтобы не терять
    /// историческую привязку.
    /// </summary>
    public bool IsArchived
    {
        get; private set;
    }

    /// <summary>
    /// Момент создания. Для сортировки в UI.
    /// </summary>
    public DateTimeOffset CreatedAt
    {
        get; private init;
    }

    private Client(
        ClientId id,
        string name,
        bool isArchived,
        DateTimeOffset createdAt) : base(id)
    {
        Name = name;
        IsArchived = isArchived;
        CreatedAt = createdAt;
    }

    /// <summary>
    /// Создать нового активного клиента
    /// </summary>
    /// <param name="name">Имя клиента</param>
    /// <param name="createdAt">Момент создания</param>
    public static Client Create(string name, DateTimeOffset? createdAt = null)
    {
        Guard.AgainstNullOrWhiteSpace(name, nameof(name));

        var normalized = NormalizeName(name);
        return new Client(
            ClientId.New(),
            normalized,
            isArchived: false,
            createdAt: createdAt ?? DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// Восстановить клиента из хранилища
    /// <param name="id">Идентификатор клиента</param>
    /// <param name="name">Имя клиента</param>
    /// <param name="isArchived">Признак архивности</param>
    /// <param name="createdAt">Момент создания</param>
    /// </summary>
    public static Client Restore(
        ClientId id,
        string name,
        bool isArchived,
        DateTimeOffset createdAt)
        => new(id, name, isArchived, createdAt);

    /// <summary>
    /// Переименовать клиента
    /// </summary>
    /// <param name="name">Новое имя клиента</param>
    public void Rename(string name)
    {
        Guard.AgainstNullOrWhiteSpace(name, nameof(name));

        var normalized = NormalizeName(name);
        if (Name == normalized)
            return;

        Name = normalized;
    }

    /// <summary>
    /// Поместить клиента в архив
    /// </summary>
    public void Archive()
    {
        if (IsArchived)
            return;
        IsArchived = true;
    }

    /// <summary>
    /// Вернуть клиента из архива
    /// </summary>
    public void Unarchive()
    {
        if (!IsArchived)
            return;
        IsArchived = false;
    }

    /// <summary>
    /// Нормализовать имя клиента
    /// </summary>
    /// <param name="name">Имя клиента</param>
    private static string NormalizeName(string name)
    {
        Guard.AgainstNullOrWhiteSpace(name, nameof(name));
        var normalized = name.Trim();

        if (normalized.Length > MaxNameLength)
            throw new DomainException($"Client name exceeds maximum length of {MaxNameLength}.");

        return normalized;
    }
}
