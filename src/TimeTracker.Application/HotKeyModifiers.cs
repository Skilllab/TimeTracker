namespace TimeTracker.Application;

/// <summary>
/// Модификаторы сочетания клавиш.
/// </summary>
[Flags]
public enum HotKeyModifiers
{
    /// <summary>Без модификаторов.</summary>
    None = 0,

    /// <summary>Клавиша Alt.</summary>
    Alt = 1,

    /// <summary>Клавиша Control.</summary>
    Control = 2,

    /// <summary>Клавиша Shift.</summary>
    Shift = 4,

    /// <summary>Клавиша Windows или Command.</summary>
    Meta = 8
}
