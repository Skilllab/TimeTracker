using TimeTracker.Domain;

namespace TimeTracker.Application;

/// <summary>
/// Сценарий чтения списка: отбирает проекты для выбора и для показа и сообщает занятый проект.
/// </summary>
public sealed class ProjectList : IProjectList
{
    private readonly IProjectRepository _repository;
    private readonly ITimeEntryRepository _entryRepository;

    /// <summary>
    /// Создает сценарий.
    /// </summary>
    /// <param name="repository">Исходящий порт хранилища проектов.</param>
    /// <param name="entryRepository">Исходящий порт хранилища записей.</param>
    public ProjectList(IProjectRepository repository, ITimeEntryRepository entryRepository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _entryRepository = entryRepository ?? throw new ArgumentNullException(nameof(entryRepository));
    }

    /// <summary>
    /// Возвращает действующие проекты в порядке, пригодном для выбора.
    /// Порядок задается здесь, а не в запросе: сравнение с учетом языка дает
    /// естественный алфавитный порядок для русских имен, тогда как сортировка
    /// средствами хранилища сравнивает строки побайтово.
    /// </summary>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public async Task<IReadOnlyList<Project>> GetAvailableAsync(CancellationToken cancellationToken = default)
    {
        var projects = await _repository.GetAllAsync(cancellationToken);

        return Order(projects.Where(project => !project.IsArchived));
    }

    /// <summary>
    /// Возвращает проекты для показа на экране проектов.
    /// </summary>
    /// <param name="includeArchived">Признак того, что архивные проекты тоже нужны.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public async Task<IReadOnlyList<Project>> GetAllAsync(bool includeArchived, CancellationToken cancellationToken = default)
    {
        var projects = await _repository.GetAllAsync(cancellationToken);

        return Order(includeArchived ? projects : projects.Where(project => !project.IsArchived));
    }

    /// <summary>
    /// Возвращает идентификатор проекта идущей записи.
    /// Занятым считается проект незавершенной записи, в том числе стоящей на паузе:
    /// архивация такого проекта запрещена, пока запись не завершена.
    /// </summary>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public async Task<Guid?> GetActiveProjectIdAsync(CancellationToken cancellationToken = default)
    {
        var active = await _entryRepository.GetActiveAsync(cancellationToken);

        return active?.ProjectId;
    }

    /// <summary>
    /// Упорядочивает проекты по имени с учетом языка.
    /// </summary>
    /// <param name="projects">Отобранные проекты.</param>
    private static IReadOnlyList<Project> Order(IEnumerable<Project> projects)
    {
        return projects
            .OrderBy(project => project.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }
}
