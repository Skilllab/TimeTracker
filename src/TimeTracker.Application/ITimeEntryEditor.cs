namespace TimeTracker.Application;

/// <summary>
/// Входящий порт: управление сохраненной записью времени.
/// </summary>
public interface ITimeEntryEditor
{
    /// <summary>
    /// Переименовывает запись.
    /// </summary>
    /// <param name="entryId">Идентификатор записи.</param>
    /// <param name="name">Новое имя задачи.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task RenameAsync(Guid entryId, string name, CancellationToken cancellationToken = default);
}
