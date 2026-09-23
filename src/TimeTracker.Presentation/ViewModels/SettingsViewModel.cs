using CommunityToolkit.Mvvm.ComponentModel;
using TimeTracker.Application;
using TimeTracker.Presentation.Shell;

namespace TimeTracker.Presentation.ViewModels;

/// <summary>
/// ViewModel экрана настроек: порог простоя и горячая клавиша.
/// </summary>
public sealed partial class SettingsViewModel : ObservableObject
{
    private readonly IdleSettings _idleSettings;
    private readonly HotKeySettings _hotKeySettings;
    private readonly LocalizationManager _localizationManager;

    /// <summary>
    /// Создает экран настроек.
    /// </summary>
    /// <param name="idleSettings">Настройка порога простоя.</param>
    /// <param name="hotKeySettings">Настройка горячей клавиши.</param>
    /// <param name="localizationManager">Управление языком.</param>
    public SettingsViewModel(
        IdleSettings idleSettings,
        HotKeySettings hotKeySettings,
        LocalizationManager localizationManager)
    {
        _idleSettings = idleSettings ?? throw new ArgumentNullException(nameof(idleSettings));
        _hotKeySettings = hotKeySettings ?? throw new ArgumentNullException(nameof(hotKeySettings));
        _localizationManager = localizationManager ?? throw new ArgumentNullException(nameof(localizationManager));

        _idleMinutes = (int)_idleSettings.Threshold.TotalMinutes;
    }

    [ObservableProperty]
    private int _idleMinutes;

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
}
