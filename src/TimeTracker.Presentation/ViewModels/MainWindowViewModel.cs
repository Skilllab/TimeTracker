using System;
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
    /// Признак того, что запись идет.
    /// </summary>
    public bool IsRunning => _timerControl.IsRunning;

    /// <summary>
    /// Надпись на кнопке: называет действие, которое будет выполнено.
    /// </summary>
    public string ToggleCaption => _timerControl.IsRunning ? "Stop" : "Start";

    /// <summary>
    /// Запускает или останавливает запись в зависимости от текущего состояния.
    /// </summary>
    [RelayCommand]
    private void Toggle()
    {
        if (_timerControl.IsRunning)
        {
            _timerControl.Stop();
        }
        else
        {
            _timerControl.Start();
        }

        OnPropertyChanged(nameof(IsRunning));
        OnPropertyChanged(nameof(ToggleCaption));

        RefreshCounter();
    }

    /// <summary>
    /// Перерисовывает счетчик раз в секунду.
    /// </summary>
    private void OnTick(object? sender, EventArgs e) => RefreshCounter();

    /// <summary>
    /// Берет длительность у входящего порта и форматирует ее как MM:SS.
    /// </summary>
    private void RefreshCounter() => Counter = _timerControl.GetElapsed().ToClockString();
}
