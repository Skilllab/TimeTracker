namespace TimeTracker.Application;

/// <summary>
/// Исходящий порт: признак того, что приложение уже запущено.
/// </summary>
public interface IInstanceGuard : IDisposable
{
    /// <summary>
    /// Пытается занять признак единственного экземпляра.
    /// </summary>
    /// <param name="name">Имя признака, общее для всех запусков приложения.</param>
    bool TryAcquire(string name);
}
