using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Microsoft.Extensions.Logging;

namespace TimeTracker.Wpf.Services;

/// <summary>
/// Реализация IHotKeyService через WinAPI RegisterHotKey.
///
/// Хоткей Ctrl+Shift+Space регистрируется на HWND окна.
/// Обработка WM_HOTKEY — через HwndSource hook.
///
/// Важно: HWND должен существовать на момент регистрации.
/// Поэтому Register вызывается после MainWindow.Show().
/// </summary>
public sealed class HotKeyService : IHotKeyService
{
    private const int HotKeyId = 9000;
    private const int WmHotKey = 0x0312;

    private const uint ModControl = 0x0002;
    private const uint ModShift = 0x0004;
    private const uint VkSpace = 0x20;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private readonly ILogger<HotKeyService> _logger;
    private HwndSource? _source;
    private IntPtr _handle;

    public event EventHandler? ToggleRequested;

    public HotKeyService(ILogger<HotKeyService> logger)
    {
        _logger = logger;
    }

    public void Register(Window window)
    {
        var helper = new WindowInteropHelper(window);
        _handle = helper.EnsureHandle();
        _source = HwndSource.FromHwnd(_handle);
        _source?.AddHook(WndProc);

        if (!RegisterHotKey(_handle, HotKeyId, ModControl | ModShift, VkSpace))
        {
            _logger.LogWarning("Failed to register hotkey Ctrl+Shift+Space");
            return;
        }

        _logger.LogInformation("Hotkey Ctrl+Shift+Space registered");
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WmHotKey && wParam.ToInt32() == HotKeyId)
        {
            ToggleRequested?.Invoke(this, EventArgs.Empty);
            handled = true;
        }

        return IntPtr.Zero;
    }

    public void Dispose()
    {
        if (_handle != IntPtr.Zero)
            UnregisterHotKey(_handle, HotKeyId);

        _source?.RemoveHook(WndProc);
        _source?.Dispose();
    }
}
