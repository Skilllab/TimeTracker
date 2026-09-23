using TimeTracker.Application;

namespace TimeTracker.Infrastructure;

/// <summary>
/// Заглушка исходящего порта простоя для систем без средства определения активности.
/// </summary>
public sealed class NoopIdleDetector : IIdleDetector
{
    /// <summary>
    /// Возвращает нулевое время простоя: активность считается непрерывной.
    /// </summary>
    public TimeSpan GetIdleTime() => TimeSpan.Zero;
}
