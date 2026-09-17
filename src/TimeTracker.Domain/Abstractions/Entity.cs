namespace TimeTracker.Domain.Abstractions;

/// <summary>
/// Базовая сущность с identity. Equality по Id
/// </summary>
public abstract class Entity<TId> : IEquatable<Entity<TId>> where TId : struct
{
    /// <summary>
    /// Идентификатор сущности. Уникален в рамках своего типа.
    /// </summary>
    public TId Id
    {
        get; protected init;
    }

    /// <summary>
    /// Базовый конструктор. Принимает уже готовый Id — генерация Id лежит на наследнике.
    /// </summary>
    protected Entity(TId id) => Id = id;

    /// <summary>
    /// Типизированное равенство сущностей
    /// </summary>
    /// <param name="other">Другая сущность для сравнения</param>
    public bool Equals(Entity<TId>? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    /// <summary>
    /// Реализация System.Object.Equals. Тонкая обертка над типизированным
    /// Equals(Entity&lt;TId&gt;?) — нужна для случаев, когда левая сторона
    /// известна только как object (например, Object.Equals(a, b),
    /// не-generic коллекции, рефлексия).
    /// </summary>
    public override bool Equals(object? obj) => Equals(obj as Entity<TId>);

    /// <summary>
    /// Хэш-код тоже основан на Id, а не на ссылке.
    ///
    /// В комбинации с Equals это дает корректную работу сущностей
    /// в HashSet, Dictionary, LINQ Distinct/Contains/GroupBy.
    /// </summary>
    public override int GetHashCode() => HashCode.Combine(GetType(), Id);
}
