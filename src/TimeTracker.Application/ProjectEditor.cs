using TimeTracker.Domain;

namespace TimeTracker.Application;

/// <summary>
/// Сценарий управления проектами: читает текущее состояние, применяет доменную операцию и сохраняет результат.
/// Архивации нет: проекты не скрываются из выбора, поэтому операций архива и возврата из архива не существует.
/// </summary>
public sealed class ProjectEditor : IProjectEditor
{
    private readonly IProjectRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Создает сценарий.
    /// </summary>
    /// <param name="repository">Исходящий порт хранилища проектов.</param>
    /// <param name="unitOfWork">Исходящий порт фиксации изменений.</param>
    public ProjectEditor(IProjectRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// Создает проект.
    /// </summary>
    /// <param name="name">Имя проекта.</param>
    /// <param name="color">Цвет маркера в виде #RRGGBB.</param>
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
    /// <param name="color">Новый цвет маркера в виде #RRGGBB.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public async Task ChangeColorAsync(Guid projectId, string color, CancellationToken cancellationToken = default)
    {
        var project = await LoadAsync(projectId, cancellationToken);

        await _repository.UpdateAsync(project.ChangeColor(color), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Удаляет проект без возможности восстановления.
    /// Задачи и записи времени остаются: ссылка на проект снимается настройкой связи.
    /// </summary>
    /// <param name="projectId">Идентификатор проекта.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public async Task DeleteAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var project = await LoadAsync(projectId, cancellationToken);

        await _repository.DeleteAsync(project, cancellationToken);
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
