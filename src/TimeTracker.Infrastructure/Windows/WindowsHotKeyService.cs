using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using TimeTracker.Application;

namespace TimeTracker.Infrastructure.Windows;

/// <summary>
/// Реализация исходящего порта горячей клавиши средствами Windows.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class WindowsHotKeyService : IHotKeyService, IDisposable
{
    private const int HotKeyIdentifier = 0x5441;
    private const uint HotKeyMessage = 0x0312;
    private const uint NoRepeat = 0x4000;
    private const uint RemoveMessage = 0x0001;

    private readonly TimeProvider _timeProvider;
    private ITimer? _pump;

    /// <summary>
    /// Создает сервис.
    /// </summary>
    /// <param name="timeProvider">Источник времени для опроса сообщений.</param>
    public WindowsHotKeyService(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    /// <summary>
    /// Событие нажатия зарегистрированной клавиши.
    /// </summary>
    public event EventHandler? Pressed;

    /// <summary>
    /// Регистрирует сочетание клавиш, заменяя прежнее.
    /// </summary>
    /// <param name="hotKey">Сочетание клавиш.</param>
    public bool TryRegister(HotKey hotKey)
    {
        Unregister();

        var registered = RegisterHotKey(
            IntPtr.Zero,
            HotKeyIdentifier,
            (uint)hotKey.Modifiers | NoRepeat,
            (uint)KeyCode(hotKey.Key));

        if (!registered)
        {
            return false;
        }

        _pump ??= _timeProvider.CreateTimer(
            _ => Pump(),
            null,
            TimeSpan.FromMilliseconds(200),
            TimeSpan.FromMilliseconds(200));

        return true;
    }

    /// <summary>
    /// Снимает регистрацию текущего сочетания.
    /// </summary>
    public void Unregister() => UnregisterHotKey(IntPtr.Zero, HotKeyIdentifier);

    /// <summary>
    /// Прекращает опрос сообщений и снимает регистрацию.
    /// </summary>
    public void Dispose()
    {
        Unregister();

        _pump?.Dispose();
        _pump = null;
    }

    /// <summary>
    /// Забирает сообщения о нажатии клавиши из очереди потока.
    /// </summary>
    private void Pump()
    {
        while (PeekMessage(out var message, IntPtr.Zero, 0, 0, RemoveMessage))
        {
            if (message.MessageId == HotKeyMessage && message.WParam == (IntPtr)HotKeyIdentifier)
            {
                Pressed?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    /// <summary>
    /// Переводит имя клавиши в ее код.
    /// </summary>
    /// <param name="key">Имя клавиши, например F9.</param>
    private static int KeyCode(string key) => key.ToUpperInvariant() switch
    {
        "F1" => 0x70,
        "F2" => 0x71,
        "F3" => 0x72,
        "F4" => 0x73,
        "F5" => 0x74,
        "F6" => 0x75,
        "F7" => 0x76,
        "F8" => 0x77,
        "F9" => 0x78,
        "F10" => 0x79,
        "F11" => 0x7A,
        "F12" => 0x7B,
        _ => 0
    };

    [StructLayout(LayoutKind.Sequential)]
    private struct Message
    {
        public IntPtr Handle;

        public uint MessageId;

        public IntPtr WParam;

        public IntPtr LParam;

        public uint Time;

        public int X;

        public int Y;
    }

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr window, int id, uint modifiers, uint key);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr window, int id);

    [DllImport("user32.dll")]
    private static extern bool PeekMessage(out Message message, IntPtr window, uint filterMin, uint filterMax, uint remove);
}
