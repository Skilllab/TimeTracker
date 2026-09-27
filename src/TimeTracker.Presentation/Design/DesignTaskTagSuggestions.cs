using TimeTracker.Application;

namespace TimeTracker.Presentation.Design;

/// <summary>
/// Заглушка входящего порта подсказки тегов для дизайнера XAML: подсказок нет.
/// </summary>
internal sealed class DesignTaskTagSuggestions : ITaskTagSuggestions
{
    /// <summary>
    /// Возвращает пустой список подсказок.
    /// </summary>
    /// <param name="prefix">Начало имени тега; <c>null</c> или пустая строка — все имена.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public Task<IReadOnlyList<string>> SuggestAsync(string? prefix, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
}
