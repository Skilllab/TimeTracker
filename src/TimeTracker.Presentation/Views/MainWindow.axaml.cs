using Avalonia.Controls;
using TimeTracker.Application;
using TimeTracker.Presentation.Design;
using TimeTracker.Presentation.Shell;
using TimeTracker.Presentation.ViewModels;

namespace TimeTracker.Presentation.Views;

/// <summary>
/// Главное окно приложения: оболочка с навигацией и переключателями темы и языка.
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// Создает главное окно для дизайнера XAML: DataContext заполняется образцами данных.
    /// </summary>
    public MainWindow()
        : this(CreateDesignViewModel())
    {
    }

    /// <summary>
    /// Создает главное окно.
    /// </summary>
    /// <param name="viewModel">ViewModel оболочки.</param>
    public MainWindow(MainWindowViewModel viewModel)
    {
        if (viewModel is null)
        {
            throw new ArgumentNullException(nameof(viewModel));
        }

        InitializeComponent();
        DataContext = viewModel;
    }

    /// <summary>
    /// Собирает ViewModel для предпросмотра: экраны заполняются заглушками.
    /// </summary>
    private static MainWindowViewModel CreateDesignViewModel()
    {
        var themeManager = new ThemeManager();
        var localizationManager = new LocalizationManager();

        var timer = new TimerViewModel(new DesignTimerControl(), localizationManager);
        var entries = new EntriesViewModel(new DesignTimeEntryList());
        var settings = new SettingsViewModel(new IdleSettings(), new HotKeySettings(new DesignHotKeyService()), localizationManager);

        return new MainWindowViewModel(timer, entries, settings, themeManager, localizationManager);
    }
}
