using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeTracker.Application;
using TimeTracker.Domain;
using TimeTracker.Presentation.Shell;
using TimeTracker.Presentation.Views;

namespace TimeTracker.Presentation.ViewModels;

/// <summary>
/// ViewModel экрана таймера: управляет записью, показывает счетчик и выбор проекта.
/// </summary>
public sealed partial class TimerViewModel : ObservableObject
{
    private readonly ITimerControl _timerControl;
    private readonly IProjectList _projectList;
    private readonly LocalizationManager _localizationManager;
    private Func<string, EntryNameWindow> _nameWindowFactory = null!;
    private Control _owner = null!;
    private readonly DispatcherTimer _ticker = new() { Interval = TimeSpan.FromSeconds(1) };

    [ObservableProperty]
    private string _counter = "00:00";

    [ObservableProperty]
    private Project? _selectedProject;

    /// <summary>
    /// Создает экран таймера.
    /// </summary>
    /// <param name="timerControl">Входящий порт управления записью.</param>
    /// <param name="projectList">Входящий порт списка проектов.</param>
    /// <param name="localizationManager">Управление языком.</param>
    public TimerViewModel(
        ITimerControl timerControl,
        IProjectList projectList,
        LocalizationManager localizationManager)
    {
        _timerControl = timerControl ?? throw new ArgumentNullException(nameof(timerControl));
        _projectList = projectList ?? throw new ArgumentNullException(nameof(projectList));
        _localizationManager = localizationManager ?? throw new ArgumentNullException(nameof(localizationManager));

        _localizationManager.Changed += OnLanguageChanged;

        _ticker.Tick += OnTick;
        _ticker.Start();

        RefreshCounter();

        _ = RefreshProjectsAsync();
    }

    /// <summary>
    /// Передает модели фабрику окна имени и владельца окна.
    /// Окно создается кодом разметки: показ окна — задача представления,
    /// а владелец берется из визуального дерева, поэтому окно открывается модально.
    /// </summary>
    /// <param name="factory">Фабрика окна имени задачи.</param>
    /// <param name="owner">Элемент управления, по которому ищется владелец окна.</param>
    public void AttachNameWindowFactory(Func<string, EntryNameWindow> factory, Control owner)
    {
        _nameWindowFactory = factory ?? throw new ArgumentNullException(nameof(factory));
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
    }

    /// <summary>
    /// Проекты, доступные для выбора.
    /// </summary>
    public ObservableCollection<Project> Projects { get; } = new();

    /// <summary>
    /// Признак того, что проект можно выбрать: во время записи выбор зафиксирован.
    /// </summary>
    public bool CanSelectProject => !_timerControl.IsRunning && !_timerControl.IsPaused;

    /// <summary>
    /// Надпись на кнопке переключения: называет действие, доступное в текущем состоянии.
    /// </summary>
    public string ToggleCaption => _timerControl.IsRunning
        ? _localizationManager["Timer.Pause"]
        : _timerControl.IsPaused
            ? _localizationManager["Timer.Resume"]
            : _localizationManager["Timer.Start"];

    /// <summary>
    /// Запускает, приостанавливает или возобновляет запись в зависимости от состояния.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanToggle))]
    private async Task Toggle()
    {
        if (_timerControl.IsRunning)
        {
            await _timerControl.Pause();
        }
        else if (_timerControl.IsPaused)
        {
            await _timerControl.Resume();
        }
        else
        {
            await StartAsync();

            return;
        }

        RefreshState();
    }

    /// <summary>
    /// Переименовывает идущую запись.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanRename))]
    private async Task Rename()
    {
        var name = await RequestNameAsync(TaskName);

        if (name is null)
        {
            return;
        }

        await _timerControl.Rename(name);

        TaskName = name;
    }

    /// <summary>
    /// Разрешает переименование, пока запись идет или стоит на паузе.
    /// </summary>
    private bool CanRename() => _timerControl.IsRunning || _timerControl.IsPaused;

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


    /// <summary>
    /// Открывает окно имени задачи и запускает запись.
    /// Запись начинается только после подтверждения имени.
    /// </summary>
    private async Task StartAsync()
    {
        var name = await RequestNameAsync(TaskName);

        if (name is null)
        {
            return;
        }

        await _timerControl.Start(name, SelectedProject?.Id);

        TaskName = name;

        RefreshState();
    }

    /// <summary>
    /// Завершает запись.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanFinish))]
    private async Task Finish()
    {
        await _timerControl.Stop();

        RefreshState();
    }

    /// <summary>
    /// Признак того, что задача завершена и можно начать следующую.
    /// </summary>
    public bool CanStartNewTask => _timerControl.IsFinished;

    /// <summary>
    /// Возвращает таймер в исходное состояние, чтобы начать следующую задачу.
    /// Завершенная запись при этом не меняется: она остается в списке за сегодня.
    /// </summary>
    [RelayCommand(CanExecute = nameof(IsFinishedState))]
    private async Task NewTask()
    {
        await _timerControl.NewTask();

        RefreshState();
    }

    /// <summary>
    /// Разрешает переход к следующей задаче только после завершения текущей.
    /// </summary>
    private bool IsFinishedState() => CanStartNewTask;

    [ObservableProperty]
    private string _taskName = string.Empty;

    /// <summary>
    /// Разрешает переключение, пока запись не завершена.
    /// </summary>
    private bool CanToggle() => !_timerControl.IsFinished;

    /// <summary>
    /// Разрешает завершение идущей или приостановленной записи.
    /// </summary>
    private bool CanFinish() => _timerControl.IsRunning || _timerControl.IsPaused;

    /// <summary>
    /// Перерисовывает счетчик раз в секунду.
    /// </summary>
    private void OnTick(object? sender, EventArgs e) => RefreshCounter();

    /// <summary>
    /// Берет длительность у входящего порта и форматирует ее как MM:SS.
    /// </summary>
    private void RefreshCounter() => Counter = _timerControl.GetElapsed().ToClockString();

    /// <summary>
    /// Обновляет надписи и доступность команд после смены состояния.
    /// </summary>
    private void RefreshState()
    {
        ToggleCommand.NotifyCanExecuteChanged();
        FinishCommand.NotifyCanExecuteChanged();
        RenameCommand.NotifyCanExecuteChanged();
        NewTaskCommand.NotifyCanExecuteChanged();

        OnPropertyChanged(nameof(ToggleCaption));
        OnPropertyChanged(nameof(CanSelectProject));
        OnPropertyChanged(nameof(CanStartNewTask));

        RefreshCounter();
    }

    /// <summary>
    /// Перечитывает проекты, доступные для выбора.
    /// Выбранный проект сохраняется: обновление списка не должно сбрасывать
    /// выбор пользователя. Если проект исчез из списка, например ушел в архив,
    /// выбор очищается.
    /// </summary>
    public async Task RefreshProjectsAsync()
    {
        var selectedId = SelectedProject?.Id;
        var projects = await _projectList.GetAvailableAsync();

        Projects.Clear();

        foreach (var project in projects)
        {
            Projects.Add(project);
        }

        if (selectedId is Guid id)
        {
            SelectedProject = Projects.FirstOrDefault(project => project.Id == id);
        }
    }

    /// <summary>
    /// Перерисовывает надпись кнопки после смены языка.
    /// </summary>
    private void OnLanguageChanged(object? sender, EventArgs e) => OnPropertyChanged(nameof(ToggleCaption));
}
