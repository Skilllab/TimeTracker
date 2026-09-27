using Microsoft.EntityFrameworkCore;
using TimeTracker.Application;

namespace TimeTracker.Infrastructure;

/// <summary>
/// Подсказка имен тегов по уже встречавшимся значениям.
/// Справочника тегов нет, поэтому имена собираются из самих задач.
/// </summary>
public sealed class EfTaskTagSuggestions : ITaskTagSuggestions
{
    private readonly TimeTrackerDbContext _context;

    /// <summary>
    /// Создает источник подсказок.
    /// </summary>
    /// <param name="context">Контекст базы данных.</param>
    public EfTaskTagSuggestions(TimeTrackerDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Возвращает имена тегов, начинающиеся с указанного текста.
    /// Повторы убираются без учета регистра, порядок алфавитный.
    /// </summary>
    /// <param name="prefix">Начало имени тега; <c>null</c> или пустая строка — все встречавшиеся имена.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public async Task<IReadOnlyList<string>> SuggestAsync(
        string? prefix,
        CancellationToken cancellationToken = default)
    {
        var tasks = await _context.Tasks.AsNoTracking().ToListAsync(cancellationToken);
        var filter = (prefix ?? string.Empty).Trim();

        return tasks
            .SelectMany(task => task.TagNames)
            .Distinct(StringComparer.CurrentCultureIgnoreCase)
            .Where(name => filter.Length == 0
                || name.StartsWith(filter, StringComparison.CurrentCultureIgnoreCase))
            .OrderBy(name => name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }
}
