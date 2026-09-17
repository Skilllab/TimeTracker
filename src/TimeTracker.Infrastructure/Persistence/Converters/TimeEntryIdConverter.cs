using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TimeTracker.Domain.TimeTracking;

namespace TimeTracker.Infrastructure.Persistence.Converters;

/// <summary>
/// Маппинг TimeEntryId в БД. Хранится как Guid
/// </summary>
public sealed class TimeEntryIdConverter : ValueConverter<TimeEntryId, Guid>
{
    public TimeEntryIdConverter() : base(
        id => id.Value,
        value => TimeEntryId.From(value))
    {
    }
}
