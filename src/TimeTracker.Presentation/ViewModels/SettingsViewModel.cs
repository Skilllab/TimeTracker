using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeTracker.Application;
using TimeTracker.Presentation.Shell;

namespace TimeTracker.Presentation.ViewModels;

/// <summary>
/// ViewModel окна настроек: тема, язык, порог простоя, горячая клавиша,
/// автозапуск и управление проектами.
/// </summary>
public sealed partial class SettingsViewModel : ObservableObject
{
    private readonly IdleSettings _idleSettings;
    private readonly HotKeySettings _hotKeySettings;
    private readonly LocalizationManager _localizationManager;
    private readonly IAutoStartService _autoStartService;
    private readonly ThemeManager _themeManager;

    /// <summary>
    /// Создает модель окна настроек.
    /// </summary>
    /// <param name="idleSettings">Настройка порога простоя.</param>
    /// <param name="hotKeySettings">Настройка горячей клавиши.</param>
    /// <param name="localizationManager">Управление языком.</param>
    /// <param name="autoStartService">Сервис автозапуска.</param>
    /// <param name="themeManager">Управление темой.</param>
    /// <param name="projects">Список проектов.</param>
    public SettingsViewModel(
        IdleSettings idleSettings,
        HotKeySettings hotKeySettings,
        LocalizationManager localizationManager,
        IAutoStartService autoStartService,
        ThemeManager themeManager,
        ProjectsViewModel projects)
    {
        _idleSettings = idleSettings ?? throw new ArgumentNullException(nameof(idleSettings));
        _hotKeySettings = hotKeySettings ?? throw new ArgumentNullException(nameof(hotKeySettings));
        _localizationManager = localizationManager ?? throw new ArgumentNullException(nameof(localizationManager));
        _autoStartService = autoStartService ?? throw new ArgumentNullException(nameof(autoStartService));
        _themeManager = themeManager ?? throw new ArgumentNullException(nameof(themeManager));
        Projects = projects ?? throw new ArgumentNullException(nameof(projects));

        _idleMinutes = (int)_idleSettings.Threshold.TotalMinutes;
        _autoStartEnabled = _autoStartService.IsEnabled();
    }

    /// <summary>
    /// Событие запроса закрытия окна.
    /// </summary>
    public event EventHandler? CloseRequested;

    /// <summary>
    /// Список проектов: создание, правка, архивация и показ архивных.
    /// </summary>
    public ProjectsViewModel Projects { get; }

    /// <summary>
    /// Версия приложения: номер записан в сборку при сборке.
    /// </summary>
    public string AppVersion { get; } = ResolveAppVersion();

    /// <summary>
    /// Читает версию из метаданных сборки.
    /// Номер информационной версии содержит значение, записанное при сборке;
    /// часть после знака «+» отбрасывается, потому что это метка сборки, а не номер.
    /// </summary>
    private static string ResolveAppVersion()
    {
        var informational = typeof(SettingsViewModel).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;

        if (string.IsNullOrWhiteSpace(informational))
        {
            return typeof(SettingsViewModel).Assembly.GetName().Version?.ToString() ?? string.Empty;
        }

        var separatorIndex = informational.IndexOf('+');

        return separatorIndex < 0 ? informational : informational[..separatorIndex];
    }

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
        }
    }

    /// <summary>
    /// Порог простоя в минутах.
    /// </summary>
    [ObservableProperty]
    private int _idleMinutes;

    /// <summary>
    /// Признак автозапуска приложения.
    /// </summary>
    [ObservableProperty]
    private bool _autoStartEnabled;

    /// <summary>
    /// Подпись горячей клавиши.
    /// </summary>
    public string HotKeyCaption => $"{_hotKeySettings.Current.Modifiers} + {_hotKeySettings.Current.Key}";

    /// <summary>
    /// Признак того, что сочетание занято другой программой.
    /// </summary>
    public bool IsHotKeyBusy => !_hotKeySettings.IsRegistered;

    /// <summary>
    /// Применяет введенное значение порога.
    /// </summary>
    partial void OnIdleMinutesChanged(int value)
    {
        if (value > 0)
        {
            _idleSettings.SetThreshold(TimeSpan.FromMinutes(value));
        }
    }

    /// <summary>
    /// Применяет изменение автозапуска.
    /// </summary>
    partial void OnAutoStartEnabledChanged(bool value)
    {
        if (value)
        {
            _autoStartService.Enable();
        }
        else
        {
            _autoStartService.Disable();
        }
    }

    /// <summary>
    /// Закрывает окно настроек.
    /// Без запроса закрытия вызывающий код не получил бы управление:
    /// показ окна завершается только после его закрытия.
    /// </summary>
    [RelayCommand]
    private void Close() => CloseRequested?.Invoke(this, EventArgs.Empty);
}
