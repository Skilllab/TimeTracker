using System.Runtime.InteropServices;
using TimeTracker.Application.Abstractions.Idle;

namespace TimeTracker.Infrastructure.Idle;

/// <summary>
/// Определение времени бездействия пользователя через WinAPI
/// GetLastInputInfo. Работает только на Windows.
/// </summary>
public sealed class IdleDetector : IIdleDetector
{
    [DllImport("user32.dll")]
    private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

    [StructLayout(LayoutKind.Sequential)]
    private struct LASTINPUTINFO
    {
        public uint cbSize;
        public uint dwTime;
    }

    public TimeSpan GetIdleTime()
    {
        var info = new LASTINPUTINFO
        {
            cbSize = (uint)Marshal.SizeOf<LASTINPUTINFO>()
        };

        if (!GetLastInputInfo(ref info))
            return TimeSpan.Zero;

        var idleMs = Environment.TickCount64 - info.dwTime;
        return idleMs <= 0 ? TimeSpan.Zero : TimeSpan.FromMilliseconds(idleMs);
    }
}
