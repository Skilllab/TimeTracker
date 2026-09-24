using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using TimeTracker.Application;
using TimeTracker.Domain;

namespace TimeTracker.Presentation.ViewModels;

/// <summary>
/// ViewModel экрана проектов: показывает список проектов с цветными маркерами.
/// </summary>
public sealed partial class ProjectsViewModel
{
    private readonly IProjectList _projectList;

    /// <summary>
    /// Создает экран проектов.
    /// </summary>
    /// <param name="projectList">Входящий порт списка проектов.</param>
    public ProjectsViewModel(IProjectList projectList)
    {
        _projectList = projectList ?? throw new ArgumentNullException(nameof(projectList));

        _ = RefreshAsync();
    }

    /// <summary>
    /// Проекты для показа.
    /// </summary>
    public ObservableCollection<Project> Projects { get; } = new();

    /// <summary>
    /// Перечитывает список проектов.
    /// </summary>
    [RelayCommand]
    public async Task RefreshAsync()
    {
        var projects = await _projectList.GetAvailableAsync();

        Projects.Clear();

        foreach (var project in projects)
        {
            Projects.Add(project);
        }
    }
}
