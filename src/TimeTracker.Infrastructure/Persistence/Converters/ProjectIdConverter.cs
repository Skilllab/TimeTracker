using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TimeTracker.Domain.Projects;

namespace TimeTracker.Infrastructure.Persistence.Converters;

/// <summary>
/// Маппинг ProjectId в БД. Хранится как Guid
/// </summary>
public sealed class ProjectIdConverter : ValueConverter<ProjectId, Guid>
{
    public ProjectIdConverter() : base(
        id => id.Value,
        value => ProjectId.From(value))
    {
    }
}
