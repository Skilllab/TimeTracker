using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TimeTracker.Domain.Projects;

namespace TimeTracker.Infrastructure.Persistence.Converters;

/// <summary>
/// Маппинг ProjectColor в БД. Хранится как "#RRGGBB".
/// </summary>
public sealed class ProjectColorConverter : ValueConverter<ProjectColor, string>
{
    public ProjectColorConverter() : base(
        color => color.Hex,
        hex => ProjectColor.FromHex(hex))
    {
    }
}
