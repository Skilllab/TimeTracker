using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Mono.TextTemplating;
using TimeTracker.Application.Abstractions.Persistence;
using TimeTracker.Application.TimeTracking;
using TimeTracker.Domain.Projects;
using TimeTracker.Wpf.Dialogs;
using TimeTracker.Wpf.Mvvm;

namespace TimeTracker.Wpf.ViewModels;

/// <summary>
/// ViewModel панели трекинга — главный элемент экрана.
///
/// Управляет ITimerService:
///   - Start/Pause/Resume/Stop через команды;
///   - обновляет DisplayTime при Tick;
///   - переключает видимость кнопок через State;
///   - загружает список проектов для ComboBox.
///
/// Событие EntryStopped поднимается после StopTimerAsync —
/// MainWindowViewModel ловит его и обновляет список записей.
/// </summary>
public sealed partial class TimerPanelViewModel : ViewModelBase, IDisposable
{
    private readonly ITimerService _timerService;
    private readonly IProjectRepository _projectRepository;
    private readonly IDialogService _dialogService;
    private readonly ILogger<TimerPanelViewModel> _logger;

    /// <summary>Поднимается после успешной остановки записи.</summary>
    public event EventHandler? EntryStopped;

    public ObservableCollection<ProjectViewModel> Projects { get; } = [];

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private ProjectViewModel? _selectedProject;

    [ObservableProperty]
    private string _displayTime = "00:00";

    [ObservableProperty]
    private TimerState _state = TimerState.Idle;

    public TimerPanelViewModel(
        ITimerService timerService,
        IProjectRepository projectRepository,
        IDialogService dialogService,
        ILogger<TimerPanelViewModel> logger)
    {
        _timerService = timerService;
        _projectRepository = projectRepository;
        _dialogService = dialogService;
        _logger = logger;

        _timerService.Tick += OnTick;
        _timerService.StateChanged += OnStateChanged;

        State = _timerService.State;
        DisplayTime = _timerService.CurrentDuration.ToHumanReadable();
    }

    /// <summary>
    /// Загрузить список активных проектов. Вызывается при инициализации.
    /// </summary>
    public async Task InitializeAsync()
    {
        var projects = await _projectRepository.GetActiveAsync();

        Projects.Clear();
        foreach (var project in projects)
            Projects.Add(ProjectViewModel.FromDomain(project));
    }

    [RelayCommand(CanExecute = nameof(CanStart))]
    private async Task StartAsync()
    {
        try
        {
            await _timerService.StartTimerAsync(Description, SelectedProject?.Id);
            Description = string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start timer");
            _dialogService.ShowError(ex.Message);
        }
    }

    private bool CanStart() => State == TimerState.Idle;

    [RelayCommand(CanExecute = nameof(CanPause))]
    private void Pause()
    {
        _timerService.PauseTimer();
    }

    private bool CanPause() => State == TimerState.Running;

    [RelayCommand(CanExecute = nameof(CanResume))]
    private void Resume()
    {
        _timerService.ResumeTimer();
    }

    private bool CanResume() => State == TimerState.Paused;

    [RelayCommand(CanExecute = nameof(CanStop))]
    private async Task StopAsync()
    {
        try
        {
            await _timerService.StopTimerAsync();
            EntryStopped?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop timer");
            _dialogService.ShowError(ex.Message);
        }
    }

    private bool CanStop() => State != TimerState.Idle;

    private void OnTick(object? sender, TimerTickEventArgs e)
    {
        // Tick поднимается с фонового потока — маршалим на UI.
        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
        {
            DisplayTime = e.Duration.ToHumanReadable();
        });
    }

    private void OnStateChanged(object? sender, TimerStateChangedEventArgs e)
    {
        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
        {
            State = e.NewState;
            StartCommand.NotifyCanExecuteChanged();
            PauseCommand.NotifyCanExecuteChanged();
            ResumeCommand.NotifyCanExecuteChanged();
            StopCommand.NotifyCanExecuteChanged();
        });
    }

    public void Dispose()
    {
        _timerService.Tick -= OnTick;
        _timerService.StateChanged -= OnStateChanged;
    }
}
