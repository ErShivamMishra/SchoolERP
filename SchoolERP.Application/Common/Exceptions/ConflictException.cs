namespace SchoolERP.Application.Common.Exceptions;

public sealed class ConflictException(string message, string errorCode = "conflict")
    : AppException(message, 409, errorCode);
