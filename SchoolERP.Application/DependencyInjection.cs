using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SchoolERP.Application.Features.Authentication.Interfaces;
using SchoolERP.Application.Features.Authentication.Services;
using SchoolERP.Application.Features.AccessControl.Interfaces;
using SchoolERP.Application.Features.AccessControl.Services;
using SchoolERP.Application.Features.Modules.Interfaces;
using SchoolERP.Application.Features.Modules.Services;
using SchoolERP.Application.Features.Schools.Interfaces;
using SchoolERP.Application.Features.Schools.Services;
using SchoolERP.Application.Features.Staff.Interfaces;
using SchoolERP.Application.Features.Staff.Services;
using SchoolERP.Application.Features.Subscriptions.Interfaces;
using SchoolERP.Application.Features.Subscriptions.Services;

namespace SchoolERP.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPermissionManagementService, PermissionManagementService>();
        services.AddScoped<IModuleService, ModuleService>();
        services.AddScoped<ISchoolService, SchoolService>();
        services.AddScoped<IStaffService, StaffService>();
        services.AddScoped<ISubscriptionPlanService, SubscriptionPlanService>();

        return services;
    }
}
