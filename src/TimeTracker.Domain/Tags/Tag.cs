using TimeTracker.Domain.Common;

namespace TimeTracker.Domain.Tags;

/// <summary>
/// Тег записи времени
///
/// Свободная метка на TimeEntry. Например, «urgent», «meeting», «billable».
/// Пользователь назначает теги вручную на этапе UI (этап 5) и может
/// фильтровать записи по ним в отчетах
/// </summary>
public record class Tag
{
    /// <summary>
    /// Максимальная длина имени тега после нормализации
    /// </summary>
    public const int MaxLength = 50;

    /// <summary>
    /// Имя тега. Всегда нормализовано (trim + lowercase).
    /// </summary>
    public string Name
    {
        get;
    }

    private Tag(string name) => Name = name;

    /// <summary>
    /// Создать тег из пользовательской строки
    /// </summary>
    /// <param name="name">Имя тега</param>
    public static Tag Create(string name)
    {
        Guard.AgainstNullOrWhiteSpace(name, nameof(name));

        var normalized = name.Trim().ToLowerInvariant();

        if (normalized.Length > MaxLength)
            throw new DomainException($"Tag name exceeds maximum length of {MaxLength}.");

        return new Tag(normalized);
    }
}
