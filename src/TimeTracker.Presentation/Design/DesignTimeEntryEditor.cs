using TimeTracker.Application;

namespace TimeTracker.Presentation.Design;

/// <summary>
/// Заглушка входящего порта управления записью для дизайнера XAML: ничего не меняет.
/// </summary>
internal sealed class DesignTimeEntryEditor : ITimeEntryEditor
{
    /// <summary>
    /// Переименовывает запись.
    /// </summary>
    /// <param name="entryId">Идентификатор записи.</param>
    /// <param name="name">Новое имя задачи.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public Task RenameAsync(Guid entryId, string name, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
