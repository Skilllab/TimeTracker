using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using TimeTracker.Presentation.ViewModels;

namespace TimeTracker.Presentation.Views;

/// <summary>
/// Окно выбора проекта задачи.
/// </summary>
public partial class ProjectPickerWindow : Window
{
    /// <summary>
    /// Создает окно.
    /// </summary>
    public ProjectPickerWindow()
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
        if (DataContext is ProjectPickerViewModel viewModel)
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
