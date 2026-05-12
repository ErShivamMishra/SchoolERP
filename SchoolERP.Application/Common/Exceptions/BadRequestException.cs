namespace SchoolERP.Application.Common.Exceptions;

public sealed class BadRequestException(string message, string errorCode = "bad_request")
    : AppException(message, 400, errorCode);
