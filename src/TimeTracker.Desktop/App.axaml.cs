using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using TimeTracker.Desktop.Views;

namespace TimeTracker.Desktop;

/// <summary>
/// Корневой класс приложения Avalonia.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Загружает XAML приложения
    /// </summary>
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <summary>
    /// Создает главное окно после инициализации платформы
    /// </summary>
    public override void OnFrameworkInitializationCompleted()
    {
        // Окно создается явно, а не по StartupUri, чтобы позже подставить ViewModel/DI без правки XAML
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
