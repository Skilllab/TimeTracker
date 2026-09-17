using TimeTracker.Domain.Clients;

namespace TimeTracker.Application.Abstractions.Persistence;

/// <summary>
/// Репозиторий клиентов
/// </summary>
public interface IClientRepository
{
    /// <summary>Найти клиента по Id. Возвращает null, если клиента нет</summary>
    /// <param name="id">Идентификатор клиента.</param>
    /// <param name="ct">Токен отмены операции.</param>
    Task<Client?> GetByIdAsync(ClientId id, CancellationToken ct = default);

    /// <summary>
    /// Все клиенты, включая архивных
    /// </summary>
    /// <param name="ct">Токен отмены операции</param>
    Task<IReadOnlyList<Client>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Только активные (не архивные) клиенты
    /// </summary>
    /// <param name="ct">Токен отмены операции.</param>
    Task<IReadOnlyList<Client>> GetActiveAsync(CancellationToken ct = default);

    /// <summary>
    /// Добавить клиента в ChangeTracker. Не сохраняет в БД.
    /// </summary>
    /// <param name="client">Клиент для добавления</param>
    /// <param name="ct">Токен отмены операции</param>
    Task AddAsync(Client client, CancellationToken ct = default);

    /// <summary>Пометить клиента на удаление в ChangeTracker</summary>
    /// <param name="client">Клиент для удаления</param>
    void Remove(Client client);
}
