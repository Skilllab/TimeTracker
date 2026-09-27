using Avalonia.Controls;
using Avalonia.Platform.Storage;
using TimeTracker.Presentation.ViewModels;

namespace TimeTracker.Presentation.Views;

/// <summary>
/// Окно отчетов: вид отчета, период и выгрузка выборки в файл.
/// </summary>
public partial class ReportsWindow : Window
{
    /// <summary>
    /// Создает окно.
    /// </summary>
    public ReportsWindow()
    {
        InitializeComponent();

        DataContextChanged += OnDataContextChanged;
    }

    /// <summary>
    /// Передает модели диалог выбора файла и подписывается на запрос закрытия.
    /// </summary>
    /// <param name="sender">Источник события.</param>
    /// <param name="e">Данные события.</param>
    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is ReportsViewModel viewModel)
        {
            viewModel.AttachSavePathPicker(PickSavePathAsync);
            viewModel.CloseRequested += OnCloseRequested;
        }
    }

    /// <summary>
    /// Закрывает окно по запросу модели.
    /// </summary>
    /// <param name="sender">Источник события.</param>
    /// <param name="e">Данные события.</param>
    private void OnCloseRequested(object? sender, EventArgs e) => Close();

    /// <summary>
    /// Спрашивает путь к файлу отчета; <c>null</c>, если выбор отменен.
    /// </summary>
    private async Task<string?> PickSavePathAsync()
    {
        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            SuggestedFileName = "report.csv",
            DefaultExtension = "csv",
            FileTypeChoices = new[]
            {
                new FilePickerFileType("CSV") { Patterns = new[] { "*.csv" } }
            }
        });

        return file?.Path.LocalPath;
    }
}
