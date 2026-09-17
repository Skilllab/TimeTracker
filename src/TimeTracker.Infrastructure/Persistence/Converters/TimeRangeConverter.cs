using System.Globalization;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TimeTracker.Domain.TimeTracking;

namespace TimeTracker.Infrastructure.Persistence.Converters;

/// <summary>
/// Маппинг TimeRange в БД. Хранится как "{StartTicks}|{EndTicks|null}".
/// Тики, а не ISO-8601, потому что точнее и не зависит от культуры.
/// </summary>
public sealed class TimeRangeConverter : ValueConverter<TimeRange, string>
{
    private const char Separator = '|';
    private const string NullEnd = "null";

    public TimeRangeConverter() : base(
        range => Serialize(range),
        value => Deserialize(value))
    {
    }

    private static string Serialize(TimeRange range)
    {
        var start = range.Start.UtcTicks.ToString(CultureInfo.InvariantCulture);
        var end = range.End?.UtcTicks.ToString(CultureInfo.InvariantCulture) ?? NullEnd;
        return $"{start}{Separator}{end}";
    }

    private static TimeRange Deserialize(string value)
    {
        var parts = value.Split(Separator);
        var start = new DateTimeOffset(
            long.Parse(parts[0], CultureInfo.InvariantCulture),
            TimeSpan.Zero);

        var end = parts[1] == NullEnd
            ? (DateTimeOffset?)null
            : new DateTimeOffset(
                long.Parse(parts[1], CultureInfo.InvariantCulture),
                TimeSpan.Zero);

        return new TimeRange(start, end);
    }
}
