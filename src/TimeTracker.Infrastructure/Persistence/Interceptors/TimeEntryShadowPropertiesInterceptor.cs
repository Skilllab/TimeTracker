using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TimeTracker.Domain.TimeTracking;

namespace TimeTracker.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Заполняет shadow properties Range_Start_Ticks и Range_End_Ticks
/// при сохранении TimeEntry.
///
/// Зачем это нужно:
///   TimeRange маппится в БД как одна строка "{startTicks}|{endTicks|null}"
///   через TimeRangeConverter. EF Core видит ее как одно свойство типа
///   string и не может транслировать в SQL выражения вида
///   Range.End == null или Range.Start >= x — он не знает, что внутри
///   строки лежит структура.
///
///   Технические столбцы Range_Start_Ticks (NOT NULL) и Range_End_Ticks
///   (NULL для running-записей) хранят UtcTicks из Range.Start и Range.End.
///   По ним работает SQL-фильтрация и индексы.
///
///   Этот interceptor вызывается перед SaveChanges и синхронизирует
///   shadow properties с текущим состоянием Range.
/// </summary>
public sealed class TimeEntryShadowPropertiesInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        UpdateShadowProperties(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateShadowProperties(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>
    /// Для каждого TimeEntry в состоянии Added или Modified
    /// выставляет актуальные значения Range_Start_Ticks и Range_End_Ticks.
    /// </summary>
    private static void UpdateShadowProperties(DbContext? context)
    {
        if (context is null)
            return;

        foreach (var entry in context.ChangeTracker.Entries<TimeEntry>())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified))
                continue;

            entry.Property("Range_Start_Ticks").CurrentValue =
                entry.Entity.Range.Start.UtcTicks;

            entry.Property("Range_End_Ticks").CurrentValue =
                entry.Entity.Range.End?.UtcTicks;
        }
    }
}
