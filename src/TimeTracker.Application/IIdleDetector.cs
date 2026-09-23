namespace TimeTracker.Application;

/// <summary>
/// Исходящий порт: сведения о том, как долго пользователь не проявлял активности.
/// </summary>
public interface IIdleDetector
{
    /// <summary>
    /// Возвращает время с момента последнего ввода мышью или клавиатурой.
    /// </summary>
    TimeSpan GetIdleTime();
}
