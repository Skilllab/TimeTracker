using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TimeTracker.Infrastructure.Persistence;

/// <summary>
/// Design-time factory для dotnet ef
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        AppPaths.EnsureCreated();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={AppPaths.DatabaseFile}")
            .Options;

        return new AppDbContext(options);
    }
}
