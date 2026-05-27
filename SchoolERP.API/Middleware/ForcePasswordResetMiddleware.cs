using System.Text.Json;
using System.Text.Json.Serialization;
using SchoolERP.API.Common.Responses;
using SchoolERP.Application.Common.Interfaces;

namespace SchoolERP.API.Middleware;

public sealed class ForcePasswordResetMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ICurrentUserContext currentUserContext)
    {
        if (!currentUserContext.IsAuthenticated || !currentUserContext.RequiresPasswordReset)
        {
            await next(context);
            return;
        }

        var path = context.Request.Path.Value ?? string.Empty;
        if (path.Equals("/api/v1/auth/change-password", StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        context.Response.ContentType = "application/json";
        var response = ApiResponse<object>.Fail(
            "Password change is required before accessing this resource.",
            new[] { "Temporary-password users must change their password first." });

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        }));
    }
}
