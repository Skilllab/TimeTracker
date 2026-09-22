using System.Threading;
using System.Threading.Tasks;

namespace TimeTracker.Application;

/// <summary>
/// Исходящий порт: единица работы, фиксирующая изменения хранилища.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Сохраняет изменения, сделанные за время работы.
    /// </summary>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
