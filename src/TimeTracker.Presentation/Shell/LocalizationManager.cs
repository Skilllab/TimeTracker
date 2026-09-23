using Avalonia;
using AvaloniaApplication = Avalonia.Application;


namespace TimeTracker.Presentation.Shell;

/// <summary>
/// Управление языком интерфейса: хранит выбор и сообщает о его смене.
/// </summary>
public sealed class LocalizationManager
{
    /// <summary>
    /// Доступные языки.
    /// </summary>
    public static IReadOnlyList<AppLanguage> AvailableLanguages { get; } =
        new[] { AppLanguage.Russian, AppLanguage.English };

    /// <summary>
    /// Создает управление языком и делает его текущим.
    /// </summary>
    public LocalizationManager()
    {
        Current = this;
    }

    /// <summary>
    /// Действующее управление языком.
    /// </summary>
    public static LocalizationManager? Current { get; private set; }

    /// <summary>
    /// Текущий язык.
    /// </summary>
    public AppLanguage Language { get; private set; } = AppLanguage.Russian;

    /// <summary>
    /// Событие смены языка.
    /// </summary>
    public event EventHandler? Changed;

    /// <summary>
    /// Возвращает строку по ключу на текущем языке.
    /// </summary>
    /// <param name="key">Ключ строки.</param>
    public string this[string key]
    {
        get
        {
            var application = AvaloniaApplication.Current;

            if (application is not null
                && application.TryGetResource(key, application.ActualThemeVariant, out var value)
                && value is string text)
            {
                return text;
            }

            return key;
        }
    }

    /// <summary>
    /// Выбирает язык.
    /// </summary>
    /// <param name="language">Выбранный язык.</param>
    public void Select(AppLanguage language)
    {
        if (Language == language)
        {
            return;
        }

        Language = language;

        Changed?.Invoke(this, EventArgs.Empty);
    }
}
