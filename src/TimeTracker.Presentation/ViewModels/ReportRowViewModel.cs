namespace TimeTracker.Presentation.ViewModels;

/// <summary>
/// Строка отчета для показа: наименование, пояснение и время.
/// </summary>
public sealed class ReportRowViewModel
{
    /// <summary>
    /// Создает строку отчета.
    /// </summary>
    /// <param name="name">Наименование строки: проект, тег или задача.</param>
    /// <param name="details">Пояснение строки: задачи строки или проект задачи.</param>
    /// <param name="timeText">Время строки.</param>
    public ReportRowViewModel(string name, string details, string timeText)
    {
        Name = name;
        Details = details;
        TimeText = timeText;
    }

    /// <summary>
    /// Наименование строки.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Пояснение строки.
    /// </summary>
    public string Details { get; }

    /// <summary>
    /// Время строки.
    /// </summary>
    public string TimeText { get; }
}
