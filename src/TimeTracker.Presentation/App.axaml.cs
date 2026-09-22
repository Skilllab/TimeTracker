using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using TimeTracker.Presentation.Views;
using AvaloniaApplication = Avalonia.Application;

namespace TimeTracker.Presentation;

/// <summary>
/// Корневой класс приложения Avalonia.
/// </summary>
public partial class App : AvaloniaApplication
{
    private readonly Func<MainWindow> _mainWindowFactory;

    /// <summary>
    /// Создает приложение.
    /// </summary>
    /// <param name="mainWindowFactory">Фабрика главного окна; реализацию подставляет composition root.</param>
    public App(Func<MainWindow> mainWindowFactory)
    {
        _mainWindowFactory = mainWindowFactory ?? throw new ArgumentNullException(nameof(mainWindowFactory));
    }

    /// <summary>
    /// Загружает XAML приложения и в Debug-сборке подключает инспектор Avalonia
    /// (без этого вызова пакет AvaloniaUI.DiagnosticsSupport не активируется).
    /// </summary>
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
#if DEBUG
        this.AttachDeveloperTools();
#endif
    }

    /// <summary>
    /// Создает главное окно после инициализации платформы.
    /// </summary>
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = _mainWindowFactory();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
