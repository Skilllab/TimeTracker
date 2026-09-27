namespace TimeTracker.Domain;

/// <summary>
/// Исключение домена: операция недопустима в текущем состоянии задачи.
/// </summary>
public sealed class InvalidTaskStateException : DomainException
{
    /// <summary>
    /// Создает исключение с описанием нарушения.
    /// </summary>
    /// <param name="message">Описание нарушения.</param>
    public InvalidTaskStateException(string message) : base(message) { }
}
