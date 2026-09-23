using System.Diagnostics.Metrics;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeTracker.Application;
using TimeTracker.Presentation.Shell;

namespace TimeTracker.Presentation.ViewModels;

/// <summary>
/// ViewModel экрана таймера: управляет записью и показывает счетчик.
/// </summary>
public sealed partial class TimerViewModel : ObservableObject
{
    private readonly ITimerControl _timerControl;
    private readonly LocalizationManager _localizationManager;
    private readonly DispatcherTimer _ticker = new() { Interval = TimeSpan.FromSeconds(1) };

    [ObservableProperty]
    private string _counter = "00:00";

    /// <summary>
    /// Создает экран таймера.
    /// </summary>
    /// <param name="timerControl">Входящий порт управления записью.</param>
    /// <param name="localizationManager">Управление языком.</param>
    public TimerViewModel(ITimerControl timerControl, LocalizationManager localizationManager)
    {
        _timerControl = timerControl ?? throw new ArgumentNullException(nameof(timerControl));
        _localizationManager = localizationManager ?? throw new ArgumentNullException(nameof(localizationManager));

        _localizationManager.Changed += OnLanguageChanged;

        _ticker.Tick += OnTick;
        _ticker.Start();

        RefreshCounter();
    }

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
            await _timerControl.Start();
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

        RefreshCounter();
    }

    /// <summary>
    /// Перерисовывает надпись кнопки после смены языка.
    /// </summary>
    private void OnLanguageChanged(object? sender, EventArgs e) => OnPropertyChanged(nameof(ToggleCaption));
}
