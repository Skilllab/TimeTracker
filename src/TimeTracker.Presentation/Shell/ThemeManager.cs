namespace TimeTracker.Presentation.Shell;

/// <summary>
/// Управление темой оформления: хранит выбор и сообщает о его смене.
/// </summary>
public sealed class ThemeManager
{
    /// <summary>
    /// Доступные темы.
    /// </summary>
    public static IReadOnlyList<AppTheme> AvailableThemes { get; } =
        new[] { AppTheme.Light, AppTheme.Dark, AppTheme.System };

    /// <summary>
    /// Текущая тема.
    /// </summary>
    public AppTheme Current { get; private set; } = AppTheme.System;

    /// <summary>
    /// Событие смены темы.
    /// </summary>
    public event EventHandler? Changed;

    /// <summary>
    /// Выбирает тему.
    /// </summary>
    /// <param name="theme">Выбранная тема.</param>
    public void Select(AppTheme theme)
    {
        if (Current == theme)
        {
            return;
        }

        Current = theme;

        Changed?.Invoke(this, EventArgs.Empty);
    }
}
