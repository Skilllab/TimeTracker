using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TimeTracker.Application.Abstractions.Persistence;
using TimeTracker.Domain.Projects;
using TimeTracker.Domain.TimeTracking;

namespace TimeTracker.Application.TimeTracking;

/// <summary>
/// Реализация ITimerService
/// Почему partial:
///   Source-generated logging через [LoggerMessage] требует, чтобы
///   класс был partial — генератор добавляет реализацию partial-методов
///   с проверкой IsEnabled и вызовом Logger.Log. Без partial
///   компилятор не даст объявить метод как partial void.
/// </summary>
public sealed partial class TimerService : ITimerService, IDisposable
{
    private readonly TimeProvider _timeProvider;
    private readonly ITimeEntryRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TimerService> _logger;
    private readonly ITimer _tickTimer;

    private TimeEntry? _currentEntry;
    private TimerState _state = TimerState.Idle;
    private TimeSpan _pausedAccumulated;
    private DateTimeOffset? _pauseStartedAt;

    public TimerState State => _state;
    public TimeEntry? CurrentEntry => _currentEntry;

    public Duration CurrentDuration
    {
        get
        {
            if (_currentEntry is null)
                return Duration.Zero;

            var now = _timeProvider.GetUtcNow();
            var elapsed = now - _currentEntry.Range.Start - _pausedAccumulated;

            if (_state == TimerState.Paused && _pauseStartedAt is not null)
                elapsed -= now - _pauseStartedAt.Value;

            return elapsed <= TimeSpan.Zero
                ? Duration.Zero
                : Duration.FromTimeSpan(elapsed);
        }
    }

    public event EventHandler<TimerTickEventArgs>? Tick;
    public event EventHandler<TimerStateChangedEventArgs>? StateChanged;

    public TimerService(
        TimeProvider timeProvider,
        ITimeEntryRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<TimerService> logger)
    {
        _timeProvider = timeProvider;
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;

        _tickTimer = _timeProvider.CreateTimer(
            callback: OnTick,
            state: null,
            dueTime: Timeout.InfiniteTimeSpan,
            period: Timeout.InfiniteTimeSpan);
    }

    public async Task RestoreTimerAsync(CancellationToken ct = default)
    {
        var running = await _repository.GetRunningAsync(ct);
        if (running is null)
        {
            LogNoRunningEntryToRestore();
            return;
        }

        _currentEntry = running;
        // TransitionTo(Running) сам запустит тик-цикл.
        TransitionTo(TimerState.Running);

        LogRestoredEntry(running.Id, running.Range.Start);
    }

    public async Task StartTimerAsync(
        string? description,
        ProjectId? projectId,
        CancellationToken ct = default)
    {
        if (_state != TimerState.Idle)
            throw new InvalidOperationException($"Cannot start: state is {_state}, expected Idle.");

        var entry = TimeEntry.StartNew(description, projectId, _timeProvider.GetUtcNow());
        await _repository.AddAsync(entry, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _currentEntry = entry;
        _pausedAccumulated = TimeSpan.Zero;
        _pauseStartedAt = null;
        // TransitionTo(Running) сам запустит тик-цикл.
        TransitionTo(TimerState.Running);

        LogStartedEntry(entry.Id);
    }

    public async Task StopTimerAsync(CancellationToken ct = default)
    {
        if (_state == TimerState.Idle)
            throw new InvalidOperationException("Cannot stop: state is Idle.");

        if (_currentEntry is null)
            throw new InvalidOperationException("Cannot stop: no current entry.");

        if (_state == TimerState.Paused && _pauseStartedAt is not null)
        {
            _pausedAccumulated += _timeProvider.GetUtcNow() - _pauseStartedAt.Value;
            _pauseStartedAt = null;
        }

        var now = _timeProvider.GetUtcNow();
        _currentEntry.Stop(now);
        await _unitOfWork.SaveChangesAsync(ct);

        var stoppedEntry = _currentEntry;

        _currentEntry = null;
        _pausedAccumulated = TimeSpan.Zero;
        _pauseStartedAt = null;
        // TransitionTo(Idle) сам остановит тик-цикл.
        TransitionTo(TimerState.Idle);

        LogStoppedEntry(stoppedEntry.Id, stoppedEntry.Range.Duration);
    }

    public void PauseTimer()
    {
        if (_state != TimerState.Running)
            throw new InvalidOperationException($"Cannot pause: state is {_state}, expected Running.");

        _pauseStartedAt = _timeProvider.GetUtcNow();
        // TransitionTo(Paused) сам остановит тик-цикл.
        TransitionTo(TimerState.Paused);

        LogPausedEntry(_currentEntry?.Id);
    }

    public void ResumeTimer()
    {
        if (_state != TimerState.Paused)
            throw new InvalidOperationException($"Cannot resume: state is {_state}, expected Paused.");

        if (_pauseStartedAt is not null)
        {
            _pausedAccumulated += _timeProvider.GetUtcNow() - _pauseStartedAt.Value;
            _pauseStartedAt = null;
        }

        // TransitionTo(Running) сам запустит тик-цикл.
        TransitionTo(TimerState.Running);

        LogResumedEntry(_currentEntry?.Id);
    }

    private void OnTick(object? state)
    {
        if (_state != TimerState.Running)
            return;
        Tick?.Invoke(this, new TimerTickEventArgs(CurrentDuration));
    }

    /// <summary>
    /// Единая точка переходов стейт-машины.
    ///
    /// Управляет тик-циклом: включает его при переходе в Running,
    /// выключает при переходе в Paused и Idle. Это гарантирует,
    /// что StartTickLoop/StopTickLoop не забудутся в одном из методов.
    ///
    /// Все публичные методы (StartTimerAsync, StopTimerAsync, PauseTimer,
    /// ResumeTimer, RestoreTimerAsync) вызывают TransitionTo и не
    /// трогают тик-цикл напрямую.
    /// </summary>
    private void TransitionTo(TimerState newState)
    {
        var oldState = _state;
        _state = newState;

        switch (newState)
        {
            case TimerState.Running:
                _tickTimer.Change(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
                break;
            case TimerState.Paused:
            case TimerState.Idle:
                _tickTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
                break;
        }

        StateChanged?.Invoke(this, new TimerStateChangedEventArgs(oldState, newState));
    }

    public void Dispose() => _tickTimer.Dispose();


    [LoggerMessage(Level = LogLevel.Information, Message = "Нет запущенных таймеров для восстановления")]
    private partial void LogNoRunningEntryToRestore();

    [LoggerMessage(Level = LogLevel.Information, Message = "Восстановлен запущенный таймер {EntryId}, начатый в {Start}")]
    private partial void LogRestoredEntry(TimeEntryId entryId, DateTimeOffset start);

    [LoggerMessage(Level = LogLevel.Information, Message = "Запущенный таймер {EntryId}")]
    private partial void LogStartedEntry(TimeEntryId entryId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Остановленный таймер {EntryId}, длительностью {Duration}")]
    private partial void LogStoppedEntry(TimeEntryId entryId, Duration duration);

    [LoggerMessage(Level = LogLevel.Information, Message = "Приостановлен таймер {EntryId}")]
    private partial void LogPausedEntry(TimeEntryId? entryId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Возобновлен таймер {EntryId}")]
    private partial void LogResumedEntry(TimeEntryId? entryId);
}
