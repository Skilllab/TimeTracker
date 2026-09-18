namespace TimeTracker.Application.Abstractions.Idle;

/// <summary>
/// Определяет время бездействия пользователя.
/// </summary>
public interface IIdleDetector
{
    /// <summary>
    /// Сколько времени пользователь не проявляет активности (движение мыши, нажатие клавиш).
    /// </summary>
    TimeSpan GetIdleTime();
}
