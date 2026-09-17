namespace TimeTracker.Domain.Common;

/// <summary>
/// Нарушение инварианта домена
/// </summary>
public sealed class DomainException : Exception
{
    /// <summary>
    /// Инициализирует новый экземпляр DomainException с указанным сообщением
    /// </summary>
    /// <param name="message">Текст сообщения, описывающего причину исключения</param>
    public DomainException(string message) : base(message) { }

    /// <summary>
    /// Инициализирует новый экземпляр DomainException с указанным сообщением и внутренним исключением
    /// </summary>
    /// <param name="message">Текст сообщения, описывающего причину исключения</param>
    /// <param name="inner">Внутреннее исключение</param>
    public DomainException(string message, Exception inner) : base(message, inner) { }
}
