using TimeTracker.Application;
using TimeTracker.Domain;

namespace TimeTracker.Presentation.Design;

/// <summary>
/// Заглушка входящего порта списка проектов для дизайнера XAML: заполняет предпросмотр.
/// </summary>
internal sealed class DesignProjectList : IProjectList
{
    /// <summary>
    /// Идентификатор первого образца проекта: на него ссылается образец записи.
    /// </summary>
    internal static readonly Guid SampleProjectId = new("4F1E7B8C-4C2A-4E63-9C2E-2B7C1A5D9E01");

    /// <summary>
    /// Возвращает образец проектов для предпросмотра.
    /// </summary>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public Task<IReadOnlyList<Project>> GetAvailableAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Project> projects = new List<Project>
        {
            new Project(SampleProjectId, "Внутренние задачи", "#2F6FED"),
            new Project(new Guid("6B2C8D9E-5D3B-4F74-8D3F-3C8D2B6E0F12"), "Учебный курс", "#2E7D32")
        };

        return Task.FromResult(projects);
    }
}
