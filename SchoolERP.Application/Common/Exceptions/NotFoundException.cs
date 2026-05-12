namespace SchoolERP.Application.Common.Exceptions;

public sealed class NotFoundException(string message, string errorCode = "not_found")
    : AppException(message, 404, errorCode);
