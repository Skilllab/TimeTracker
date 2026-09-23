namespace TimeTracker.Application;

/// <summary>
/// Исходящий порт: глобальная горячая клавиша, работающая вне окна приложения.
/// </summary>
public interface IHotKeyService
{
    /// <summary>
    /// Событие нажатия зарегистрированной клавиши.
    /// </summary>
    event EventHandler? Pressed;

    /// <summary>
    /// Регистрирует сочетание клавиш, заменяя прежнее.
    /// </summary>
    /// <param name="hotKey">Сочетание клавиш.</param>
    bool TryRegister(HotKey hotKey);

    /// <summary>
    /// Снимает регистрацию текущего сочетания.
    /// </summary>
    void Unregister();
}
