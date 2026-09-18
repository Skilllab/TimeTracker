using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeTracker.Domain.Tags;
using TimeTracker.Domain.TimeTracking;
using TimeTracker.Infrastructure.Persistence.Converters;

namespace TimeTracker.Infrastructure.Persistence.Configurations;

/// <summary>
/// Конфигурация маппинга TimeEntry в БД.
///
/// Range маппится как одна строка через TimeRangeConverter.
/// Дополнительно объявлены два shadow property — Range_Start_Ticks
/// и Range_End_Ticks — технические столбцы INTEGER, которые заполняет
/// TimeEntryShadowPropertiesInterceptor. По ним работает SQL-фильтрация
/// (running-записи, диапазоны дат) и индексы, потому что фильтровать
/// по строке Range через value converter EF Core не умеет.
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

        // Range — единое значение VO, хранится как строка.
        builder.Property(x => x.Range)
            .HasConversion<TimeRangeConverter>()
            .HasColumnName("Range")
            .IsRequired();

        // Shadow property: UtcTicks старта. NOT NULL.
        // Используется для фильтрации по датам и сортировки.
        // Заполняется TimeEntryShadowPropertiesInterceptor.
        builder.Property<long>("Range_Start_Ticks")
            .HasColumnName("Range_Start_Ticks")
            .IsRequired();

        // Shadow property: UtcTicks окончания или NULL для running.
        // Используется для фильтрации running/stopped.
        // Заполняется TimeEntryShadowPropertiesInterceptor.
        builder.Property<long?>("Range_End_Ticks")
            .HasColumnName("Range_End_Ticks");

        builder.HasIndex("Range_Start_Ticks");
        builder.HasIndex("Range_End_Ticks");

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

        builder.Navigation(x => x.Tags)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(x => x.DomainEvents);
    }
}
