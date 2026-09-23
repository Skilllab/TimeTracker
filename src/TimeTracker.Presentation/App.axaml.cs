using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using TimeTracker.Application;
using TimeTracker.Presentation.Shell;
using AvaloniaApplication = Avalonia.Application;

namespace TimeTracker.Presentation;

/// <summary>
/// Корневой класс приложения Avalonia.
/// </summary>
public partial class App : AvaloniaApplication
{
    private readonly Func<Window> _mainWindowFactory;
    private readonly ThemeManager _themeManager;
    private readonly LocalizationManager _localizationManager;
    private readonly TrayPresenter _trayPresenter;
    private readonly IdleWatcher _idleWatcher;

    /// <summary>
    /// Создает приложение.
    /// </summary>
    /// <param name="mainWindowFactory">Фабрика главного окна.</param>
    /// <param name="themeManager">Управление темой.</param>
    /// <param name="localizationManager">Управление языком.</param>
    /// <param name="trayPresenter">Представитель трей-иконки.</param>
    /// <param name="idleWatcher">Наблюдение за простоем.</param>
    public App(Func<Window> mainWindowFactory, ThemeManager themeManager, LocalizationManager localizationManager, TrayPresenter trayPresenter, IdleWatcher idleWatcher)
    {
        _mainWindowFactory = mainWindowFactory ?? throw new ArgumentNullException(nameof(mainWindowFactory));
        _themeManager = themeManager ?? throw new ArgumentNullException(nameof(themeManager));
        _localizationManager = localizationManager ?? throw new ArgumentNullException(nameof(localizationManager));
        _trayPresenter = trayPresenter ?? throw new ArgumentNullException(nameof(trayPresenter));
        _idleWatcher = idleWatcher ?? throw new ArgumentNullException(nameof(idleWatcher));

        _themeManager.Changed += OnThemeChanged;
        _localizationManager.Changed += OnLanguageChanged;
    }

    /// <summary>
    /// Загружает XAML приложения, применяет выбранную тему и язык.
    /// </summary>
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        ApplyTheme(_themeManager.Current);
        ApplyLanguage(_localizationManager.Language);

#if DEBUG
        this.AttachDeveloperTools();
#endif
    }

    /// <summary>
    /// Применяет тему к приложению после ее смены.
    /// </summary>
    private void OnThemeChanged(object? sender, EventArgs e) => ApplyTheme(_themeManager.Current);

    /// <summary>
    /// Подменяет словарь строк после смены языка.
    /// </summary>
    private void OnLanguageChanged(object? sender, EventArgs e) => ApplyLanguage(_localizationManager.Language);

    /// <summary>
    /// Задает вариант темы приложения.
    /// </summary>
    /// <param name="theme">Выбранная тема.</param>
    private void ApplyTheme(AppTheme theme)
    {
        RequestedThemeVariant = theme switch
        {
            AppTheme.Light => ThemeVariant.Light,
            AppTheme.Dark => ThemeVariant.Dark,
            _ => ThemeVariant.Default
        };
    }

    /// <summary>
    /// Подключает словарь строк выбранного языка.
    /// </summary>
    /// <param name="language">Выбранный язык.</param>
    private void ApplyLanguage(AppLanguage language)
    {
        var source = language == AppLanguage.English
            ? "avares://TimeTracker.Presentation/Themes/Strings.en.axaml"
            : "avares://TimeTracker.Presentation/Themes/Strings.ru.axaml";

        var dictionary = (ResourceDictionary)AvaloniaXamlLoader.Load(new Uri(source));

        var strings = Resources.MergedDictionaries
            .OfType<ResourceDictionary>()
            .FirstOrDefault(item => item.TryGetResource("App.Title", null, out _));

        if (strings is not null)
        {
            var index = Resources.MergedDictionaries.IndexOf(strings);
            Resources.MergedDictionaries[index] = dictionary;
        }
    }

    /// <summary>
    /// Создает главное окно после инициализации платформы.
    /// </summary>
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var window = _mainWindowFactory();

            window.Closing += OnWindowClosing;
            desktop.MainWindow = window;
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            _trayPresenter.Attach(window);
            _trayPresenter.ExitRequested += (_, _) =>
            {
                _idleWatcher.Dispose();
                _trayPresenter.Dispose();
                desktop.Shutdown();
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>
    /// Сворачивает окно в трей вместо завершения приложения.
    /// </summary>
    private static void OnWindowClosing(object? sender, WindowClosingEventArgs e)
    {
        e.Cancel = true;

        if (sender is Window window)
        {
            window.Hide();
        }
    }

}
