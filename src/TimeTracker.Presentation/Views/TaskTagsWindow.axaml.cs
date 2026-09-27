using Avalonia.Controls;
using TimeTracker.Presentation.ViewModels;

namespace TimeTracker.Presentation.Views;

/// <summary>
/// Окно тегов задачи: теги задачи, имена из подсказок и создание нового тега.
/// </summary>
public partial class TaskTagsWindow : Window
{
    /// <summary>
    /// Создает окно.
    /// </summary>
    public TaskTagsWindow()
    {
        InitializeComponent();

        DataContextChanged += OnDataContextChanged;
    }

    /// <summary>
    /// Подписывается на запрос закрытия при смене модели.
    /// </summary>
    /// <param name="sender">Источник события.</param>
    /// <param name="e">Данные события.</param>
    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is TaskTagsViewModel viewModel)
        {
            viewModel.CloseRequested += OnCloseRequested;
        }
    }

    /// <summary>
    /// Закрывает окно по запросу модели.
    /// </summary>
    /// <param name="sender">Источник события.</param>
    /// <param name="e">Данные события.</param>
    private void OnCloseRequested(object? sender, EventArgs e) => Close();
}
