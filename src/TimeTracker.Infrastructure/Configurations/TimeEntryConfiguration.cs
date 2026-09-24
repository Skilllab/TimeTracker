using System.Globalization;
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

        builder.Property(entry => entry.StartedAt)
            .IsRequired()
            .HasConversion(
                value => value.UtcDateTime.ToString("O", CultureInfo.InvariantCulture),
                value => DateTimeOffset.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind));

        builder.Property(entry => entry.EndedAt)
            .HasConversion(
                value => value.HasValue ? value.Value.UtcDateTime.ToString("O", CultureInfo.InvariantCulture) : null,
                value => value == null
                    ? null
                    : DateTimeOffset.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind));

        builder.Property(entry => entry.PausedAt)
            .HasConversion(
                value => value.HasValue ? value.Value.UtcDateTime.ToString("O", CultureInfo.InvariantCulture) : null,
                value => value == null
                    ? null
                    : DateTimeOffset.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind));

        builder.Property(entry => entry.PausedSeconds).IsRequired();
        builder.Property(entry => entry.IsBillable).IsRequired();

        builder.HasOne<Project>()
            .WithMany()
            .HasForeignKey(entry => entry.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(entry => entry.IsOpen);

        builder.HasIndex(entry => entry.StartedAt);
        builder.HasIndex(entry => entry.ProjectId);
    }
}
