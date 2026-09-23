namespace TimeTracker.Application;

/// <summary>
/// Сочетание клавиш: модификаторы и основная клавиша.
/// </summary>
/// <param name="Modifiers">Модификаторы сочетания.</param>
/// <param name="Key">Основная клавиша сочетания.</param>
public readonly record struct HotKey(HotKeyModifiers Modifiers, string Key);
