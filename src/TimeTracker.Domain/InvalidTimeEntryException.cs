namespace TimeTracker.Domain;

/// <summary>
/// Исключение домена: нарушено правило записи времени.
/// </summary>
public sealed class InvalidTimeEntryException : DomainException
{
    /// <summary>
    /// Создает исключение с описанием нарушения.
    /// </summary>
    /// <param name="message">Описание нарушения.</param>
    public InvalidTimeEntryException(string message) : base(message) { }
}
