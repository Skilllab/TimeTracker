namespace TimeTracker.Application.Abstractions.Persistence;

/// <summary>
/// Единица работы. Оборачивает SaveChangesAsync у DbContext
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Сохранить все изменения ChangeTracker одной транзакцией
    /// </summary>
    /// <param name="ct">Токен отмены операции.</param>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
