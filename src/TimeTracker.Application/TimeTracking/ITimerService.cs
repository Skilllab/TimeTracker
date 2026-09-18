using TimeTracker.Domain.Projects;
using TimeTracker.Domain.TimeTracking;

namespace TimeTracker.Application.TimeTracking;

/// <summary>
/// Сервис трекинга времени.
///
/// Управляет ровно одной активной записью. Гарантирует, что
/// одновременно в БД не может быть больше одной running-записи:
/// попытка StartAsync при State != Idle бросает исключение.
///
/// Публикует события Tick (раз в секунду) и StateChanged (при
/// переходах стейт-машины). UI (этап 5) подписывается на них,
/// TimerService ничего не знает про UI.
///
/// Восстановление после рестарта: RestoreAsync проверяет БД на
/// наличие running-записи. Если есть, то переводит сервис в состояние
/// Running и продолжает счётчик с исходного Range.Start.
/// </summary>
public interface ITimerService
{
    /// <summary>Текущее состояние таймера</summary>
    TimerState State
    {
        get;
    }

    /// <summary>
    /// Текущая активная запись или null, если таймер не запущен
    /// </summary>
    TimeEntry? CurrentEntry
    {
        get;
    }

    /// <summary>
    /// Текущая длительность сессии без учёта пауз.
    /// Для Idle — Duration.Zero
    /// </summary>
    Duration CurrentDuration
    {
        get;
    }

    /// <summary>
    /// Поднимается раз в секунду, пока State == Running
    /// </summary>
    event EventHandler<TimerTickEventArgs>? Tick;

    /// <summary>
    /// Поднимается при каждом переходе стейт-машины
    /// </summary>
    event EventHandler<TimerStateChangedEventArgs>? StateChanged;

    /// <summary>
    /// Начать новую запись
    /// </summary>
    /// <param name="description">Описание записи</param>
    /// <param name="projectId">Id проекта или null</param>
    /// <param name="ct">Токен отмены</param>
    Task StartTimerAsync(string? description, ProjectId? projectId, CancellationToken ct = default);

    /// <summary>
    /// Остановить активную запись и сохранить ее
    /// </summary>
    /// <param name="ct">Токен отмены</param>
    Task StopTimerAsync(CancellationToken ct = default);

    /// <summary>
    /// Поставить на паузу. Время паузы не идет в Duration
    /// </summary>
    void PauseTimer();

    /// <summary>
    /// Возобновить после паузы
    /// </summary>
    void ResumeTimer();

    /// <summary>
    /// Восстановить running-сессию после рестарта приложения.
    /// Вызывается один раз при старте. Если running-записи нет, то состояние остается Idle.
    /// </summary>
    /// <param name="ct">Токен отмены</param>
    Task RestoreTimerAsync(CancellationToken ct = default);
}
