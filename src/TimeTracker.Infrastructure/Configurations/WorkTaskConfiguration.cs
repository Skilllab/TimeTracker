using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeTracker.Domain;

namespace TimeTracker.Infrastructure.Configurations;

/// <summary>
/// Отображение задачи на таблицу базы данных.
/// Моменты хранятся в UTC строкой, поэтому сравнение и сортировка не зависят от часового пояса.
/// Имена тегов хранятся строкой прямо в задаче: отдельного справочника и таблицы связи нет.
/// </summary>
public sealed class WorkTaskConfiguration : IEntityTypeConfiguration<WorkTask>
{
    /// <summary>
    /// Настраивает таблицу, колонки и индексы.
    /// </summary>
    /// <param name="builder">Построитель конфигурации.</param>
    public void Configure(EntityTypeBuilder<WorkTask> builder)
    {
        builder.ToTable("WorkTasks");

        builder.HasKey(task => task.Id);
        builder.Property(task => task.Id).ValueGeneratedNever();
        builder.Property(task => task.Description).IsRequired().HasMaxLength(500);
        builder.Property(task => task.Status).IsRequired();
        builder.Property(task => task.PausedSeconds).IsRequired();
        builder.Property(task => task.IsBillable).IsRequired();
        builder.Property(task => task.IsDeleted).IsRequired().HasDefaultValue(false);

        builder.Property(task => task.CreatedAt)
            .IsRequired()
            .HasConversion(
                value => value.UtcDateTime.ToString("O", CultureInfo.InvariantCulture),
                value => DateTimeOffset.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind));

        builder.Property(task => task.StartedAt)
            .HasConversion(
                value => value.HasValue ? value.Value.UtcDateTime.ToString("O", CultureInfo.InvariantCulture) : null,
                value => value == null
                    ? null
                    : DateTimeOffset.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind));

        builder.Property(task => task.LastStartedAt)
            .HasConversion(
                value => value.HasValue ? value.Value.UtcDateTime.ToString("O", CultureInfo.InvariantCulture) : null,
                value => value == null
                    ? null
                    : DateTimeOffset.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind));

        builder.Property(task => task.FinishedAt)
            .HasConversion(
                value => value.HasValue ? value.Value.UtcDateTime.ToString("O", CultureInfo.InvariantCulture) : null,
                value => value == null
                    ? null
                    : DateTimeOffset.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind));

        builder.Property(task => task.PausedAt)
            .HasConversion(
                value => value.HasValue ? value.Value.UtcDateTime.ToString("O", CultureInfo.InvariantCulture) : null,
                value => value == null
                    ? null
                    : DateTimeOffset.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind));

        builder.HasOne<Project>()
            .WithMany()
            .HasForeignKey(task => task.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(task => task.Tags).IsRequired().HasMaxLength(500);

        builder.Ignore(task => task.TagNames);
        builder.Ignore(task => task.Labels);

        builder.HasIndex(task => task.LastStartedAt);
        builder.HasIndex(task => task.Status);
        builder.HasIndex(task => task.ProjectId);
        builder.HasIndex(task => task.IsDeleted);
    }
}
