using TimeTracker.Application;

namespace TimeTracker.Infrastructure;

/// <summary>
/// Реализация исходящего порта единственного экземпляра на именованном мьютексе.
/// </summary>
public sealed class SingleInstanceGuard : IInstanceGuard
{
    private Mutex? _mutex;

    /// <summary>
    /// Пытается занять признак единственного экземпляра.
    /// </summary>
    /// <param name="name">Имя признака, общее для всех запусков приложения.</param>
    public bool TryAcquire(string name)
    {
        _mutex = new Mutex(initiallyOwned: true, name, out var createdNew);

        return createdNew;
    }

    /// <summary>
    /// Освобождает признак единственного экземпляра.
    /// </summary>
    public void Dispose()
    {
        _mutex?.Dispose();
        _mutex = null;
    }
}
