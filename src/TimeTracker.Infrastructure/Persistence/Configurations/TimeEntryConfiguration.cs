using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeTracker.Domain.Tags;
using TimeTracker.Domain.TimeTracking;
using TimeTracker.Infrastructure.Persistence.Converters;

namespace TimeTracker.Infrastructure.Persistence.Configurations;

/// <summary>
/// Конфигурация маппинга TimeEntry в БД
/// </summary>
public sealed class TimeEntryConfiguration : IEntityTypeConfiguration<TimeEntry>
{
    public void Configure(EntityTypeBuilder<TimeEntry> builder)
    {
        builder.ToTable("TimeEntries");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion<TimeEntryIdConverter>()
            .ValueGeneratedNever();

        builder.Property(x => x.Description)
            .HasMaxLength(TimeEntry.MaxDescriptionLength)
            .IsRequired();

        builder.Property(x => x.Range)
            .HasConversion<TimeRangeConverter>()
            .HasColumnName("Range")
            .IsRequired();

        builder.Property(x => x.ProjectId)
            .HasConversion<ProjectIdConverter>()
            .IsRequired(false);

        builder.Property(x => x.IsBillable)
            .IsRequired();

        builder.HasOne<Domain.Projects.Project>()
            .WithMany()
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.OwnsMany(x => x.Tags, ownedBuilder =>
        {
            ownedBuilder.ToTable("TimeEntryTags");
            ownedBuilder.WithOwner().HasForeignKey("TimeEntryId");
            ownedBuilder.HasKey("TimeEntryId", "Name");
            ownedBuilder.Property(t => t.Name)
                .HasMaxLength(Tag.MaxLength)
                .IsRequired();
        });

        // ImmutableArray<Tag> — структура, поэтому builder.Navigation(...)
        // не подходит. Вместо этого указываем EF работать через backing
        // field _tags напрямую.
        //
        // ВАЖНО: _tags НЕ readonly — EF присваивает новое значение
        // при материализации.
        builder.Metadata
            .FindNavigation(nameof(TimeEntry.Tags))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(x => x.DomainEvents);
    }
}
