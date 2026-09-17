using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TimeTracker.Domain.Clients;

namespace TimeTracker.Infrastructure.Persistence.Converters;

/// <summary>
/// Маппинг ClientId в БД. Хранится как Guid
/// </summary>
public sealed class ClientIdConverter : ValueConverter<ClientId, Guid>
{
    public ClientIdConverter() : base(
        id => id.Value,
        value => ClientId.From(value))
    {
    }
}
