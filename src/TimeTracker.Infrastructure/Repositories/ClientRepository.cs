using Microsoft.EntityFrameworkCore;
using TimeTracker.Application.Abstractions.Persistence;
using TimeTracker.Domain.Clients;
using TimeTracker.Infrastructure.Persistence;

namespace TimeTracker.Infrastructure.Repositories;

/// <summary>
/// EF Core реализация IClientRepository
/// </summary>
public sealed class ClientRepository(AppDbContext db) : IClientRepository
{
    public Task<Client?> GetByIdAsync(ClientId id, CancellationToken ct = default)
        => db.Clients.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<Client>> GetAllAsync(CancellationToken ct = default)
        => await db.Clients
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Client>> GetActiveAsync(CancellationToken ct = default)
        => await db.Clients
            .AsNoTracking()
            .Where(c => !c.IsArchived)
            .OrderBy(c => c.Name)
            .ToListAsync(ct);

    public async Task AddAsync(Client client, CancellationToken ct = default)
        => await db.Clients.AddAsync(client, ct);

    public void Remove(Client client) => db.Clients.Remove(client);
}
