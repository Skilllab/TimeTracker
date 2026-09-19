using CommunityToolkit.Mvvm.ComponentModel;
using TimeTracker.Domain.TimeTracking;

namespace TimeTracker.Wpf.ViewModels;

/// <summary>
/// ViewModel одной строки в списке записей за сегодня.
///
/// Хранит Id, описание, время начала/окончания, длительность.
/// </summary>
public sealed partial class EntryRowViewModel : ObservableObject
{
    public TimeEntryId Id
    {
        get;
    }
    public DateTimeOffset StartedAt
    {
        get;
    }
    public DateTimeOffset? StoppedAt
    {
        get;
    }
    public Duration Duration
    {
        get;
    }

    [ObservableProperty]
    private string _description;

    [ObservableProperty]
    private string? _projectName;

    [ObservableProperty]
    private string? _projectColorHex;

    [ObservableProperty]
    private bool _isEditing;

    /// <summary>
    /// Создать ViewModel строки из доменной записи.
    /// </summary>
    /// <param name="entry">Доменная запись. Не может быть null</param>
    /// <param name="projectName"> Имя проекта или null, если запись без проекта </param>
    /// <param name="projectColorHex"> Hex-цвет проекта или null, если запись без проекта </param>
    public EntryRowViewModel(TimeEntry entry, string? projectName, string? projectColorHex)
    {
        ArgumentNullException.ThrowIfNull(entry);

        Id = entry.Id;
        StartedAt = entry.Range.Start;
        StoppedAt = entry.Range.End;
        Duration = entry.Range.Duration;
        _description = entry.Description;
        _projectName = projectName;
        _projectColorHex = projectColorHex;
    }

    public string TimeRangeDisplay =>
        StoppedAt is null
            ? $"{StartedAt:HH:mm} — сейчас"
            : $"{StartedAt:HH:mm} — {StoppedAt:HH:mm}";

    public string DurationDisplay => Duration.ToHumanReadable();
}
