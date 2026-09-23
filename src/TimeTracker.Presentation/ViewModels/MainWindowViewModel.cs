using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeTracker.Presentation.Shell;

namespace TimeTracker.Presentation.ViewModels;

/// <summary>
/// ViewModel оболочки: навигация между экранами, выбор темы и языка.
/// </summary>
public sealed partial class MainWindowViewModel : ObservableObject
{
    private readonly ThemeManager _themeManager;
    private readonly LocalizationManager _localizationManager;

    /// <summary>
    /// Создает оболочку.
    /// </summary>
    /// <param name="timerViewModel">Экран таймера.</param>
    /// <param name="entriesViewModel">Экран записей за сегодня.</param>
    /// <param name="themeManager">Управление темой.</param>
    /// <param name="localizationManager">Управление языком.</param>
    public MainWindowViewModel(
        TimerViewModel timerViewModel,
        EntriesViewModel entriesViewModel,
        ThemeManager themeManager,
        LocalizationManager localizationManager)
    {
        _themeManager = themeManager ?? throw new ArgumentNullException(nameof(themeManager));
        _localizationManager = localizationManager ?? throw new ArgumentNullException(nameof(localizationManager));

        Items.Add(new NavigationItem("Nav.Timer", timerViewModel));
        Items.Add(new NavigationItem("Nav.Entries", entriesViewModel));

        SelectedItem = Items[0];
    }

    /// <summary>
    /// Пункты навигации.
    /// </summary>
    public ObservableCollection<NavigationItem> Items { get; } = new();

    /// <summary>
    /// Выбранный пункт навигации.
    /// </summary>
    [ObservableProperty]
    private NavigationItem? _selectedItem;

    /// <summary>
    /// Экран выбранного пункта.
    /// </summary>
    public object? CurrentPage => SelectedItem?.Page;

    /// <summary>
    /// Доступные темы.
    /// </summary>
    public IReadOnlyList<AppTheme> Themes { get; } = ThemeManager.AvailableThemes;

    /// <summary>
    /// Доступные языки.
    /// </summary>
    public IReadOnlyList<AppLanguage> Languages { get; } = LocalizationManager.AvailableLanguages;

    /// <summary>
    /// Текущая тема.
    /// </summary>
    public AppTheme Theme
    {
        get => _themeManager.Current;
        set
        {
            _themeManager.Select(value);
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Текущий язык.
    /// </summary>
    public AppLanguage Language
    {
        get => _localizationManager.Language;
        set
        {
            _localizationManager.Select(value);
            OnPropertyChanged();
            RefreshTitles();
        }
    }

    /// <summary>
    /// Открывает выбранный пункт навигации.
    /// </summary>
    /// <param name="item">Пункт, который нужно открыть.</param>
    [RelayCommand]
    private void Select(NavigationItem? item)
    {
        if (item is not null)
        {
            SelectedItem = item;
        }
    }

    /// <summary>
    /// Обновляет заголовки пунктов после смены языка.
    /// </summary>
    private void RefreshTitles()
    {
        foreach (var item in Items)
        {
            item.RefreshTitle();
        }
    }

    partial void OnSelectedItemChanged(NavigationItem? value)
    {
        OnPropertyChanged(nameof(CurrentPage));

        if (value?.Page is EntriesViewModel entries)
        {
            _ = entries.RefreshAsync();
        }
    }
}
