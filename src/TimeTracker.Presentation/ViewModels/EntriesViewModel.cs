using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using TimeTracker.Application;
using TimeTracker.Domain;

namespace TimeTracker.Presentation.ViewModels;

/// <summary>
/// ViewModel экрана записей за сегодня.
/// </summary>
public sealed partial class EntriesViewModel
{
    private readonly ITimeEntryList _entryList;
    private readonly IProjectList _projectList;

    /// <summary>
    /// Создает экран записей.
    /// </summary>
    /// <param name="entryList">Входящий порт списка записей.</param>
    /// <param name="projectList">Входящий порт списка проектов.</param>
    public EntriesViewModel(ITimeEntryList entryList, IProjectList projectList)
    {
        _entryList = entryList ?? throw new ArgumentNullException(nameof(entryList));
        _projectList = projectList ?? throw new ArgumentNullException(nameof(projectList));

        _ = RefreshAsync();
    }

    /// <summary>
    /// Записи времени за сегодня.
    /// </summary>
    public ObservableCollection<EntryRowViewModel> Entries { get; } = new();

    /// <summary>
    /// Перечитывает записи за сегодня.
    /// Проекты читаются вместе с архивными: старая запись может ссылаться
    /// на архивный проект, и без него у строки исчезли бы имя и маркер.
    /// </summary>
    [RelayCommand]
    public async Task RefreshAsync()
    {
        var entries = await _entryList.GetTodayAsync();
        var projects = await _projectList.GetAllAsync(includeArchived: true);

        var projectsById = projects.ToDictionary(project => project.Id);

        Entries.Clear();

        foreach (var entry in entries)
        {
            Entries.Add(new EntryRowViewModel(entry, ResolveProject(entry, projectsById)));
        }
    }

    /// <summary>
    /// Находит проект записи по ссылке.
    /// </summary>
    /// <param name="entry">Запись времени.</param>
    /// <param name="projectsById">Проекты, разложенные по идентификатору.</param>
    private static Project? ResolveProject(TimeEntry entry, IReadOnlyDictionary<Guid, Project> projectsById)
    {
        if (entry.ProjectId is not Guid id)
        {
            return null;
        }

        projectsById.TryGetValue(id, out var project);

        return project;
    }
}
