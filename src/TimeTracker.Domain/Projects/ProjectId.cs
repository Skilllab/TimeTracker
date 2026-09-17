using TimeTracker.Domain.Common;

namespace TimeTracker.Domain.Projects;

/// <summary>
/// Идентификатор проекта
/// </summary>
public readonly record struct ProjectId(Guid Value)
{
    /// <summary>
    /// Создать новый уникальный Id
    /// </summary>
    public static ProjectId New() => new(Guid.NewGuid());

    /// <summary>
    /// Создать Id из готового Guid
    /// </summary>
    /// <param name="value">Guid для создания Id</param>
    public static ProjectId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new DomainException("ProjectId cannot be empty.");
        return new ProjectId(value);
    }
}
