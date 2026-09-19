using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using TimeTracker.Application.Abstractions.Persistence;
using TimeTracker.Wpf.Dialogs;
using TimeTracker.Wpf.Mvvm;

namespace TimeTracker.Wpf.ViewModels;

/// <summary>
/// ViewModel списка записей за сегодня.
///
/// Загружает записи через ITimeEntryRepository.GetByDateRangeAsync
/// с границами [начало дня, начало завтрашнего дня) в UTC.
///
/// Группировка по проекту — на уровне UI (CollectionViewSource)
/// или здесь через GroupName. Для простоты — здесь.
///
/// ReloadAsync вызывается:
///   - при инициализации;
///   - после StopTimerAsync (через EntryStopped);
///   - после редактирования/удаления записи.
/// </summary>
public sealed partial class TodayEntriesViewModel : ViewModelBase
{
    private readonly ITimeEntryRepository _entryRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IDialogService _dialogService;
    private readonly ILogger<TodayEntriesViewModel> _logger;

    public ObservableCollection<EntryRowViewModel> Entries { get; } = [];

    public string TotalDurationDisplay { get; private set; } = "00:00";

    public TodayEntriesViewModel(
        ITimeEntryRepository entryRepository,
        IProjectRepository projectRepository,
        IDialogService dialogService,
        ILogger<TodayEntriesViewModel> logger)
    {
        _entryRepository = entryRepository;
        _projectRepository = projectRepository;
        _dialogService = dialogService;
        _logger = logger;
    }

    /// <summary>
    /// Загрузить записи за сегодня.
    /// </summary>
    public async Task ReloadAsync()
    {
        var now = DateTimeOffset.Now;
        var dayStart = new DateTimeOffset(now.Date, now.Offset);
        var dayEnd = dayStart.AddDays(1);

        var entries = await _entryRepository.GetByDateRangeAsync(dayStart, dayEnd);
        var projects = await _projectRepository.GetAllAsync();

        var projectLookup = projects.ToDictionary(p => p.Id, p => p);

        Entries.Clear();

        foreach (var entry in entries)
        {
            var projectName = entry.ProjectId is not null
                && projectLookup.TryGetValue(entry.ProjectId.Value, out var project)
                    ? project.Name
                    : null;

            var projectColor = entry.ProjectId is not null
                && projectLookup.TryGetValue(entry.ProjectId.Value, out var p)
                    ? p.Color.Hex
                    : null;

            Entries.Add(new EntryRowViewModel(entry, projectName, projectColor));
        }

        var total = entries.Aggregate(
            TimeSpan.Zero,
            (acc, e) => acc + e.Range.Duration.Value);
        TotalDurationDisplay = Domain.TimeTracking.Duration
            .FromTimeSpan(total)
            .ToHumanReadable();

        OnPropertyChanged(nameof(TotalDurationDisplay));
    }
}
