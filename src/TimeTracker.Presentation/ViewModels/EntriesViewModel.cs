using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using TimeTracker.Application;
using TimeTracker.Domain;

namespace TimeTracker.Presentation.ViewModels;

/// <summary>
/// ViewModel экрана записей за сегодня.
/// </summary>
public sealed partial class EntriesViewModel
{
    private readonly ITimeEntryList _entryList;

    /// <summary>
    /// Создает экран записей.
    /// </summary>
    /// <param name="entryList">Входящий порт списка записей.</param>
    public EntriesViewModel(ITimeEntryList entryList)
    {
        _entryList = entryList ?? throw new ArgumentNullException(nameof(entryList));

        _ = RefreshAsync();
    }

    /// <summary>
    /// Записи времени за сегодня.
    /// </summary>
    public ObservableCollection<TimeEntry> Entries { get; } = new();

    /// <summary>
    /// Перечитывает записи за сегодня.
    /// </summary>
    [RelayCommand]
    public async Task RefreshAsync()
    {
        var entries = await _entryList.GetTodayAsync();

        Entries.Clear();

        foreach (var entry in entries)
        {
            Entries.Add(entry);
        }
    }
}
