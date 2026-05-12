using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Authentication.Interfaces;
using SchoolERP.Application.Features.Schools.Interfaces;
using SchoolERP.Infrastructure.Options;
using SchoolERP.Infrastructure.Persistence;
using SchoolERP.Infrastructure.Repositories;
using SchoolERP.Infrastructure.Services;

namespace SchoolERP.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<AuthOptions>(configuration.GetSection(AuthOptions.SectionName));

        services.AddDbContext<SchoolErpDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IAuthPolicyProvider, AuthPolicyProvider>();
        services.AddScoped<ISchoolRepository, SchoolRepository>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuditService, AuditService>();

        return services;
    }
}
