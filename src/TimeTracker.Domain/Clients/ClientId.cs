using TimeTracker.Domain.Common;

namespace TimeTracker.Domain.Clients;

/// <summary>
/// Идентификатор клиента
/// </summary>
public readonly record struct ClientId(Guid Value)
{
    /// <summary>
    /// Создать новый уникальный Id
    /// </summary>
    public static ClientId New() => new(Guid.NewGuid());

    /// <summary>
    /// Создать Id из готового Guid
    /// </summary>
    /// <param name="value">Guid для создания Id</param>
    public static ClientId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new DomainException("ClientId cannot be empty.");
        return new ClientId(value);
    }
}
