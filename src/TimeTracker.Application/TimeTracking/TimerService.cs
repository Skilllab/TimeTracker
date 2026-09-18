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
        _state = TimerState.Running;
        StartTickLoop();

        LogRestoredEntry(running.Id, running.Range.Start);
        RaiseStateChanged(TimerState.Idle, TimerState.Running);
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

        StopTickLoop();

        var stoppedEntry = _currentEntry;
        var oldState = _state;

        _currentEntry = null;
        _pausedAccumulated = TimeSpan.Zero;
        _pauseStartedAt = null;
        _state = TimerState.Idle;

        LogStoppedEntry(stoppedEntry.Id, stoppedEntry.Range.Duration);
        RaiseStateChanged(oldState, TimerState.Idle);
    }

    public void PauseTimer()
    {
        if (_state != TimerState.Running)
            throw new InvalidOperationException($"Cannot pause: state is {_state}, expected Running.");

        _pauseStartedAt = _timeProvider.GetUtcNow();
        StopTickLoop();
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

        StartTickLoop();
        TransitionTo(TimerState.Running);

        LogResumedEntry(_currentEntry?.Id);
    }

    private void OnTick(object? state)
    {
        if (_state != TimerState.Running)
            return;
        Tick?.Invoke(this, new TimerTickEventArgs(CurrentDuration));
    }

    private void StartTickLoop() =>
        _tickTimer.Change(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));

    private void StopTickLoop() =>
        _tickTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

    private void TransitionTo(TimerState newState)
    {
        var oldState = _state;
        _state = newState;
        RaiseStateChanged(oldState, newState);
    }

    private void RaiseStateChanged(TimerState oldState, TimerState newState) =>
        StateChanged?.Invoke(this, new TimerStateChangedEventArgs(oldState, newState));

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
