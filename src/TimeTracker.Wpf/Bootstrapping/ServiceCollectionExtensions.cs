using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Wpf.Dialogs;
using TimeTracker.Wpf.Mvvm;
using TimeTracker.Wpf.Navigation;
using TimeTracker.Wpf.ViewModels;
using TimeTracker.Wpf.Views;

namespace TimeTracker.Wpf.Bootstrapping;

/// <summary>
/// Регистрация WPF-слоя: окна, ViewModels, сервисы навигации и диалогов,
/// ViewMapping.
///
/// Все View — transient, потому что окно создаётся один раз
/// и после закрытия уничтожается. Если делать singleton —
/// повторное открытие упадёт.
///
/// ViewModel:
///   - MainWindowViewModel — singleton (главное окно одно);
///   - остальные — transient.
///
/// Сервисы навигации/диалогов — singleton, без состояния.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWpf(this IServiceCollection services)
    {
        // Windows
        services.AddSingleton<MainWindow>();

        // ViewModels
        services.AddSingleton<MainWindowViewModel>();

        // Navigation & Dialogs
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<IWindowService, WindowService>();

        // View ↔ ViewModel mapping
        ViewMapping.Register<MainWindow, MainWindowViewModel>();

        return services;
    }
}
