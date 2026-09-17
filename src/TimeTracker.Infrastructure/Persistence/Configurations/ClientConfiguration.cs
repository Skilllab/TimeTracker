using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeTracker.Domain.Clients;
using TimeTracker.Infrastructure.Persistence.Converters;

namespace TimeTracker.Infrastructure.Persistence.Configurations;

/// <summary>
/// Конфигурация маппинга Client в БД
/// </summary>
public sealed class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("Clients");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion<ClientIdConverter>()
            .ValueGeneratedNever();

        builder.Property(x => x.Name)
            .HasMaxLength(Client.MaxNameLength)
            .IsRequired();

        builder.Property(x => x.IsArchived)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => x.IsArchived);

        builder.Ignore(x => x.DomainEvents);
    }
}
