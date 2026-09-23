using TimeTracker.Application;

namespace TimeTracker.Infrastructure;

/// <summary>
/// Заглушка исходящего порта горячей клавиши для систем без поддержки глобальных сочетаний.
/// </summary>
public sealed class NoopHotKeyService : IHotKeyService
{
    /// <summary>
    /// Событие нажатия зарегистрированной клавиши.
    /// </summary>
    public event EventHandler? Pressed
    {
        add { }
        remove { }
    }

    /// <summary>
    /// Сообщает, что сочетание зарегистрировать не удалось.
    /// </summary>
    /// <param name="hotKey">Сочетание клавиш.</param>
    public bool TryRegister(HotKey hotKey) => false;

    /// <summary>
    /// Снимает регистрацию текущего сочетания.
    /// </summary>
    public void Unregister()
    {
    }
}
