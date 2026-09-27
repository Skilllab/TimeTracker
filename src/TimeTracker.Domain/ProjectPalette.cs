namespace TimeTracker.Domain;

/// <summary>
/// Набор цветов маркеров, предлагаемый для выбора, и проверка записи цвета.
/// Значения взяты из палитры токенов и совпадают с ролями светлой темы:
/// акцент, успех, предупреждение, ошибка.
/// </summary>
public static class ProjectPalette
{
    /// <summary>
    /// Предлагаемые цвета маркеров в виде #RRGGBB.
    /// Набор не ограничивает выбор: цвет можно задать любой.
    /// </summary>
    public static IReadOnlyList<string> Colors { get; } = new[]
    {
        "#2F6FED",
        "#2E7D32",
        "#B26A00",
        "#C62828"
    };

    /// <summary>
    /// Проверяет, что цвет записан в виде #RRGGBB.
    /// Проверяется формат, а не вхождение в набор: палитра только предлагает готовые значения.
    /// </summary>
    /// <param name="color">Проверяемый цвет.</param>
    public static bool IsValid(string? color)
    {
        if (color is null || color.Length != 7 || color[0] != '#')
        {
            return false;
        }

        for (var index = 1; index < color.Length; index++)
        {
            if (!Uri.IsHexDigit(color[index]))
            {
                return false;
            }
        }

        return true;
    }
}
