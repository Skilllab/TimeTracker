using Avalonia.Controls;
using TimeTracker.Application;
using TimeTracker.Presentation.Design;
using TimeTracker.Presentation.Shell;
using TimeTracker.Presentation.ViewModels;

namespace TimeTracker.Presentation.Views;

/// <summary>
/// Главное окно приложения: показывает список задач, настройки и отчеты открываются отдельными окнами.
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

        viewModel.AttachWindowFactories(
            settings => new SettingsWindow { DataContext = settings },
            reports => new ReportsWindow { DataContext = reports },
            this);
    }

    /// <summary>
    /// Собирает ViewModel для предпросмотра: экраны заполняются заглушками.
    /// </summary>
    private static MainWindowViewModel CreateDesignViewModel()
    {
        var themeManager = new ThemeManager();
        var localizationManager = new LocalizationManager();
        var projectList = new DesignProjectList();

        var tasks = new TasksViewModel(
            new DesignTaskList(),
            new DesignTaskControl(),
            projectList,
            localizationManager,
            new DesignTaskTagSuggestions());
        var projects = new ProjectsViewModel(projectList, new DesignProjectEditor());
        var settings = new SettingsViewModel(
            new IdleSettings(),
            new HotKeySettings(new DesignHotKeyService()),
            localizationManager,
            new DesignAutoStartService(),
            themeManager,
            projects);

        var reports = new ReportsViewModel(
            new DesignReportService(),
            new DesignReportExporter(),
            TimeProvider.System);

        return new MainWindowViewModel(tasks, settings, reports);
    }
}
