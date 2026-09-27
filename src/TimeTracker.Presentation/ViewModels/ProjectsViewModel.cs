using System.Collections.ObjectModel;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeTracker.Application;
using TimeTracker.Domain;

namespace TimeTracker.Presentation.ViewModels;

/// <summary>
/// ViewModel экрана проектов: показывает список и открывает диалог редактора.
/// Архивации нет: все проекты доступны для выбора.
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
    /// Цвет, выбранный в пикере; синхронизирован со строкой цвета.
    /// </summary>
    [ObservableProperty]
    private Color _pickerColor;

    /// <summary>
    /// Признак программного изменения цвета: защищает синхронизацию от повторного входа.
    /// </summary>
    private bool _syncingColor;

    /// <summary>
    /// Текст ошибки редактора; пустая строка, если ошибки нет.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string _editorError = string.Empty;

    /// <summary>
    /// Признак того, что есть текст ошибки редактора.
    /// </summary>
    public bool HasError => EditorError.Length > 0;

    /// <summary>
    /// Проект, который правится; <c>null</c> при создании.
    /// </summary>
    /// <summary>
    /// Переносит введенную строку цвета в пикер.
    /// Неверная запись оставляет пикер без изменений: сообщение покажет сохранение.
    /// </summary>
    /// <param name="value">Строка цвета в виде #RRGGBB.</param>
    partial void OnEditorColorChanged(string value)
    {
        if (_syncingColor || !ProjectPalette.IsValid(value))
        {
            return;
        }

        _syncingColor = true;

        try
        {
            PickerColor = Color.Parse(value);
        }
        finally
        {
            _syncingColor = false;
        }
    }

    /// <summary>
    /// Переносит выбранный в пикере цвет в строку цвета.
    /// </summary>
    /// <param name="value">Выбранный цвет.</param>
    partial void OnPickerColorChanged(Color value)
    {
        if (_syncingColor)
        {
            return;
        }

        _syncingColor = true;

        try
        {
            EditorColor = $"#{value.R:X2}{value.G:X2}{value.B:X2}";
        }
        finally
        {
            _syncingColor = false;
        }
    }

    private Project? _editedProject;

    /// <summary>
    /// Перечитывает список проектов.
    /// </summary>
    [RelayCommand]
    public async Task RefreshAsync()
    {
        var projects = await _projectList.GetAllAsync();

        Projects.Clear();

        foreach (var project in projects)
        {
            Projects.Add(project);
        }
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
        EditorColor = ProjectPalette.Colors[0];
        PickerColor = Color.Parse(EditorColor);
        EditorError = string.Empty;
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
        PickerColor = Color.Parse(EditorColor);
        EditorError = string.Empty;
        IsEditorOpen = true;
    }

    /// <summary>
    /// Сохраняет введенные значения и закрывает редактор.
    /// Цвет проверяется до вызова портов: неверная запись не сохраняется,
    /// а редактор остается открытым, поэтому значение можно исправить.
    /// </summary>
    [RelayCommand]
    private async Task Save()
    {
        if (!ProjectPalette.IsValid(EditorColor))
        {
            EditorError = "Цвет проекта должен быть записан в виде #RRGGBB.";

            return;
        }

        EditorError = string.Empty;

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
}
