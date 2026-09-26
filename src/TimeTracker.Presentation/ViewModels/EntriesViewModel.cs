using System.Collections.ObjectModel;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using TimeTracker.Application;
using TimeTracker.Domain;
using TimeTracker.Presentation.Views;

namespace TimeTracker.Presentation.ViewModels;

/// <summary>
/// ViewModel экрана записей за сегодня.
/// </summary>
public sealed partial class EntriesViewModel
{
    private readonly ITimeEntryList _entryList;
    private readonly IProjectList _projectList;
    private readonly ITimeEntryEditor _entryEditor;
    private Func<string, EntryNameWindow> _nameWindowFactory = null!;
    private Control _owner = null!;

    /// <summary>
    /// Создает экран записей.
    /// </summary>
    /// <param name="entryList">Входящий порт списка записей.</param>
    /// <param name="projectList">Входящий порт списка проектов.</param>
    /// <param name="entryEditor">Входящий порт управления записью.</param>
    public EntriesViewModel(ITimeEntryList entryList, IProjectList projectList, ITimeEntryEditor entryEditor)
    {
        _entryList = entryList ?? throw new ArgumentNullException(nameof(entryList));
        _projectList = projectList ?? throw new ArgumentNullException(nameof(projectList));
        _entryEditor = entryEditor ?? throw new ArgumentNullException(nameof(entryEditor));

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

    /// <summary>
    /// Открывает окно правки имени записи.
    /// </summary>
    /// <param name="row">Строка списка.</param>
    [RelayCommand(CanExecute = nameof(CanRename))]
    private async Task Rename(EntryRowViewModel? row)
    {
        if (row is null)
        {
            return;
        }

        var name = await RequestNameAsync(row.TaskName);

        if (name is null)
        {
            return;
        }

        await _entryEditor.RenameAsync(row.Entry.Id, name);

        await RefreshAsync();
    }

    /// <summary>
    /// Передает модели фабрику окна имени и владельца окна.
    /// </summary>
    /// <param name="factory">Фабрика окна имени задачи.</param>
    /// <param name="owner">Элемент управления, по которому ищется владелец окна.</param>
    public void AttachNameWindowFactory(Func<string, EntryNameWindow> factory, Control owner)
    {
        _nameWindowFactory = factory ?? throw new ArgumentNullException(nameof(factory));
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
    }

    /// <summary>
    /// Разрешает правку имени только у завершенной записи.
    /// </summary>
    /// <param name="row">Проверяемая строка списка.</param>
    private bool CanRename(EntryRowViewModel? row) => row is { CanRename: true };

    /// <summary>
    /// Запрашивает имя задачи в отдельном окне; <c>null</c>, если пользователь отказался.
    /// </summary>
    /// <param name="current">Текущее имя задачи.</param>
    private async Task<string?> RequestNameAsync(string current)
    {
        var window = _nameWindowFactory(current);

        if (TopLevel.GetTopLevel(_owner) is Window owner)
        {
            await window.ShowDialog(owner);
        }
        else
        {
            window.Show();
        }

        return window.DataContext is EntryNameViewModel model && model.IsConfirmed
            ? model.TrimmedName
            : null;
    }
}
