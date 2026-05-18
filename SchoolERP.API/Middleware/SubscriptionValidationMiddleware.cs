using Microsoft.AspNetCore.Authorization;
using SchoolERP.API.Common.Authorization;
using Microsoft.EntityFrameworkCore;
using SchoolERP.API.Common.Responses;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Constants;
using SchoolERP.Domain.Enums;
using SchoolERP.Infrastructure.Persistence;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SchoolERP.API.Middleware;

public sealed class SubscriptionValidationMiddleware
{
    private readonly RequestDelegate _next;

    public SubscriptionValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        SchoolErpDbContext dbContext,
        ICurrentUserContext currentUserContext,
        IAccessControlService accessControlService)
    {
        if (!ShouldValidate(context))
        {
            await _next(context);
            return;
        }

        if (currentUserContext.Roles.Contains(RoleNames.SuperAdmin))
        {
            await _next(context);
            return;
        }

        if (!currentUserContext.SchoolId.HasValue)
        {
            await WriteForbiddenAsync(
                context,
                "School subscription context is missing.",
                "School context is missing for this request.");

            return;
        }

        var school = await dbContext.Schools
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                x => x.Id == currentUserContext.SchoolId.Value,
                context.RequestAborted);

        if (school is null || !school.IsActive || school.Status == SchoolStatus.Suspended)
        {
            await WriteForbiddenAsync(
                context,
                "School is inactive.",
                "This school is inactive.");

            return;
        }

        if (school.SubscriptionEndDateUtc < DateTime.UtcNow)
        {
            await WriteForbiddenAsync(
                context,
                "School subscription is expired.",
                "The current school subscription has expired.");

            return;
        }

        var moduleRequirements = context.GetEndpoint()?.Metadata.GetOrderedMetadata<ModuleAccessAttribute>() ?? Array.Empty<ModuleAccessAttribute>();
        foreach (var requirement in moduleRequirements)
        {
            await accessControlService.EnsureModuleAccessAsync(requirement.ModuleCode, requirement.PermissionAction, context.RequestAborted);
        }

        await _next(context);
    }

    private static bool ShouldValidate(HttpContext context)
    {
        var endpoint = context.GetEndpoint();

        if (endpoint?.Metadata.GetMetadata<IAllowAnonymous>() is not null)
        {
            return false;
        }

        return context.User.Identity?.IsAuthenticated == true;
    }

    private static async Task WriteForbiddenAsync(
        HttpContext context,
        string message,
        string error)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        context.Response.ContentType = "application/json";

        var response = ApiResponse<object>.Fail(message, new[] { error });

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(
                response,
                new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                }));
    }
}
