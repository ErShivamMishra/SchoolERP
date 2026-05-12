namespace SchoolERP.Application.Common.Exceptions;

public sealed class ForbiddenException(string message, string errorCode = "forbidden")
    : AppException(message, 403, errorCode);
