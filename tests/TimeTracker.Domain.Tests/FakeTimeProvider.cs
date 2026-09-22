namespace TimeTracker.Domain.Tests;

/// <summary>
/// Управляемый источник времени: тесты двигают время вручную и не зависят от «сейчас».
/// </summary>
internal sealed class FakeTimeProvider : TimeProvider
{
    private DateTimeOffset _now;

    /// <summary>
    /// Создает источник времени с фиксированным началом.
    /// </summary>
    /// <param name="start">Начальный момент (UTC).</param>
    public FakeTimeProvider(DateTimeOffset start) => _now = start;

    public override DateTimeOffset GetUtcNow() => _now;

    /// <summary>
    /// Сдвигает «текущее» время вперед.
    /// </summary>
    /// <param name="delta">Величина сдвига.</param>
    public void Advance(TimeSpan delta) => _now += delta;
}
