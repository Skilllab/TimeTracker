namespace TimeTracker.Domain.Common;

/// <summary>
/// Проверки инвариантов. При падении выбрасывается исключение DomainException
/// </summary>
public static class Guard
{
    public static void AgainstNullOrWhiteSpace(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException($"{paramName} не должен быть null или whitespace");
    }
}
