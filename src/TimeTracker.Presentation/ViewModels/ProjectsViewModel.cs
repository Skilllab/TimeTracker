using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeTracker.Application;
using TimeTracker.Domain;

namespace TimeTracker.Presentation.ViewModels;

/// <summary>
/// ViewModel экрана проектов: показывает список, управляет фильтром и диалогом редактора.
/// </summary>
public sealed partial class ProjectsViewModel : ObservableObject
{
    private readonly IProjectList _projectList;
    private readonly IProjectEditor _projectEditor;

    /// <summary>
    /// Создает экран проектов.
    /// </summary>
    /// <param name="projectList">Входящий порт списка проектов.</param>
    /// <param name="projectEditor">Входящий порт управления проектами.</param>
    public ProjectsViewModel(IProjectList projectList, IProjectEditor projectEditor)
    {
        _projectList = projectList ?? throw new ArgumentNullException(nameof(projectList));
        _projectEditor = projectEditor ?? throw new ArgumentNullException(nameof(projectEditor));

        _ = RefreshAsync();
    }

    /// <summary>
    /// Проекты для показа.
    /// </summary>
    public ObservableCollection<Project> Projects { get; } = new();

    /// <summary>
    /// Цвета, доступные для выбора в редакторе.
    /// </summary>
    public IReadOnlyList<string> Palette { get; } = ProjectPalette.Colors;

    /// <summary>
    /// Признак того, что показываются и архивные проекты.
    /// </summary>
    [ObservableProperty]
    private bool _showArchived;

    /// <summary>
    /// Признак того, что открыт редактор проекта.
    /// </summary>
    [ObservableProperty]
    private bool _isEditorOpen;

    /// <summary>
    /// Заголовок редактора: создание или правка.
    /// </summary>
    [ObservableProperty]
    private string _editorTitle = string.Empty;

    /// <summary>
    /// Введенное имя проекта.
    /// </summary>
    [ObservableProperty]
    private string _editorName = string.Empty;

    /// <summary>
    /// Выбранный цвет проекта.
    /// </summary>
    [ObservableProperty]
    private string _editorColor = string.Empty;

    /// <summary>
    /// Проект, который правится; <c>null</c> при создании.
    /// </summary>
    private Project? _editedProject;

    /// <summary>
    /// Идентификатор проекта незавершенной записи; <c>null</c>, если записи нет.
    /// </summary>
    private Guid? _activeProjectId;

    /// <summary>
    /// Перечитывает список проектов и сведения о занятом проекте.
    /// </summary>
    [RelayCommand]
    public async Task RefreshAsync()
    {
        var projects = await _projectList.GetAllAsync(ShowArchived);
        _activeProjectId = await _projectList.GetActiveProjectIdAsync();

        Projects.Clear();

        foreach (var project in projects)
        {
            Projects.Add(project);
        }

        ArchiveCommand.NotifyCanExecuteChanged();
        UnarchiveCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// Открывает редактор для создания проекта.
    /// Новый проект получает первый цвет палитры.
    /// </summary>
    [RelayCommand]
    private void Create()
    {
        _editedProject = null;

        EditorTitle = "Создание проекта";
        EditorName = string.Empty;
        EditorColor = Palette[0];
        IsEditorOpen = true;
    }

    /// <summary>
    /// Открывает редактор для правки проекта.
    /// </summary>
    /// <param name="project">Правящийся проект.</param>
    [RelayCommand]
    private void Edit(Project? project)
    {
        if (project is null)
        {
            return;
        }

        _editedProject = project;

        EditorTitle = "Правка проекта";
        EditorName = project.Name;
        EditorColor = project.Color;
        IsEditorOpen = true;
    }

    /// <summary>
    /// Сохраняет введенные значения и закрывает редактор.
    /// </summary>
    [RelayCommand]
    private async Task Save()
    {
        if (_editedProject is null)
        {
            await _projectEditor.CreateAsync(EditorName, EditorColor);
        }
        else
        {
            if (_editedProject.Name != EditorName.Trim())
            {
                await _projectEditor.RenameAsync(_editedProject.Id, EditorName);
            }

            if (_editedProject.Color != EditorColor)
            {
                await _projectEditor.ChangeColorAsync(_editedProject.Id, EditorColor);
            }
        }

        IsEditorOpen = false;

        await RefreshAsync();
    }

    /// <summary>
    /// Закрывает редактор без сохранения.
    /// </summary>
    [RelayCommand]
    private void Cancel() => IsEditorOpen = false;

    /// <summary>
    /// Архивирует проект.
    /// </summary>
    /// <param name="project">Архивируемый проект.</param>
    [RelayCommand(CanExecute = nameof(CanArchive))]
    private async Task Archive(Project? project)
    {
        if (project is null)
        {
            return;
        }

        await _projectEditor.ArchiveAsync(project.Id);

        await RefreshAsync();
    }

    /// <summary>
    /// Возвращает проект из архива.
    /// </summary>
    /// <param name="project">Возвращаемый проект.</param>
    [RelayCommand(CanExecute = nameof(CanUnarchive))]
    private async Task Unarchive(Project? project)
    {
        if (project is null)
        {
            return;
        }

        await _projectEditor.UnarchiveAsync(project.Id);

        await RefreshAsync();
    }

    /// <summary>
    /// Разрешает архивацию действующего проекта, на котором не идет запись.
    /// Занятый проект архивировать нельзя: сначала нужно завершить запись.
    /// </summary>
    /// <param name="project">Проверяемый проект.</param>
    private bool CanArchive(Project? project) =>
        project is not null && !project.IsArchived && project.Id != _activeProjectId;

    /// <summary>
    /// Разрешает возврат из архива только для архивного проекта.
    /// </summary>
    /// <param name="project">Проверяемый проект.</param>
    private bool CanUnarchive(Project? project) => project is { IsArchived: true };

    partial void OnShowArchivedChanged(bool value)
    {
        _ = RefreshAsync();
    }
}
