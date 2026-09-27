namespace TimeTracker.Presentation.ViewModels;

/// <summary>
/// Пункт отбора задач по проекту: конкретный проект или вариант «Все проекты».
/// </summary>
/// <param name="Id">Идентификатор проекта; <c>null</c> — все проекты.</param>
/// <param name="Name">Надпись пункта.</param>
/// <param name="Color">Цвет маркера проекта; пустая строка, если маркера нет.</param>
public sealed record ProjectFilterItem(Guid? Id, string Name, string Color)
{
    /// <summary>
    /// Признак того, что у пункта есть цветной маркер.
    /// У варианта «Все проекты» маркера нет.
    /// </summary>
    public bool HasColor => !string.IsNullOrEmpty(Color);
}
