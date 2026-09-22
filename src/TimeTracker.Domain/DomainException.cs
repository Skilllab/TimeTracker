/// <summary>
/// Базовое исключение предметной области: нарушено правило домена.
/// </summary>
public abstract class DomainException : Exception
{
    /// <summary>
    /// Создает исключение с описанием нарушения.
    /// </summary>
    /// <param name="message">Описание нарушения.</param>
    protected DomainException(string message)
        : base(message)
    {
    }
}
