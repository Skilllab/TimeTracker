using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeTracker.Application;
using TimeTracker.Domain;

namespace TimeTracker.Presentation.ViewModels;

/// <summary>
/// ViewModel главного окна: управляет записью, показывает счетчик и список за сегодня.
/// </summary>
public sealed partial class MainWindowViewModel : ObservableObject
{
    private readonly ITimerControl _timerControl;
    private readonly ITimeEntryList _entryList;
    private readonly DispatcherTimer _ticker = new() { Interval = TimeSpan.FromSeconds(1) };

    [ObservableProperty]
    private string _counter = "00:00";

    /// <summary>
    /// Создает ViewModel главного окна.
    /// </summary>
    /// <param name="timerControl">Входящий порт управления записью.</param>
    /// <param name="entryList">Входящий порт списка записей.</param>
    public MainWindowViewModel(ITimerControl timerControl, ITimeEntryList entryList)
    {
        _timerControl = timerControl ?? throw new ArgumentNullException(nameof(timerControl));
        _entryList = entryList ?? throw new ArgumentNullException(nameof(entryList));

        _ticker.Tick += OnTick;
        _ticker.Start();

        RefreshCounter();

        _ = RefreshEntriesAsync();
    }

    /// <summary>
    /// Записи времени за сегодня.
    /// </summary>
    public ObservableCollection<TimeEntry> Entries { get; } = new();

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
    /// Завершает запись и обновляет список.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanFinish))]
    private async Task Finish()
    {
        await _timerControl.Stop();

        await RefreshEntriesAsync();

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
    /// Перечитывает записи за сегодня.
    /// </summary>
    private async Task RefreshEntriesAsync()
    {
        var entries = await _entryList.GetTodayAsync();

        Entries.Clear();

        foreach (var entry in entries)
        {
            Entries.Add(entry);
        }
    }
}
