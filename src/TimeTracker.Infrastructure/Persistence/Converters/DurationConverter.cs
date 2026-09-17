using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TimeTracker.Domain.TimeTracking;

namespace TimeTracker.Infrastructure.Persistence.Converters;

/// <summary>
/// Маппинг Duration в БД. Хранится как long (тики TimeSpan).
/// </summary>
public sealed class DurationConverter : ValueConverter<Duration, long>
{
    public DurationConverter() : base(
        duration => duration.Value.Ticks,
        ticks => Duration.FromTimeSpan(TimeSpan.FromTicks(ticks)))
    {
    }
}
