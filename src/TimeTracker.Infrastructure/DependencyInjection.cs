using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TimeTracker.Application.Abstractions.Idle;
using TimeTracker.Application.Abstractions.Persistence;
using TimeTracker.Application.TimeTracking;
using TimeTracker.Infrastructure.Idle;
using TimeTracker.Infrastructure.Persistence;
using TimeTracker.Infrastructure.Persistence.Interceptors;
using TimeTracker.Infrastructure.Repositories;

namespace TimeTracker.Infrastructure;

/// <summary>
/// Регистрация инфраструктурных сервисов, контекста данных и репозиториев в контейнере зависимостей
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        AppPaths.EnsureCreated();

        // Interceptor — singleton, не имеет состояния.
        services.AddSingleton<TimeEntryShadowPropertiesInterceptor>();

        // DbContext — scoped. Один на scope.
        services.AddDbContext<AppDbContext>((sp, options) =>
            options
                .UseSqlite($"Data Source={AppPaths.DatabaseFile}")
                .AddInterceptors(sp.GetRequiredService<TimeEntryShadowPropertiesInterceptor>()));

        // Репозитории — scoped, живут в одном scope с DbContext.
        services.AddScoped<ITimeEntryRepository, TimeEntryRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // TimerService — SCOPED. Зависит от scoped-репозиториев,
        // поэтому не может быть singleton. В WPF резолвится из
        // одного долгоживущего scope, что даёт «почти singleton»
        // поведение без captured dependencies.
        services.AddScoped<ITimerService, TimerService>();

        // IIdleDetector — singleton, обёртка над WinAPI без состояния.
        services.AddSingleton<IIdleDetector, IdleDetector>();

        // TimeProvider.System — singleton, стандартная реализация BCL.
        // TryAddSingleton — потому что хост (этап 4) может уже
        // зарегистрировать его.
        services.TryAddSingleton(TimeProvider.System);

        return services;
    }
}
