using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TimeTracker.Application.Abstractions.Persistence;
using TimeTracker.Infrastructure.Persistence;
using TimeTracker.Infrastructure.Persistence.Interceptors;
using TimeTracker.Infrastructure.Repositories;

namespace TimeTracker.Infrastructure;

/// <summary>
/// Регистрация Infrastructure-сервисов в DI
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        AppPaths.EnsureCreated();

        services.AddSingleton<TimeEntryShadowPropertiesInterceptor>();

        services.AddDbContext<AppDbContext>((sp, options) =>
            options
                .UseSqlite($"Data Source={AppPaths.DatabaseFile}")
                .AddInterceptors(sp.GetRequiredService<TimeEntryShadowPropertiesInterceptor>()));

        services.AddScoped<ITimeEntryRepository, TimeEntryRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.TryAddSingleton(TimeProvider.System);

        return services;
    }
}
