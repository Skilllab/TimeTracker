namespace TimeTracker.Application;

/// <summary>
/// Входящий порт: подсказка имен тегов при вводе.
/// Справочника тегов больше нет, поэтому источником подсказок служат сами задачи:
/// порт возвращает имена, которые уже встречались у других задач.
/// </summary>
public interface ITaskTagSuggestions
{
    /// <summary>
    /// Возвращает имена тегов, начинающиеся с указанного текста.
    /// Пустой текст возвращает все встречавшиеся имена: так подсказка работает сразу при открытии поля.
    /// Повторы в результат не попадают, сравнение идет без учета регистра.
    /// </summary>
    /// <param name="prefix">Начало имени тега; <c>null</c> или пустая строка — все встречавшиеся имена.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task<IReadOnlyList<string>> SuggestAsync(string? prefix, CancellationToken cancellationToken = default);
}
