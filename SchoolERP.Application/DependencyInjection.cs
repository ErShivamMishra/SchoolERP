using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SchoolERP.Application.Features.Authentication.Interfaces;
using SchoolERP.Application.Features.Authentication.Services;
using SchoolERP.Application.Features.Schools.Interfaces;
using SchoolERP.Application.Features.Schools.Services;

namespace SchoolERP.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ISchoolService, SchoolService>();

        return services;
    }
}
