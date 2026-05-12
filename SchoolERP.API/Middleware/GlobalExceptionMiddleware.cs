using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using SchoolERP.API.Common.Responses;
using SchoolERP.Application.Common.Exceptions;

namespace SchoolERP.API.Middleware;

public sealed class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled exception for request {TraceId}", context.TraceIdentifier);
            await HandleExceptionAsync(context, exception);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = exception is AppException appException
            ? appException.StatusCode
            : exception is ValidationException
                ? (int)HttpStatusCode.BadRequest
            : exception is JsonException
                ? (int)HttpStatusCode.BadRequest
            : (int)HttpStatusCode.InternalServerError;

        var errors = exception is ValidationException validationException
            ? validationException.Errors.Select(x => x.ErrorMessage).Distinct().ToArray()
            : exception is JsonException
                ? new[] { "Request body is invalid or malformed." }
            : exception is AppException
                ? Array.Empty<string>()
            : null;

        var message = exception is ValidationException
            ? "One or more validation errors occurred."
            : exception is JsonException
                ? "Invalid request payload."
            : statusCode == (int)HttpStatusCode.InternalServerError
                ? "An unexpected error occurred."
            : exception.Message;

        var response = ApiResponse<object>.Fail(message, errors);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        }));
    }
}
