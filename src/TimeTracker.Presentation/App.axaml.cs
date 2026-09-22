using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using TimeTracker.Presentation.Views;

namespace TimeTracker.Presentation;

/// <summary>
/// Корневой класс приложения Avalonia.
/// </summary>
public partial class App : Application
{
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
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
