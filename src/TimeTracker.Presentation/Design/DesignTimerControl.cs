using System;
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
    /// Начинает новую запись.
    /// </summary>
    public void Start()
    {
    }

    /// <summary>
    /// Останавливает активную запись.
    /// </summary>
    public void Stop()
    {
    }

    /// <summary>
    /// Возвращает длительность текущей записи к текущему моменту.
    /// </summary>
    public Duration GetElapsed() => Sample;
}
