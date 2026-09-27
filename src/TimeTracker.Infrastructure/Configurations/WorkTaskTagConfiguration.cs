using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeTracker.Domain;

namespace TimeTracker.Infrastructure.Configurations;

/// <summary>
/// Отображение связи задачи с тегом на таблицу базы данных.
/// </summary>
public sealed class WorkTaskTagConfiguration : IEntityTypeConfiguration<WorkTaskTag>
{
    /// <summary>
    /// Настраивает таблицу, составной ключ и внешние ссылки.
    /// Связь удаляется вместе с задачей или тегом: сами задачи и теги не удаляются,
    /// поэтому каскад затрагивает только строки связи.
    /// </summary>
    /// <param name="builder">Построитель конфигурации.</param>
    public void Configure(EntityTypeBuilder<WorkTaskTag> builder)
    {
        builder.ToTable("WorkTaskTags");

        builder.HasKey(link => new { link.TaskId, link.TagId });

        builder.HasOne<WorkTask>()
            .WithMany()
            .HasForeignKey(link => link.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Tag>()
            .WithMany()
            .HasForeignKey(link => link.TagId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
