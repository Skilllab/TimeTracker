namespace TimeTracker.Domain;

/// <summary>
/// Фиксированный набор цветов маркеров проектов, взятый из палитры токенов.
/// Значения совпадают с ролями светлой темы: акцент, успех, предупреждение, ошибка.
/// </summary>
public static class ProjectPalette
{
    /// <summary>
    /// Допустимые цвета маркеров в виде #RRGGBB.
    /// </summary>
    public static IReadOnlyList<string> Colors { get; } = new[]
    {
        "#2F6FED",
        "#2E7D32",
        "#B26A00",
        "#C62828"
    };

    /// <summary>
    /// Проверяет, что цвет входит в набор.
    /// Сравнение без учета регистра, потому что регистр букв в записи цвета не несет смысла.
    /// </summary>
    /// <param name="color">Проверяемый цвет в виде #RRGGBB.</param>
    public static bool Contains(string? color)
    {
        return color is not null && Colors.Contains(color, StringComparer.OrdinalIgnoreCase);
    }
}
