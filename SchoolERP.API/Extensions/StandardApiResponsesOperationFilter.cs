using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using SchoolERP.API.Common.Authorization;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SchoolERP.API.Extensions;

public sealed class StandardApiResponsesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        ApplyAuthorizationNotes(operation, context);
        AddExample(operation, "200", true, "Request completed successfully.");
        AddExample(operation, "400", false, "One or more validation errors occurred.", "Field A is required.", "Field B must be a valid email address.");
        AddExample(operation, "401", false, "Unauthorized.", "Authentication is required to access this resource.");
        AddExample(operation, "403", false, "Forbidden.", "You do not have permission to perform this action.");
        AddExample(operation, "404", false, "Resource not found.", "The requested resource was not found.");
        AddExample(operation, "500", false, "An unexpected error occurred.", "Please contact support if the problem persists.");
    }

    private static void AddExample(OpenApiOperation operation, string statusCode, bool isSuccess, string message, params string[] errors)
    {
        if (!operation.Responses.TryGetValue(statusCode, out var response))
        {
            response = new OpenApiResponse { Description = message };
            operation.Responses[statusCode] = response;
        }

        response.Content ??= new Dictionary<string, OpenApiMediaType>();

        if (!response.Content.TryGetValue("application/json", out var mediaType))
        {
            mediaType = new OpenApiMediaType();
            response.Content["application/json"] = mediaType;
        }

        mediaType.Example = isSuccess
            ? new OpenApiObject
            {
                ["success"] = new OpenApiBoolean(true),
                ["message"] = new OpenApiString(message),
                ["data"] = new OpenApiObject()
            }
            : new OpenApiObject
            {
                ["success"] = new OpenApiBoolean(false),
                ["message"] = new OpenApiString(message),
                ["errors"] = CreateErrorsArray(errors)
            };
    }

    private static OpenApiArray CreateErrorsArray(IEnumerable<string> errors)
    {
        var array = new OpenApiArray();
        foreach (var error in errors)
        {
            array.Add(new OpenApiString(error));
        }

        return array;
    }

    private static void ApplyAuthorizationNotes(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Summary ??= context.MethodInfo.Name;

        var allowAnonymous = context.MethodInfo.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any();
        var authorizeAttributes = context.MethodInfo.DeclaringType?.GetCustomAttributes(true).OfType<AuthorizeAttribute>().ToArray() ?? Array.Empty<AuthorizeAttribute>();
        authorizeAttributes = authorizeAttributes.Concat(context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>()).ToArray();
        var moduleAccessAttributes = context.MethodInfo.GetCustomAttributes(true).OfType<ModuleAccessAttribute>().ToArray();

        var descriptionParts = new List<string>();

        if (allowAnonymous)
        {
            descriptionParts.Add("Authentication: Public endpoint.");
        }
        else if (authorizeAttributes.Length > 0)
        {
            var roles = authorizeAttributes
                .Where(x => !string.IsNullOrWhiteSpace(x.Roles))
                .Select(x => x.Roles!)
                .Distinct()
                .ToArray();

            descriptionParts.Add(roles.Length > 0
                ? $"Authentication: JWT Bearer required. Roles: {string.Join("; ", roles)}."
                : "Authentication: JWT Bearer required.");
        }

        foreach (var moduleAccess in moduleAccessAttributes)
        {
            descriptionParts.Add($"Module access: `{moduleAccess.ModuleCode}` with `{moduleAccess.PermissionAction}` permission is required.");
            if (!string.IsNullOrWhiteSpace(moduleAccess.AuthorizationNote))
            {
                descriptionParts.Add(moduleAccess.AuthorizationNote!);
            }
        }

        if (descriptionParts.Count > 0)
        {
            operation.Description = string.Join("\n\n", descriptionParts);
        }
    }
}
