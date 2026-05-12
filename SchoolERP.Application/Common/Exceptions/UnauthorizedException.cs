namespace SchoolERP.Application.Common.Exceptions;

public sealed class UnauthorizedException(string message, string errorCode = "unauthorized")
    : AppException(message, 401, errorCode);
