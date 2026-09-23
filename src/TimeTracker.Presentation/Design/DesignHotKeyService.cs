using TimeTracker.Application;

namespace TimeTracker.Presentation.Design;

/// <summary>
/// Заглушка исходящего порта горячей клавиши для дизайнера XAML: сочетание считается зарегистрированным.
/// </summary>
internal sealed class DesignHotKeyService : IHotKeyService
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
    /// Сообщает, что сочетание удалось зарегистрировать.
    /// </summary>
    /// <param name="hotKey">Сочетание клавиш.</param>
    public bool TryRegister(HotKey hotKey) => true;

    /// <summary>
    /// Снимает регистрацию текущего сочетания.
    /// </summary>
    public void Unregister()
    {
    }
}
