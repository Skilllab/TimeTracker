using TimeTracker.Application;

namespace TimeTracker.Presentation.Shell;

/// <summary>
/// Настройка горячей клавиши: текущее сочетание и попытка его зарегистрировать.
/// </summary>
public sealed class HotKeySettings
{
    /// <summary>
    /// Сочетание по умолчанию: Control, Alt и F9.
    /// </summary>
    public static readonly HotKey Default = new(HotKeyModifiers.Control | HotKeyModifiers.Alt, "F9");

    private readonly IHotKeyService _hotKeyService;

    /// <summary>
    /// Создает настройку.
    /// </summary>
    /// <param name="hotKeyService">Исходящий порт горячей клавиши.</param>
    public HotKeySettings(IHotKeyService hotKeyService)
    {
        _hotKeyService = hotKeyService ?? throw new ArgumentNullException(nameof(hotKeyService));

        Current = Default;
    }

    /// <summary>
    /// Текущее сочетание.
    /// </summary>
    public HotKey Current { get; private set; }

    /// <summary>
    /// Признак того, что сочетание удалось зарегистрировать.
    /// </summary>
    public bool IsRegistered { get; private set; }

    /// <summary>
    /// Пытается зарегистрировать сочетание по умолчанию.
    /// </summary>
    public bool RegisterDefault()
    {
        IsRegistered = _hotKeyService.TryRegister(Default);

        return IsRegistered;
    }
}
