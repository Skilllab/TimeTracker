namespace TimeTracker.Wpf.Services;

/// <summary>
/// Сервис tray icon. Управляет иконкой в системном трее,
/// контекстным меню и взаимодействием с таймером.
///
/// Показывает Start/Stop/Open/Exit в меню, реагирует на
/// двойной клик — показывает окно.
/// </summary>
public interface ITrayIconService : IDisposable
{
    /// <summary>Инициализировать иконку в трее.</summary>
    void Initialize();

    /// <summary>Показать уведомление.</summary>
    void ShowNotification(string title, string message);
}
