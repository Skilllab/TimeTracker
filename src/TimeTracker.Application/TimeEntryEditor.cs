using TimeTracker.Domain;

namespace TimeTracker.Application;

/// <summary>
/// Сценарий управления записью: читает запись, применяет доменную операцию и сохраняет результат.
/// </summary>
public sealed class TimeEntryEditor : ITimeEntryEditor
{
    private readonly ITimeEntryRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Создает сценарий.
    /// </summary>
    /// <param name="repository">Исходящий порт хранилища записей.</param>
    /// <param name="unitOfWork">Исходящий порт фиксации изменений.</param>
    public TimeEntryEditor(ITimeEntryRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// Переименовывает запись.
    /// Идущая запись не переименовывается: ее состоянием владеет таймер,
    /// и правка в обход него вернула бы прежнее имя при следующем переходе.
    /// </summary>
    /// <param name="entryId">Идентификатор записи.</param>
    /// <param name="name">Новое имя задачи.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public async Task RenameAsync(Guid entryId, string name, CancellationToken cancellationToken = default)
    {
        var entry = await _repository.GetByIdAsync(entryId, cancellationToken);

        if (entry is null)
        {
            throw new InvalidTimeEntryException("Запись не найдена.");
        }

        if (entry.IsOpen)
        {
            throw new InvalidTimeEntryException("Идущая запись переименовывается на экране таймера.");
        }

        await _repository.UpdateAsync(entry.Rename(name), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
