using TimeTracker.Domain.Projects;

namespace TimeTracker.Wpf.ViewModels;

/// <summary>
/// ViewModel одного проекта для ComboBox выбора.
///
/// Легкая обертка над Project — хранит только то, что нужно UI:
/// Id, Name, Hex-цвет. Не тянет весь агрегат.
/// </summary>
public sealed class ProjectViewModel(ProjectId id, string name, string hexColor)
{
    public ProjectId Id { get; } = id;
    public string Name { get; } = name;
    public string HexColor { get; } = hexColor;

    /// <summary>
    /// Создать из доменного Project.
    /// </summary>
    /// <param name="project">Доменный проект</param>
    /// <returns>ViewModel проекта для UI</returns>
    public static ProjectViewModel FromDomain(Project project)
    {
        ArgumentNullException.ThrowIfNull(project);
        return new ProjectViewModel(project.Id, project.Name, project.Color.Hex);
    }
}
