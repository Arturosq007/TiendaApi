using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TiendaApi.Domain.Interfaces;
using TiendaApi.Infrastructure.Data;
using TiendaApi.Infrastructure.Data.Seed;
using TiendaApi.Infrastructure.Repositories;
using TiendaApi.Infrastructure.Security;

namespace TiendaApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddDatabase(configuration)
            .AddRepositories()
            .AddSecurity(configuration);

        return services;
    }

    // ─── Base de datos ────────────────────────────────────────────────────────

    private static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null
                    );
                }
            );
        });

        // El seeder se registra como Scoped — una instancia por request
        services.AddScoped<DataSeeder>();

        return services;
    }

    // ─── Repositorios ─────────────────────────────────────────────────────────

    private static IServiceCollection AddRepositories(
        this IServiceCollection services)
    {
        // UnitOfWork — agrupa todos los repositorios
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    // ─── Seguridad ────────────────────────────────────────────────────────────

    private static IServiceCollection AddSecurity(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Mapea la sección JwtSettings del appsettings.json
        // a la clase JwtSettings
        services.Configure<JwtSettings>(
            configuration.GetSection("JwtSettings"));

        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        return services;
    }
}