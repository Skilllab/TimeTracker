namespace TimeTracker.Application;

/// <summary>
/// Настройка простоя: интервал, после которого отсутствие активности считается простоем.
/// </summary>
public sealed class IdleSettings
{
    /// <summary>
    /// Порог по умолчанию: пять минут.
    /// </summary>
    public static readonly TimeSpan DefaultThreshold = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Событие изменения порога.
    /// </summary>
    public event EventHandler? Changed;

    /// <summary>
    /// Текущий порог простоя.
    /// </summary>
    public TimeSpan Threshold { get; private set; } = DefaultThreshold;

    /// <summary>
    /// Задает порог простоя.
    /// </summary>
    /// <param name="threshold">Новый порог; должен быть больше нуля.</param>
    public void SetThreshold(TimeSpan threshold)
    {
        if (threshold <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(threshold), "Порог простоя должен быть больше нуля.");
        }

        if (Threshold == threshold)
        {
            return;
        }

        Threshold = threshold;

        Changed?.Invoke(this, EventArgs.Empty);
    }
}
