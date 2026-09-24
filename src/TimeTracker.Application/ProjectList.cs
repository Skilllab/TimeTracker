using TimeTracker.Domain;

namespace TimeTracker.Application;

/// <summary>
/// Сценарий чтения списка: читает проекты и упорядочивает их по имени.
/// </summary>
public sealed class ProjectList : IProjectList
{
    private readonly IProjectRepository _repository;

    /// <summary>
    /// Создает сценарий.
    /// </summary>
    /// <param name="repository">Исходящий порт хранилища проектов.</param>
    public ProjectList(IProjectRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>
    /// Возвращает проекты в порядке, пригодном для показа.
    /// Порядок задается здесь, а не в запросе: сравнение с учетом языка дает
    /// естественный алфавитный порядок для русских имен, тогда как сортировка
    /// средствами хранилища сравнивает строки побайтово.
    /// </summary>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public async Task<IReadOnlyList<Project>> GetAvailableAsync(CancellationToken cancellationToken = default)
    {
        var projects = await _repository.GetAllAsync(cancellationToken);

        return projects
            .OrderBy(project => project.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }
}
