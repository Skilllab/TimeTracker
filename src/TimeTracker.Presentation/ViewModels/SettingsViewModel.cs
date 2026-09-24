using CommunityToolkit.Mvvm.ComponentModel;
using TimeTracker.Application;
using TimeTracker.Presentation.Shell;

namespace TimeTracker.Presentation.ViewModels;

/// <summary>
/// ViewModel экрана настроек: порог простоя, горячая клавиша и автозапуск.
/// </summary>
public sealed partial class SettingsViewModel : ObservableObject
{
    private readonly IdleSettings _idleSettings;
    private readonly HotKeySettings _hotKeySettings;
    private readonly LocalizationManager _localizationManager;
    private readonly IAutoStartService _autoStartService;

    /// <summary>
    /// Создает экран настроек.
    /// </summary>
    /// <param name="idleSettings">Настройка порога простоя.</param>
    /// <param name="hotKeySettings">Настройка горячей клавиши.</param>
    /// <param name="localizationManager">Управление языком.</param>
    /// <param name="autoStartService">Сервис автозапуска.</param>
    public SettingsViewModel(
        IdleSettings idleSettings,
        HotKeySettings hotKeySettings,
        LocalizationManager localizationManager,
        IAutoStartService autoStartService)
    {
        _idleSettings = idleSettings ?? throw new ArgumentNullException(nameof(idleSettings));
        _hotKeySettings = hotKeySettings ?? throw new ArgumentNullException(nameof(hotKeySettings));
        _localizationManager = localizationManager ?? throw new ArgumentNullException(nameof(localizationManager));
        _autoStartService = autoStartService ?? throw new ArgumentNullException(nameof(autoStartService));

        _idleMinutes = (int)_idleSettings.Threshold.TotalMinutes;
        _autoStartEnabled = _autoStartService.IsEnabled();
    }

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
}
