using TimeTracker.Domain;

namespace TimeTracker.Application;

/// <summary>
/// Сценарий управления проектами: читает текущее состояние, применяет доменную операцию и сохраняет результат.
/// </summary>
public sealed class ProjectEditor : IProjectEditor
{
    private readonly IProjectRepository _repository;
    private readonly ITimeEntryRepository _entryRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Создает сценарий.
    /// </summary>
    /// <param name="repository">Исходящий порт хранилища проектов.</param>
    /// <param name="entryRepository">Исходящий порт хранилища записей.</param>
    /// <param name="unitOfWork">Исходящий порт фиксации изменений.</param>
    public ProjectEditor(
        IProjectRepository repository,
        ITimeEntryRepository entryRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _entryRepository = entryRepository ?? throw new ArgumentNullException(nameof(entryRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// Создает проект.
    /// </summary>
    /// <param name="name">Имя проекта.</param>
    /// <param name="color">Цвет маркера в виде #RRGGBB из набора палитры.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public async Task CreateAsync(string name, string color, CancellationToken cancellationToken = default)
    {
        var project = new Project(Guid.NewGuid(), name, color);

        await _repository.AddAsync(project, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Переименовывает проект.
    /// </summary>
    /// <param name="projectId">Идентификатор проекта.</param>
    /// <param name="name">Новое имя проекта.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public async Task RenameAsync(Guid projectId, string name, CancellationToken cancellationToken = default)
    {
        var project = await LoadAsync(projectId, cancellationToken);

        await _repository.UpdateAsync(project.Rename(name), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Меняет цвет маркера проекта.
    /// </summary>
    /// <param name="projectId">Идентификатор проекта.</param>
    /// <param name="color">Новый цвет маркера в виде #RRGGBB из набора палитры.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public async Task ChangeColorAsync(Guid projectId, string color, CancellationToken cancellationToken = default)
    {
        var project = await LoadAsync(projectId, cancellationToken);

        await _repository.UpdateAsync(project.ChangeColor(color), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Архивирует проект.
    /// Архивация занятого проекта запрещена: на нем идет незавершенная запись,
    /// и убрать его из выбора можно только после ее завершения.
    /// </summary>
    /// <param name="projectId">Идентификатор проекта.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public async Task ArchiveAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var active = await _entryRepository.GetActiveAsync(cancellationToken);

        if (active?.ProjectId == projectId)
        {
            throw new InvalidProjectException("Нельзя архивировать проект незавершенной записи.");
        }

        var project = await LoadAsync(projectId, cancellationToken);

        await _repository.UpdateAsync(project.Archive(), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Возвращает проект из архива.
    /// </summary>
    /// <param name="projectId">Идентификатор проекта.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public async Task UnarchiveAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var project = await LoadAsync(projectId, cancellationToken);

        await _repository.UpdateAsync(project.Unarchive(), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Читает проект по идентификатору.
    /// Отсутствие проекта — нарушение правила: правка несуществующей записи недопустима.
    /// </summary>
    /// <param name="projectId">Идентификатор проекта.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    private async Task<Project> LoadAsync(Guid projectId, CancellationToken cancellationToken)
    {
        var project = await _repository.GetByIdAsync(projectId, cancellationToken);

        if (project is null)
        {
            throw new InvalidProjectException("Проект не найден.");
        }

        return project;
    }
}
