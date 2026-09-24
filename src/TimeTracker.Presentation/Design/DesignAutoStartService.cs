using TimeTracker.Application;

namespace TimeTracker.Presentation.Design;

/// <summary>
/// Заглушка исходящего порта автозапуска для дизайнера XAML: автозапуск считается выключенным.
/// </summary>
internal sealed class DesignAutoStartService : IAutoStartService
{
    /// <summary>
    /// Ничего не делает для дизайнера.
    /// </summary>
    public void Enable()
    {
    }

    /// <summary>
    /// Ничего не делает для дизайнера.
    /// </summary>
    public void Disable()
    {
    }

    /// <summary>
    /// Возвращает false для дизайнера.
    /// </summary>
    public bool IsEnabled() => false;
}
