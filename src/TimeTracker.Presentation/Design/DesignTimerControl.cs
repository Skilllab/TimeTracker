using System;
using System.Threading.Tasks;
using TimeTracker.Application;
using TimeTracker.Domain;

namespace TimeTracker.Presentation.Design;

/// <summary>
/// Заглушка входящего порта для дизайнера XAML: заполняет предпросмотр без запуска таймера.
/// </summary>
internal sealed class DesignTimerControl : ITimerControl
{
    /// <summary>Образец длительности для предпросмотра.</summary>
    private static readonly Duration Sample = Duration.From(TimeSpan.FromSeconds(65));

    /// <summary>
    /// Признак того, что запись идет.
    /// </summary>
    public bool IsRunning => true;

    /// <summary>
    /// Признак того, что запись приостановлена.
    /// </summary>
    public bool IsPaused => false;

    /// <summary>
    /// Признак того, что запись завершена и сохранена.
    /// </summary>
    public bool IsFinished => false;

    /// <summary>
    /// Запускает запись.
    /// </summary>
    public void Start()
    {
    }

    /// <summary>
    /// Приостанавливает идущую запись.
    /// </summary>
    public void Pause()
    {
    }

    /// <summary>
    /// Возобновляет приостановленную запись.
    /// </summary>
    public void Resume()
    {
    }

    /// <summary>
    /// Завершает запись.
    /// </summary>
    public Task Stop() => Task.CompletedTask;

    /// <summary>
    /// Возвращает длительность записи без времени пауз.
    /// </summary>
    public Duration GetElapsed() => Sample;
}
