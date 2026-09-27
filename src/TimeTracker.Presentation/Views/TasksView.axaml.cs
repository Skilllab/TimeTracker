using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using TimeTracker.Application;
using TimeTracker.Domain;
using TimeTracker.Presentation.ViewModels;

namespace TimeTracker.Presentation.Views;

/// <summary>
/// Экран списка задач: плашки задач с кнопками управления и фильтры отбора.
/// </summary>
public partial class TasksView : UserControl
{
    /// <summary>
    /// Создает экран списка задач.
    /// </summary>
    public TasksView()
    {
        InitializeComponent();

        DataContextChanged += OnDataContextChanged;
    }

    /// <summary>
    /// Передает модели фабрики окон и владельца окна.
    /// </summary>
    /// <param name="sender">Источник события.</param>
    /// <param name="e">Данные события.</param>
    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is TasksViewModel viewModel)
        {
            viewModel.AttachWindowFactories(
                CreateNameWindow,
                (title, projects, selectedProjectId, taskId) => CreateProjectWindow(viewModel, title, projects, selectedProjectId, taskId),
                (title, taskTitle, taskId, assignedTags) => CreateTagsWindow(viewModel, title, taskTitle, taskId, assignedTags),
                (title, question) => new ConfirmWindow { DataContext = new ConfirmViewModel(title, question) },
                this);
        }
    }

    /// <summary>
    /// Открывает переименование задачи по двойному клику по ее имени.
    /// Одиночный клик ничего не делает: имя меняется только в отдельном окне.
    /// Удаленная задача не переименовывается, поэтому окно для нее не открывается.
    /// </summary>
    /// <param name="sender">Источник события: элемент с именем задачи.</param>
    /// <param name="e">Данные события.</param>
    private void OnTaskNameDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Control control
            && control.DataContext is TaskCardViewModel card
            && DataContext is TasksViewModel viewModel
            && card.CanRename)
        {
            viewModel.RenameTaskCommand.Execute(card);
        }
    }

    /// <summary>
    /// Создает окно тегов задачи.
    /// </summary>
    /// <param name="viewModel">Модель экрана списка задач.</param>
    /// <param name="title">Заголовок окна.</param>
    /// <param name="taskTitle">Наименование задачи.</param>
    /// <param name="taskId">Идентификатор задачи.</param>
    /// <param name="assignedTags">Теги, назначенные задаче.</param>
    private static TaskTagsWindow CreateTagsWindow(
        TasksViewModel viewModel,
        string title,
        string taskTitle,
        Guid taskId,
        IReadOnlyList<string> assignedTags)
    {
        return new TaskTagsWindow
        {
            DataContext = viewModel.CreateTaskTags(title, taskTitle, taskId, assignedTags)
        };
    }

    /// <summary>
    /// Создает окно имени задачи.
    /// Заголовок передает вызывающий, поэтому создание и переименование отличаются подписью.
    /// </summary>
    /// <param name="title">Заголовок окна.</param>
    /// <param name="current">Текущее имя задачи; пустая строка при создании новой.</param>
    private static EntryNameWindow CreateNameWindow(string title, string current)
    {
        return new EntryNameWindow
        {
            DataContext = new EntryNameViewModel(title, current)
        };
    }

    /// <summary>
    /// Создает окно выбора проекта задачи.
    /// </summary>
    /// <param name="viewModel">Модель экрана списка задач.</param>
    /// <param name="title">Заголовок окна.</param>
    /// <param name="projects">Проекты, доступные для выбора.</param>
    /// <param name="selectedProjectId">Идентификатор текущего проекта; <c>null</c>, если проекта нет.</param>
    /// <param name="taskId">Идентификатор задачи, у которой меняется проект.</param>
    private static ProjectPickerWindow CreateProjectWindow(
        TasksViewModel viewModel,
        string title,
        IReadOnlyList<Project> projects,
        Guid? selectedProjectId,
        Guid taskId)
    {
        return new ProjectPickerWindow
        {
            DataContext = viewModel.CreateProjectPicker(title, projects, selectedProjectId, taskId)
        };
    }
}
