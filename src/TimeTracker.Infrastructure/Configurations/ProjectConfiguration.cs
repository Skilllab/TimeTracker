using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeTracker.Domain;

namespace TimeTracker.Infrastructure.Configurations;

/// <summary>
/// Отображение проекта на таблицу базы данных.
/// </summary>
public sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    /// <summary>
    /// Настраивает таблицу, колонки и индексы.
    /// </summary>
    /// <param name="builder">Построитель конфигурации.</param>
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");

        builder.HasKey(project => project.Id);
        builder.Property(project => project.Id).ValueGeneratedNever();
        builder.Property(project => project.Name).IsRequired().HasMaxLength(100);
        builder.Property(project => project.Color).IsRequired().HasMaxLength(7);
        builder.Property(project => project.IsArchived).IsRequired();

        builder.HasIndex(project => project.IsArchived);
    }
}
