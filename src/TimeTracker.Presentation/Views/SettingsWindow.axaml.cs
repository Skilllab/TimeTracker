using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using TimeTracker.Presentation.ViewModels;

namespace TimeTracker.Presentation.Views;

/// <summary>
/// Окно настроек: тема, язык, простой, горячая клавиша, автозапуск и проекты.
/// </summary>
public partial class SettingsWindow : Window
{
    /// <summary>
    /// Создает окно.
    /// </summary>
    public SettingsWindow()
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
        if (DataContext is SettingsViewModel viewModel)
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
