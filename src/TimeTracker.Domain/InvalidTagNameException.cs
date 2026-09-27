namespace TimeTracker.Domain;

/// <summary>
/// Исключение домена: имя тега нарушает правило.
/// </summary>
public sealed class InvalidTagNameException : DomainException
{
    /// <summary>
    /// Создает исключение с описанием нарушения.
    /// </summary>
    /// <param name="message">Описание нарушения.</param>
    public InvalidTagNameException(string message) : base(message) { }
}
