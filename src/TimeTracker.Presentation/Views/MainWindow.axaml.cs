using Avalonia.Controls;
using TimeTracker.Presentation.Design;
using TimeTracker.Presentation.ViewModels;

namespace TimeTracker.Presentation.Views;

/// <summary>
/// Главное окно приложения: показывает счетчик и управляет записью.
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// Создает главное окно для дизайнера XAML: DataContext заполняется образцом данных.
    /// </summary>
    public MainWindow()
        : this(new MainWindowViewModel(new DesignTimerControl(), new DesignTimeEntryList()))
    {
    }

    /// <summary>
    /// Создает главное окно.
    /// </summary>
    /// <param name="viewModel">ViewModel окна.</param>
    public MainWindow(MainWindowViewModel viewModel)
    {
        if (viewModel is null)
        {
            throw new ArgumentNullException(nameof(viewModel));
        }

        InitializeComponent();
        DataContext = viewModel;
    }


}
