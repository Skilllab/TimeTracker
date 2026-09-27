using Avalonia.Controls;
using TimeTracker.Presentation.ViewModels;

namespace TimeTracker.Presentation.Views;

/// <summary>
/// Окно подтверждения необратимого действия.
/// </summary>
public partial class ConfirmWindow : Window
{
    /// <summary>
    /// Создает окно.
    /// </summary>
    public ConfirmWindow()
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
        if (DataContext is ConfirmViewModel viewModel)
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
