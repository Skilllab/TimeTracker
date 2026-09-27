using TimeTracker.Application;
using TimeTracker.Domain;
using DomainTaskStatus = TimeTracker.Domain.TaskStatus;

namespace TimeTracker.Presentation.Design;

/// <summary>
/// Заглушка входящего порта списка задач для дизайнера XAML: показывает образцы плашек.
/// </summary>
internal sealed class DesignTaskList : ITaskList
{
    /// <summary>Момент, от которого считаются образцы.</summary>
    private static readonly DateTimeOffset Start = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// Возвращает образцы задач для предпросмотра.
    /// Фильтры не применяются: предпросмотру нужен заполненный список.
    /// </summary>
    /// <param name="search">Строка минипоиска по наименованию; в заглушке не используется.</param>
    /// <param name="tagSearch">Строка минипоиска по тегам; в заглушке не используется.</param>
    /// <param name="projectId">Идентификатор проекта; в заглушке не используется.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    /// <param name="includeDeleted">Признак того, что удаленные задачи тоже нужны; в заглушке не используется.</param>
    public Task<IReadOnlyList<TaskListItem>> GetAsync(
        string? search,
        string? tagSearch,
        Guid? projectId,
        CancellationToken cancellationToken = default,
        bool includeDeleted = false)
    {
        IReadOnlyList<TaskListItem> items = new[]
        {
            new TaskListItem(
                Guid.NewGuid(),
                "Работа над отчетом",
                DesignProjectList.SampleProjectId,
                "Внутренние работы",
                ProjectPalette.Colors[0],
                DomainTaskStatus.Running,
                Duration.From(TimeSpan.FromMinutes(123)),
                Start,
                Start,
                Start,
                null,
                "Отчеты",
                false),
            new TaskListItem(
                Guid.NewGuid(),
                "Созвон с клиентом",
                DesignProjectList.SampleProjectId,
                "Внутренние работы",
                ProjectPalette.Colors[0],
                DomainTaskStatus.Paused,
                Duration.From(TimeSpan.FromMinutes(45)),
                Start.AddDays(-1),
                Start.AddDays(-1),
                Start.AddHours(-2),
                null,
                string.Empty,
                false),
            new TaskListItem(
                Guid.NewGuid(),
                "Разбор требований",
                null,
                string.Empty,
                string.Empty,
                DomainTaskStatus.Finished,
                Duration.From(TimeSpan.FromMinutes(30)),
                Start.AddDays(-2),
                Start.AddDays(-2),
                Start.AddDays(-2),
                Start.AddDays(-2),
                "Анализ|Требования",
                false)
        };

        return Task.FromResult(items);
    }
}
