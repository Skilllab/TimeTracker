namespace TimeTracker.Infrastructure;

/// <summary>
/// Связь задачи с тегом: отдельная таблица связи, потому что один тег принадлежит многим задачам.
/// Тип нужен только хранилищу: домен работает с набором тегов внутри задачи,
/// поэтому связь хранится строкой таблицы и не попадает в модель задачи.
/// </summary>
public sealed class WorkTaskTag
{
    /// <summary>Идентификатор задачи.</summary>
    public Guid TaskId { get; set; }

    /// <summary>Идентификатор тега.</summary>
    public Guid TagId { get; set; }
}
