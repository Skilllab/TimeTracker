using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeTracker.Domain.Projects;
using TimeTracker.Infrastructure.Persistence.Converters;

namespace TimeTracker.Infrastructure.Persistence.Configurations;

/// <summary>
/// Конфигурация маппинга Project в БД
/// </summary>
public sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion<ProjectIdConverter>()
            .ValueGeneratedNever();

        builder.Property(x => x.Name)
            .HasMaxLength(Project.MaxNameLength)
            .IsRequired();

        builder.Property(x => x.Color)
            .HasConversion<ProjectColorConverter>()
            .HasMaxLength(7)
            .IsRequired();

        builder.Property(x => x.ClientId)
            .HasConversion<ClientIdConverter>()
            .IsRequired(false);

        builder.Property(x => x.IsArchived)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasOne<Domain.Clients.Client>()
            .WithMany()
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.IsArchived);
        builder.HasIndex(x => x.Name);

        builder.Ignore(x => x.DomainEvents);
    }
}
