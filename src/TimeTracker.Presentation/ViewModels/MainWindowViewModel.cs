using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeTracker.Application;

namespace TimeTracker.Presentation.ViewModels;

/// <summary>
/// ViewModel главного окна: управляет записью и показывает счетчик.
/// </summary>
public sealed partial class MainWindowViewModel : ObservableObject
{
    private readonly ITimerControl _timerControl;
    private readonly DispatcherTimer _ticker = new() { Interval = TimeSpan.FromSeconds(1) };

    [ObservableProperty]
    private string _counter = "00:00";

    /// <summary>
    /// Создает ViewModel главного окна.
    /// </summary>
    /// <param name="timerControl">Входящий порт управления записью.</param>
    public MainWindowViewModel(ITimerControl timerControl)
    {
        _timerControl = timerControl ?? throw new ArgumentNullException(nameof(timerControl));

        _ticker.Tick += OnTick;
        _ticker.Start();

        RefreshCounter();
    }

    /// <summary>
    /// Надпись на кнопке переключения: называет действие, которое будет выполнено.
    /// </summary>
    public string ToggleCaption => _timerControl.IsRunning ? "Pause"
        : _timerControl.IsPaused ? "Resume"
        : "Start";

    /// <summary>
    /// Запускает, приостанавливает или возобновляет запись в зависимости от состояния.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanToggle))]
    private void Toggle()
    {
        if (_timerControl.IsRunning)
        {
            _timerControl.Pause();
        }
        else if (_timerControl.IsPaused)
        {
            _timerControl.Resume();
        }
        else
        {
            _timerControl.Start();
        }

        RefreshState();
    }

    /// <summary>
    /// Завершает запись.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanFinish))]
    private void Finish()
    {
        _timerControl.Stop();

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
}
