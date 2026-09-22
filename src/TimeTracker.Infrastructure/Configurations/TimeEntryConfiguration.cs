using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeTracker.Domain;

namespace TimeTracker.Infrastructure.Configurations;

/// <summary>
/// Отображение записи времени на таблицу базы данных.
/// </summary>
public sealed class TimeEntryConfiguration : IEntityTypeConfiguration<TimeEntry>
{
    /// <summary>
    /// Настраивает таблицу, колонки и индексы.
    /// </summary>
    /// <param name="builder">Построитель конфигурации.</param>
    public void Configure(EntityTypeBuilder<TimeEntry> builder)
    {
        builder.ToTable("TimeEntries");

        builder.HasKey(entry => entry.Id);
        builder.Property(entry => entry.Id).ValueGeneratedNever();
        builder.Property(entry => entry.Description).IsRequired().HasMaxLength(500);
        builder.Property(entry => entry.StartedAt).IsRequired();
        builder.Property(entry => entry.PausedSeconds).IsRequired();
        builder.Property(entry => entry.IsBillable).IsRequired();

        builder.Ignore(entry => entry.IsOpen);

        builder.HasIndex(entry => entry.StartedAt);
    }
}
