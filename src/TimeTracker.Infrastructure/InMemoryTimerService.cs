using TimeTracker.Application;
using TimeTracker.Domain;

namespace TimeTracker.Infrastructure;

/// <summary>
/// Заглушка порта <see cref="ITimerService"/>: держит одну активную запись в памяти,
/// на диск ничего не пишет. Время берется только из <see cref="TimeProvider"/>.
/// </summary>
public sealed class InMemoryTimerService : ITimerService
{
    private readonly TimeProvider _timeProvider;

    /// <summary>Единственная активная запись. Открытый конец — сессия «идет».</summary>
    private readonly TimeRange _active;

    /// <summary>
    /// Создает заглушку и открывает активную запись с текущего момента.
    /// </summary>
    /// <param name="timeProvider">Источник времени.</param>
    public InMemoryTimerService(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _active = new TimeRange(_timeProvider.GetUtcNow());
    }

    /// <summary>
    /// Получить пройденное время.
    /// </summary>
    /// <returns></returns>
    public Duration GetElapsed() => _active.ElapsedAt(_timeProvider.GetUtcNow());
}
