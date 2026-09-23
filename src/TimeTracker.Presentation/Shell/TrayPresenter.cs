using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace TimeTracker.Presentation.Shell;

/// <summary>
/// Значок в трее: показывает окно и управляет выходом из приложения.
/// </summary>
public sealed class TrayPresenter : IDisposable
{
    private readonly LocalizationManager _localizationManager;
    private readonly NativeMenuItem _showItem;
    private readonly NativeMenuItem _exitItem;
    private TrayIcon? _icon;

    /// <summary>
    /// Создает значок в трее.
    /// </summary>
    /// <param name="localizationManager">Управление языком.</param>
    public TrayPresenter(LocalizationManager localizationManager)
    {
        _localizationManager = localizationManager ?? throw new ArgumentNullException(nameof(localizationManager));

        _showItem = new NativeMenuItem(_localizationManager["Tray.Show"]);
        _exitItem = new NativeMenuItem(_localizationManager["Tray.Exit"]);

        _localizationManager.Changed += OnLanguageChanged;
    }

    /// <summary>
    /// Событие выхода из приложения.
    /// </summary>
    public event EventHandler? ExitRequested;

    /// <summary>
    /// Подключает значок к окну.
    /// </summary>
    /// <param name="window">Главное окно.</param>
    public void Attach(Window window)
    {
        _showItem.Click += (_, _) => Show(window);
        _exitItem.Click += (_, _) => ExitRequested?.Invoke(this, EventArgs.Empty);

        var menu = new NativeMenu();
        menu.Items.Add(_showItem);
        menu.Items.Add(_exitItem);

        _icon = new TrayIcon
        {
            Icon = new WindowIcon(AssetLoader.Open(new Uri("avares://TimeTracker.Presentation/Assets/app.ico"))),
            ToolTipText = _localizationManager["App.Title"],
            Menu = menu,
            IsVisible = true
        };

        _icon.Clicked += (_, _) => Show(window);
    }

    /// <summary>
    /// Освобождает значок в трее.
    /// </summary>
    public void Dispose()
    {
        _localizationManager.Changed -= OnLanguageChanged;

        if (_icon is not null)
        {
            _icon.IsVisible = false;
            _icon.Dispose();
            _icon = null;
        }
    }

    /// <summary>
    /// Показывает и активирует окно, если оно свернуто.
    /// </summary>
    /// <param name="window">Главное окно.</param>
    private static void Show(Window window)
    {
        window.Show();

        if (window.WindowState == WindowState.Minimized)
        {
            window.WindowState = WindowState.Normal;
        }

        window.Activate();
    }

    /// <summary>
    /// Обновляет подписи меню после смены языка.
    /// </summary>
    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        _showItem.Header = _localizationManager["Tray.Show"];
        _exitItem.Header = _localizationManager["Tray.Exit"];
    }
}
