using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeTracker.Domain;

namespace TimeTracker.Infrastructure.Configurations;

/// <summary>
/// Отображение тега на таблицу базы данных.
/// Имя тега уникально: одинаковые теги не должны существовать порознь,
/// иначе переименование тега не затрагивало бы все задачи с этим тегом.
/// </summary>
public sealed class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    /// <summary>
    /// Настраивает таблицу, колонки и уникальный индекс имени.
    /// </summary>
    /// <param name="builder">Построитель конфигурации.</param>
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("Tags");

        builder.HasKey(tag => tag.Id);
        builder.Property(tag => tag.Id).ValueGeneratedNever();
        builder.Property(tag => tag.Name).IsRequired().HasMaxLength(50);

        builder.HasIndex(tag => tag.Name).IsUnique();
    }
}
