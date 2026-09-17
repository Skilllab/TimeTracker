namespace TimeTracker.Domain.Abstractions;

/// <summary>
/// Корень агрегата для накопления доменных событий
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId> where TId : struct
{
    /// <summary>
    /// Список доменных событий, поднятых агрегатом с момента последней
    /// публикации. Наполняется через Raise(), очищается через
    /// ClearDomainEvents() — обычно это делает AppDbContext после того,
    /// как события опубликованы подписчикам и транзакция закоммичена.
    /// </summary>
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Базовый конструктор. Принимает готовый Id — генерация Id лежит на наследнике
    /// </summary>
    protected AggregateRoot(TId id) : base(id) { }

    /// <summary>
    /// Список доменных событий, поднятых агрегатом с момента последней
    /// публикации. Наполняется через Raise(), очищается через
    /// ClearDomainEvents() — обычно это делает AppDbContext после того,
    /// как события опубликованы подписчикам и транзакция закоммичена.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Поднять доменное событие. Вызывается из методов агрегата
    /// (TimeEntry.Stop, Project.Rename, ChangeDescription и т.д.) сразу
    /// после успешного изменения состояния.
    ///
    /// protected — снаружи агрегата Raise() недоступен. Только сам агрегат
    /// решает, какие события он поднимает и в какой момент. Это часть
    /// инкапсуляции: клиентский код вызывает Stop(), а не Raise(TimerStopped).
    /// </summary>
    /// <param name="domainEvent">Доменное событие</param>
    protected void Raise(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    /// <summary>
    /// Очистить список накопленных событий. Вызывается инфраструктурой
    /// после успешной публикации событий подписчикам — обычно в
    /// AppDbContext.SaveChangesAsync, ПОСЛЕ SaveChanges и ПОСЛЕ dispatch.
    ///
    /// Public, потому что вызывается снаружи агрегата — инфраструктурой.
    /// Если забыть вызвать — события накопятся и будут публиковаться
    /// повторно при следующем SaveChanges (двойная публикация, баги
    /// в подписчиках, утечка памяти).
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
