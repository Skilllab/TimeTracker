using Avalonia.Controls;
using Avalonia.Threading;
using TimeTracker.Application;

namespace TimeTracker.Presentation.Views;

/// <summary>
/// Главное окно приложения: показывает счетчик в формате MM:SS.
/// </summary>
public partial class MainWindow : Window
{
    private readonly ITimerService _timerService;

    /// <summary>Тикер интерфейса: только перерисовка, время он не считает.</summary>
    private readonly DispatcherTimer _ticker = new() { Interval = TimeSpan.FromSeconds(1) };

    /// <summary>
    /// Создает главное окно.
    /// </summary>
    /// <param name="timerService">Порт источника времени; реализацию подставляет composition root.</param>
    public MainWindow(ITimerService timerService)
    {
        _timerService = timerService ?? throw new ArgumentNullException(nameof(timerService));

        InitializeComponent();

        _ticker.Tick += OnTick;
        _ticker.Start();

        RefreshCounter();
    }

    /// <summary>
    /// Перерисовывает счетчик раз в секунду.
    /// </summary>
    private void OnTick(object? sender, EventArgs e) => RefreshCounter();

    /// <summary>
    /// Берет длительность у порта и форматирует ее как MM:SS.
    /// </summary>
    private void RefreshCounter()
    {
        CounterText.Text = _timerService.GetElapsed().ToClockString();
    }
}
