using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Authentication.Interfaces;
using SchoolERP.Application.Features.AccessControl.Interfaces;
using SchoolERP.Application.Features.Modules.Interfaces;
using SchoolERP.Application.Features.Schools.Interfaces;
using SchoolERP.Application.Features.Staff.Interfaces;
using SchoolERP.Application.Features.Subscriptions.Interfaces;
using SchoolERP.Infrastructure.Options;
using SchoolERP.Infrastructure.Persistence;
using SchoolERP.Infrastructure.Repositories;
using SchoolERP.Infrastructure.Services;
using System.Text.Json;

namespace SchoolERP.Infrastructure;

public static class DependencyInjection
{
    private const string DefaultLocalDbConnectionString = "Server=(localdb)\\MSSQLLocalDB;Database=SchoolERPDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<AuthOptions>(configuration.GetSection(AuthOptions.SectionName));

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            connectionString = TryReadConnectionStringFromFile(Path.Combine(AppContext.BaseDirectory, "appsettings.json"));

            if (string.IsNullOrWhiteSpace(connectionString) && !string.IsNullOrWhiteSpace(environmentName))
            {
                connectionString = TryReadConnectionStringFromFile(Path.Combine(AppContext.BaseDirectory, $"appsettings.{environmentName}.json"));
            }
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = DefaultLocalDbConnectionString;
        }

        services.AddDbContext<SchoolErpDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IAuthPolicyProvider, AuthPolicyProvider>();
        services.AddScoped<IAccessControlService, AccessControlService>();
        services.AddScoped<IModuleRepository, ModuleRepository>();
        services.AddScoped<IPermissionManagementRepository, PermissionManagementRepository>();
        services.AddScoped<ISchoolRepository, SchoolRepository>();
        services.AddScoped<IStaffRepository, StaffRepository>();
        services.AddScoped<ISubscriptionPlanRepository, SubscriptionPlanRepository>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuditService, AuditService>();

        return services;
    }

    private static string? TryReadConnectionStringFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return null;
        }

        using var document = JsonDocument.Parse(File.ReadAllText(filePath));
        if (!document.RootElement.TryGetProperty("ConnectionStrings", out var connectionStrings))
        {
            return null;
        }

        if (!connectionStrings.TryGetProperty("DefaultConnection", out var connectionString))
        {
            return null;
        }

        return connectionString.GetString();
    }
}
