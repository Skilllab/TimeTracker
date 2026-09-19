namespace TimeTracker.Wpf.Services;

/// <summary>
/// Глобальная горячая клавиша. По умолчанию Ctrl+Shift+Space —
/// toggle Start/Stop таймера.
///
/// Реализация — через WinAPI RegisterHotKey. Требует HWND окна —
/// поэтому регистрация идёт после того, как окно создано.
/// </summary>
public interface IHotKeyService : IDisposable
{
    /// <summary>Зарегистрировать хоткей на HWND окна.</summary>
    void Register(System.Windows.Window window);

    /// <summary>Поднимается при срабатывании хоткея.</summary>
    event EventHandler? ToggleRequested;
}
