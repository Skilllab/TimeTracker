namespace TimeTracker.Domain;

/// <summary>
/// Исключение домена: нарушено правило проекта.
/// </summary>
public sealed class InvalidProjectException : DomainException
{
    /// <summary>
    /// Создает исключение с описанием нарушения.
    /// </summary>
    /// <param name="message">Описание нарушения.</param>
    public InvalidProjectException(string message) : base(message) { }
}
