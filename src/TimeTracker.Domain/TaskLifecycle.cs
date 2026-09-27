namespace TimeTracker.Domain;

/// <summary>
/// Метка жизненного цикла задачи: создана, начата или завершена.
/// Отдельный тип нужен потому, что метки описывают путь задачи целиком,
/// а не текущее состояние таймера и не состояние записи времени.
/// </summary>
public enum TaskLifecycle
{
    /// <summary>Задача создана.</summary>
    Created,

    /// <summary>Задача была начата.</summary>
    Started,

    /// <summary>Задача завершена.</summary>
    Finished,
}
