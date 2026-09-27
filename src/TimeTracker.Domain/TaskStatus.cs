namespace TimeTracker.Domain;

/// <summary>
/// Состояние задачи в списке работ.
/// </summary>
public enum TaskStatus
{
    /// <summary>Задача создана, но ни разу не запускалась.</summary>
    NotStarted,

    /// <summary>Задача выполняется, время идет.</summary>
    Running,

    /// <summary>Задача приостановлена, время стоит.</summary>
    Paused,

    /// <summary>Задача завершена.</summary>
    Finished
}
