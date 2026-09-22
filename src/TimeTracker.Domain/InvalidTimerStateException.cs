/// <summary>
/// Исключение домена: операция недопустима в текущем состоянии записи.
/// </summary>
public sealed class InvalidTimerStateException : DomainException
{
    /// <summary>
    /// Создает исключение с описанием нарушения.
    /// </summary>
    /// <param name="message">Описание нарушения.</param>
    public InvalidTimerStateException(string message) : base(message) { }
}
