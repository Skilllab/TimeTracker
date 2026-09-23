using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using TimeTracker.Application;

namespace TimeTracker.Infrastructure.Windows;

/// <summary>
/// Реализация исходящего порта простоя средствами Windows.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class WindowsIdleDetector : IIdleDetector
{
    /// <summary>
    /// Возвращает время с момента последнего ввода мышью или клавиатурой.
    /// </summary>
    public TimeSpan GetIdleTime()
    {
        var info = new LastInputInfo { Size = (uint)Marshal.SizeOf<LastInputInfo>() };

        if (!GetLastInputInfo(ref info))
        {
            return TimeSpan.Zero;
        }

        var elapsed = unchecked((uint)Environment.TickCount - info.Time);

        return TimeSpan.FromMilliseconds(elapsed);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct LastInputInfo
    {
        public uint Size;

        public uint Time;
    }

    [DllImport("user32.dll")]
    private static extern bool GetLastInputInfo(ref LastInputInfo info);
}
