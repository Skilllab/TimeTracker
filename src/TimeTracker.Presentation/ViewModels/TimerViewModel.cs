using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeTracker.Application;
using TimeTracker.Domain;
using TimeTracker.Presentation.Shell;

namespace TimeTracker.Presentation.ViewModels;

/// <summary>
/// ViewModel экрана таймера: управляет записью, показывает счетчик и выбор проекта.
/// </summary>
public sealed partial class TimerViewModel : ObservableObject
{
    private readonly ITimerControl _timerControl;
    private readonly IProjectList _projectList;
    private readonly LocalizationManager _localizationManager;
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

        _ = LoadProjectsAsync();
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
            await _timerControl.Start(SelectedProject?.Id);
        }

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

        OnPropertyChanged(nameof(ToggleCaption));
        OnPropertyChanged(nameof(CanSelectProject));

        RefreshCounter();
    }

    /// <summary>
    /// Перечитывает проекты, доступные для выбора.
    /// </summary>
    private async Task LoadProjectsAsync()
    {
        var projects = await _projectList.GetAvailableAsync();

        Projects.Clear();

        foreach (var project in projects)
        {
            Projects.Add(project);
        }
    }

    /// <summary>
    /// Перерисовывает надпись кнопки после смены языка.
    /// </summary>
    private void OnLanguageChanged(object? sender, EventArgs e) => OnPropertyChanged(nameof(ToggleCaption));
}
